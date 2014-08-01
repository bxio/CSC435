/* LLVM.cs
 * 
 * Utility code to help with outputting intermediate code in the
 * LLVM text format (as a '.ll' file).
 * 
 * Author: Nigel Horspool
 * Date: July 2014
 */
 
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;


namespace FrontEnd
{
    // This datatype provides a <type, value> pair as used by LLVM
    public class LLVMValue {
        public bool IsReference { get; set; }
        public string LLType{ get; set; }
        public string LLValue{ get; set; }
        public LLVMValue( string t, string v, bool isref ) {
            LLType = t; LLValue = v; IsReference = isref;
        }
        public override string ToString() { return LLType + " " + LLValue; }
    }

    public partial class LLVM
    {
        const char leftBrace = '{';
        const char rightBrace = '}';
        int ptrSize = 64;
        int ptrAlign = 8;
        string targetTriple;
        IDictionary<string, int> SSANumbering = new Dictionary<string,int>();
        int nextBBNumber = 0;

        StreamWriter ll = null;

        public LLVM( string llFileName, string targetTriple ) {
            this.targetTriple = targetTriple;
            try {
                ll = new StreamWriter(llFileName);
                switch(targetTriple) {
                case "i686-pc-mingw32":
                    ll.WriteLine(preamble32);  ptrSize = 32;  ptrAlign = 4;
                    break;
                case "x86_64-unknown-linux-gnu":
                    ll.WriteLine(preamble64);  ptrSize = 64;  ptrAlign = 8;
                    break;
                default:
                    Console.WriteLine("Unsupported triple: {0}", targetTriple);
                    break;
                }
                ll.WriteLine("target triple = \"{0}\"\n", targetTriple);
                WritePredefinedCode();
            } catch(Exception e) {
                Console.WriteLine("Unable to write to file {0}\n\n{1}\n",
                    llFileName, e.Message);
                System.Environment.Exit(1);
            }
        }

        public void Dispose() {
            string s = (targetTriple == "i686-pc-mingw32")? epilog32 : epilog64;
            ll.Write(s);
            ll.Close();
            ll = null;
        }

        public string GetTypeDescr(CbBasic bt)
        {
            if (bt == CbType.Int) return "i32";
            if (bt == CbType.Char) return "i8";
            if (bt == CbType.Bool) return "i1";
            if (bt == CbType.Void) return "void";
            if (bt == CbType.Null) return "i8*";
            throw new Exception("unknown basic type");
        }

        public string GetTypeDescr(CFArray bt)
        {
            return GetTypeDescr(bt.ElementType)+"*";
        }

        public string GetTypeDescr(CbClass bt)
        {
            // a typename of the form %classname*
            // Note: it really *should* be %namespace.classname*
            return "%" + bt.Name + "*";
        }

        public string GetTypeDescr(CbType t)
        {
            if (t is CbBasic) return GetTypeDescr((CbBasic)t);
            if (t is CFArray) return GetTypeDescr((CFArray)t);
            if (t is CbClass) return GetTypeDescr((CbClass)t);
            throw new Exception("invalid call to getTypeDescr");
        }
        
        protected string GetTypeDescr(CbMethod bt)
        {
            // generates the full signature
            // E.g., i32 (%struct.ClassExample*)*
            StringBuilder sb = new StringBuilder();
            sb.Append(GetTypeDescr(bt.ResultType));
            string sep = " (";
            if (!bt.IsStatic)
            {   // the implicit first parameter is 'this'
                sb.Append(" (");
                sb.Append(GetTypeDescr(bt.Owner));
                sep = ", ";
            }
            foreach(var argType in bt.ArgType) {
                sb.Append(sep);
                sb.Append(GetTypeDescr(argType));
                sep = ", ";
            }
            if (sep == ", ")
                sb.Append(")");
            else
                sb.Append(" ()");
            return sb.ToString();
        }

        public string CreateSSAName( string baseName ) {
            int num = 0;
            if (SSANumbering.TryGetValue(baseName, out num))
            {
                num++;
                SSANumbering[baseName] = num;
            }
            else
            {
                SSANumbering[baseName] = num;
            }
            if (num == 0)
                return baseName;
            else
                return baseName + "." + num;
        }

        public string CreateBBLabel(string prefix="label")
        {
            return prefix + "." + nextBBNumber++;
        }

        public void WriteLabel(string name)
        {
            ll.WriteLine(name + ":");
        }

        public void WriteBranch(string lab)
        {
            ll.WriteLine("  br label %{0}", lab);
        }

        public void WriteCondBranch(LLVMValue cond, string trueDest, string falseDest)
        {
            Debug.Assert(cond.LLType == "i1");
            ll.WriteLine("  br i1 {0}, label %{1}, label %{2}",
                cond.LLValue, trueDest, falseDest);
        }

        public SymTab Join(string pred1, SymTab tab1, string pred2, SymTab tab2)
        {
            IEnumerator<SymTabEntry> list1 = tab1.GetEnumerator();
            IEnumerator<SymTabEntry> list2 = tab2.GetEnumerator();
            SymTab result = new SymTab();
            for ( ; ; )
            {
                bool b1 = list1.MoveNext();
                bool b2 = list2.MoveNext();
                if (!b1 && !b2)
                    break;
                if (b1 && b2)
                {
                    SymTabEntry e1 = list1.Current;
                    SymTabEntry e2 = list2.Current;
                    if (e1 == null && e2 == null)
                    {
                        result.Enter();
                        continue;
                    }
                    Debug.Assert(e1 != null && e2 != null && e1.Name == e2.Name);
                    SymTabEntry ne = result.Binding(e1.Name, e1.DeclLineNo);
                    ne.Type = e1.Type;
                    if (e1.SSAName == e2.SSAName)
                    {
                        ne.SSAName = e1.SSAName;
                    }
                    else
                    {
                        string newName = "%" + CreateSSAName(e1.Name);
                        ll.WriteLine("  {0} = phi {1} [{2}, %{3}], [{4}, %{5}]",
                            newName, GetTypeDescr(e1.Type),
                            e1.SSAName, pred1, e2.SSAName, pred2);
                        ne.SSAName = newName;
                    }
                }
                else
                    throw new Exception("Attempt to join inconsistent symbol tables");
            }
            return result;
        }

        public void OutputClassDefinition(CbClass t)
        {
            // output type definition for heap instances of class t,
            // including its vtable and a string constant holding its name
            
            IList<CbMethod> vtEntries = new List<CbMethod>();
            IList<CbField> fields = new List<CbField>();
            addMembers(vtEntries, fields, t);

            // Define the type of this class's VTable
            ll.Write("  %{0}..VTableType = type ", t.Name);
            char sep = '{';
            foreach( var entry in vtEntries ) {
                ll.Write("{0} {1}*", sep, GetTypeDescr(entry));
                sep = ',';
            }
            if (sep == ',')
                ll.WriteLine(" {0}", rightBrace);
            else
                ll.WriteLine("{0} i64* {1}", leftBrace, rightBrace);
            // Now declare the VTable itself
            ll.Write("  @{0}..VTable = global %{0}..VTableType ", t.Name);
            sep = '{';
            foreach( var entry in vtEntries ) {
                ll.Write("{0} {1}* @{2}.{3}",
                    sep, GetTypeDescr(entry), entry.Owner.Name, entry.Name);
                sep = ',';
            }
            if (sep == ',')
                ll.WriteLine(" {0}, align {1}", rightBrace, ptrAlign);
            else
                ll.WriteLine("  {0} i64* null {1}, align {2}", leftBrace, rightBrace, ptrAlign);

            // instances begin with class name pointer and vtable pointer
            ll.Write("  %{0} = type {1} i8*, %{0}..VTableType*", t.Name, leftBrace);
            int index = 2;
            foreach( CbField mm in fields ) {
                mm.Index = index++;
                ll.Write(", " + GetTypeDescr(mm.Type));
            }
            // Note: there is a fictitious int array with 0 elements at the end.
            // The byte offset of that field gives us the size of the instance. 
            ll.WriteLine(", [0 x i32] {0}", rightBrace);
            t.LastIndex = index;
            ll.WriteLine("  @{0}..Name = private unnamed_addr constant [{1} x i8] c\"{2}\\00\", align 1",
                t.Name, t.Name.Length+1, t.Name);
        }

        int stringConstNum = 0;

        public void OutputConstDefn( CbConst cnst, AST_leaf initVal ) {
            CbClass c = cnst.Owner;
            CbType cnstType = cnst.Type;
            CbType initType = initVal.Type;
            // Cb consts can only have int, char or string types
            if (cnstType == CbType.Int) {
                ll.WriteLine("@{0}.{1} = global i32 {2}, align 4",
                    c.Name, cnst.Name, GetIntVal(initVal).LLValue);
            } else if (cnstType == CbType.Char) {
                ll.WriteLine("@{0}.{1} = global i8 {2}, align 4",
                    c.Name, cnst.Name, GetIntVal(initVal).LLValue);
            } else { // it's a string
                byte[] bytes = convertString(initVal.Sval);
                string name = "@" + c.Name + "." + cnst.Name;
                CreateStringConstant(name, bytes);
            }
        }

        byte[] convertString( string s ) {
            List<byte> r = new List<byte>();
            int len = s.Length-1;
            Debug.Assert(len >= 1 && s[0] == '"' && s[len] == '"');
            int ix = 1;
            while(ix < len) {
                char c = s[ix++];
                if (c == '\\') {
                    Debug.Assert(ix < len);
                    c = s[ix++];
                    switch(c) {
                    case 'r':
                        r.Add((byte)13);
                        continue;
                    case 'n':
                        r.Add((byte)10);
                        continue;
                    case 't':
                        r.Add((byte)9);
                        continue;
                    }
                    // drop through for the \' and \" cases
                }
                r.Add((byte)c);
            }
            r.Add((byte)0);  // C string terminator
            return r.ToArray();
        }

        // string constant definitions are collected here until the current method
        // has been completed, and then they are output
        List<string> stringConstantDefs = new List<string>();

        public void CreateStringConstant(string name, byte[] chars)
        {
            string sname = "@.str" + stringConstNum;
            stringConstNum++;
            ll.WriteLine(createStringConstDefn(sname,chars));
            ll.WriteLine("{0} = global i8* getelementptr inbounds ([{1} x i8]* {2}, i32 0, i32 0), align {3}",
                name, chars.Length, sname, ptrAlign);
        }

        string createStringConstDefn(string name, byte[] chars)
        {
            string type = String.Format("[{0} x i8]", chars.Length);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} = private unnamed_addr constant {1} c\"", name, type);
            foreach (byte b in chars)
            {
                if (b < (byte)20 || b == (byte)34 || b == (byte)92 || b >= (byte)127)
                {
                    // use hex for special chars or doublequote or backslash
                    sb.AppendFormat("\\{0:X2}", b);
                }
                else
                {
                    sb.Append((char)b);
                }
            }
            sb.Append("\", align 1");
            return sb.ToString();
        }

        // creates a new string constant; it returns the name and type
        public LLVMValue WriteStringConstant( byte[] chars ) {
            string name = "@.str"+stringConstNum;
            stringConstNum++;
            string ss = createStringConstDefn(name, chars);
            stringConstantDefs.Add(ss);
            string rv = "%" + nextUnnamedIndex++;
            ll.WriteLine("  {0} = getelementptr inbounds [{1} x i8]* {2}, i32 0, i32 0",
                rv, chars.Length, name);
            return new LLVMValue("i8*", rv, false);
        }

        // creates a new string constant; it returns the name and type
        public LLVMValue WriteStringConstant( AST_leaf node ) {
            byte[] bb = convertString(node.Sval);
            return WriteStringConstant(bb);
        }

        // given a node which is either IntConst or CharConst, return its int value
        public LLVMValue GetIntVal( AST_leaf n ) {
            if (n.Tag == NodeType.IntConst)
                return new LLVMValue("i32", n.Ival.ToString(), false);
            Debug.Assert(n.Tag == NodeType.CharConst);
            string s = n.Sval;
            int len = s.Length;
            int val;
            Debug.Assert(len >= 3 && s[0] == '\'' && s[len-1] == '\'');
            if (len == 3)
                val = (int)s[1];
            else {
                Debug.Assert(len == 4 && s[1] == '\\');
                switch(s[2]) {
                case 'r':   val = 13; break;
                case 'n':   val = 10; break;
                case 't':   val = 9;  break;
                default:    val = (int)s[2]; break;
                }
            }
            return new LLVMValue("i8", val.ToString(), false);
        }

        // creates list of (virtual) methods to go into a class's VTable and fields
        // to go into a class instance;
        // each method is annotated with its index into the VTable
        private void addMembers(IList<CbMethod> list, IList<CbField> fields, CbClass t) {
            if (t == null) return;
            addMembers(list, fields, t.Parent);  // parent's methods & fields go in first!
            foreach( var m in t.Members.Values ) {
                CbField field = m as CbField;
                if (field != null) {
                    fields.Add(field);
                    continue;
                }
                CbMethod meth = m as CbMethod;
                if (meth == null || meth.IsStatic) continue;
                bool isOverride = false;
                for( int i=0; i < list.Count; i++ ) {
                    CbMethod c = list[i];
                    if (c.Name == meth.Name) {
                        // it's an override
                        meth.VTableIndex = i;
                        list[i] = meth;
                        isOverride = true;
                        break;
                    }
                }
                if (!isOverride) {
                    meth.VTableIndex = list.Count;
                    list.Add(meth);
                }
            }
        }

    }
}