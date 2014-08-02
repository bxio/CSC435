/* LLVM-WriteMethods.cs
 * 
 * Utility code to help with outputting intermediate code in the
 * LLVM text format (as a '.ll' file).
 * 
 * Author: Nigel Horspool
 * Date: July 2014
 */
 
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;


namespace FrontEnd
{
        
    public partial class LLVM
    {
    
        int nextUnnamedIndex = -1;  // used to generate %0, %1, %2 ... sequences

        // generate code for the start of method m in class c
        // methodDecl references the AST node with tag MethodDecl where the method is described
        public void WriteMethodStart( CbClass c, CbMethod m, AST methodDecl ) {
            SSANumbering.Clear();
            if (m.Name == "Main" && m.IsStatic)
                ll.Write("\ndefine void @main ");
            else
                ll.Write("\ndefine {0} @{1}.{2} ",
                    GetTypeDescr(m.ResultType), c.Name, m.Name);
            char sep;
            if (m.IsStatic) {
                sep = '(';
            } else {
                // provide 'this' pointer as first argument
                ll.Write("({0} %this", GetTypeDescr(c));
                sep = ',';
            }
            if (methodDecl.Tag != NodeType.Method)
                throw new Exception("bad call to WriteMethodStart");
            AST_kary formals = methodDecl[2] as AST_kary;
            if (formals == null || formals.NumChildren != m.ArgType.Count)
                throw new Exception("bad AST structure");
            for( int i=0; i<m.ArgType.Count; i++ ) {
                AST_nonleaf formal = formals[i] as AST_nonleaf;
                AST_leaf idNode = formal[1] as AST_leaf;
                string id = idNode.Sval;
                ll.Write("{0} {1} %{2}", sep, GetTypeDescr(m.ArgType[i]), id);
                sep = ',';
            }
            if (sep == '(')
                ll.WriteLine("() #0 {");
            else
                ll.WriteLine(" ) #0 {");
            nextBBNumber = 0;
            nextUnnamedIndex = 0;
        }

        public void WriteMethodEnd(CbMethod currentMethod) {
            // make sure that all code paths return a result!
            if (currentMethod.ResultType != CbType.Void) {
                if (currentMethod.ResultType == CbType.Int)
                    ll.WriteLine("  ret i32 0");
                else if (currentMethod.ResultType == CbType.Char)
                    ll.WriteLine("  ret i8 0");
                else
                    ll.WriteLine("  ret {0} null", GetTypeDescr(currentMethod.ResultType));
            } else {
                ll.WriteLine("  ret void");
            }
            ll.WriteLine("}\n");
            if (stringConstantDefs.Count > 0)
            {
                ll.WriteLine();
                foreach (string s in stringConstantDefs)
                {
                    ll.WriteLine(s);
                }
                stringConstantDefs.Clear();
            }
        }

        // generate code to instantiate a class instance (aka the new operator);
        // the result is the name of the temporary (e.g. %37) which holds the
        // reference to the new instance on the heap and its type, as a pair
        public LLVMValue NewClassInstance( CbClass t ) {
            string instanceType = GetTypeDescr(t);
            ll.WriteLine("  %{0} = getelementptr inbounds {1} null, i32 0, i32 {2}",
                nextUnnamedIndex, instanceType, t.LastIndex);
            ll.WriteLine("  %{0} = ptrtoint [0 x i32]* %{1} to i32",
                nextUnnamedIndex+1, nextUnnamedIndex);
            ll.WriteLine("  %{0} = call i8* @malloc(i32 %{1})",
                nextUnnamedIndex+2, nextUnnamedIndex+1);
            // clear the allocated storage to 0
            ll.WriteLine("  call void @llvm.memset.p0i8.i32(i8* %{0}, i8 0, i32 %{1}, i32 0, i1 0)",
                nextUnnamedIndex+2, nextUnnamedIndex+1);
            // convert from i8* to the proper class instance pointer type
            ll.WriteLine("  %{0} = bitcast i8* %{1} to {2}",
                nextUnnamedIndex+3, nextUnnamedIndex+2, GetTypeDescr(t));
            string r = "%" + (nextUnnamedIndex+3);  // eventually the final result!!

            // store the class name in the first field of the instance
            ll.WriteLine("  ;  store the class name in field #0 of the instance");
            ll.WriteLine("  %{0} = getelementptr inbounds {1} %{2}, i32 0, i32 0",
                nextUnnamedIndex+4, instanceType, nextUnnamedIndex+3);
            ll.WriteLine("  store i8* getelementptr inbounds ([{0} x i8]* @{1}..Name, i32 0, i32 0), i8** %{2}, align {3}",
                t.Name.Length+1, t.Name, nextUnnamedIndex+4, ptrAlign);
            // store the VTable address in the second field
            ll.WriteLine("  ;  store the VTable address in field #1 of the instance");
            ll.WriteLine("  %{0} = getelementptr inbounds {1} %{2}, i32 0, i32 1",
                nextUnnamedIndex+5, instanceType, nextUnnamedIndex+3);
            ll.WriteLine("  store %{0}..VTableType* @{0}..VTable, %{0}..VTableType** %{1}, align {2}",
                t.Name, nextUnnamedIndex+5, ptrAlign);

            nextUnnamedIndex += 6;
            return new LLVMValue(instanceType,r,false);
        }
        
        // generate code to call a virtual method m in class c
        // thisPtr is a value such as "%3" specifying where the instance pointer is held,
        // and args is an array of LLVM values to use as arguments in the method call;
        // the result is the LLVM value which holds the result returned by m,
        // or null if m is void
        public LLVMValue CallVirtualMethod( CbMethod m, LLVMValue thisPtr, LLVMValue[] args ) {
            if (args.Length != m.ArgType.Count)
                throw new Exception("invalid call to CallVirtualMethod");
            CbClass c = m.Owner;
            LLVMValue result = null;
            // load VTable pointer
            ll.WriteLine("  %{0} = getelementptr inbounds {1}, i32 0, i32 1",
                nextUnnamedIndex, thisPtr);
            ll.WriteLine("  %{0} = load %{1}..VTableType** %{2}, align {3}",
                nextUnnamedIndex+1, c.Name, nextUnnamedIndex, ptrAlign);
            // load method address from VTable
            ll.WriteLine("  %{0} = getelementptr inbounds %{1}..VTableType* %{2}, i32 0, i32 {3}",
                nextUnnamedIndex+2, c.Name, nextUnnamedIndex+1, m.VTableIndex);
            ll.WriteLine("  %{0} = load {1}** %{2}, align {3}",
                nextUnnamedIndex+3, GetTypeDescr(m), nextUnnamedIndex+2, ptrAlign);
            int callReg = nextUnnamedIndex+3;
            nextUnnamedIndex += 4;
            string rt = GetTypeDescr(m.ResultType);
            if (m.ResultType != CbType.Void) {
                string rv = "%" + nextUnnamedIndex++;
                ll.Write("  {0} =", rv);
                result = new LLVMValue(rt,rv,false);
            }
            ll.Write("  call {0} %{1} ({2}", rt, callReg, thisPtr);
            for( int i=0; i<args.Length; i++ ) {
                ll.Write(", {0}", args[i]);
            }
            ll.WriteLine(")");
            return result;
        }

       // generate code to call a static method m in class c;
        // args is an array of LLVM values to use as arguments in the method call;
        // the result is the LLVM value & type which holds the result returned by m,
        // or null if m is void
        public LLVMValue CallStaticMethod( CbMethod m, LLVMValue[] args ) {
            if (args.Length != m.ArgType.Count)
                throw new Exception("invalid call to CallVirtualMethod");
            CbClass c = m.Owner;
            string rt = GetTypeDescr(m.ResultType);
            LLVMValue result = null;
            if (m.ResultType != CbType.Void) {
                string rv = "%" + nextUnnamedIndex++;
                ll.Write("{0} = ", rv);
                result = new LLVMValue(rt,rv,false);
            }
            ll.Write("  call {0} @{1}.{2} ", rt, c.Name, m.Name);
            char sep = '(';
            for( int i=0; i<args.Length; i++ ) {
                ll.Write("{0} {1}", sep, args[i]);
                sep = ',';
            }
            if (sep == ',')
                ll.WriteLine(")");
            else
                ll.WriteLine("()");
            return result;
        }

        // Used to generate calls for System.Console methods: Write, WriteLine, ReadLine
        public LLVMValue CallBuiltInMethod( CbType resultType, string name, LLVMValue arg ) {
            LLVMValue result = null;
            string rt = GetTypeDescr(resultType);
            if (resultType != CbType.Void) {
                string rv = "%" + nextUnnamedIndex++;
                ll.Write("{0} = ", rv);
                result = new LLVMValue(rt,rv,false);
            }
            if (arg == null)
                ll.WriteLine("  call {0} {1} ()", rt, name);
            else
                ll.WriteLine("  call {0} {1} ({2})", rt, name, arg);
            return result;  
        }

        public void AllocLocalVar( string name, CbType type ) {
            int align = 4;
            if (type == CbType.Char) align = 1;
            ll.WriteLine("  %{0}.addr = alloca {1}, align {2}",
                name, GetTypeDescr(type), align);
        }
        
        public LLVMValue RefLocalVar( string name, CbType type ) {
            int num = 0;
            if (!SSANumbering.TryGetValue(name, out num) || num == 0)
            {
                SSANumbering[name] = 0;
                return new LLVMValue(GetTypeDescr(type), "%" + name, false);
            }
            return new LLVMValue(GetTypeDescr(type), "%" + name + "." + num, false);
        }

        // Generates a reference to a field of a class instance
        public LLVMValue RefClassField( LLVMValue instancePtr, CbField field ) {
            string rv = "%" + nextUnnamedIndex++;
            ll.WriteLine("  {0} = getelementptr inbounds {1}, i32 0, i32 {2}",
                rv, instancePtr, field.Index);
            return new LLVMValue(GetTypeDescr(field.Type), rv, true);
        }

        // Generates access to a constant -- either the value (for int,char)
        // or a reference for a string value
        public LLVMValue AccessClassConstant( CbConst cnst ) {
            string fullName = String.Format("@{0}.{1}", cnst.Owner.Name, cnst.Name);
            string rv = "%" + nextUnnamedIndex++;
            if (cnst.Type == CbType.Int)
            {
                ll.WriteLine("  {0} = load i32* {1}", rv, fullName);
                return new LLVMValue("i32", rv, false);
            }
            if (cnst.Type == CbType.Char)
            {
                ll.WriteLine("  {0} = load i8* {1}", rv, fullName);
                return new LLVMValue("i8", rv, false);
            }
            ll.WriteLine("  {0} = load i8** {1}", rv, fullName);
            return new LLVMValue("i8*", rv, false);
        }

        public void Store( LLVMValue source, LLVMValue dest ) {
            if (!dest.IsReference)
                throw new Exception("LLVM.Store needs a memory reference for the dest");
            source = Dereference(source);
            string srcType = source.LLType;
            string destType = dest.LLType;
            int align;
            if (destType == "i8") align = 1;
            else if (destType.EndsWith("*")) align = ptrAlign;
            else align = 4;
            ll.WriteLine("  store {0}, {1}* {2}, align {3}",
                source, dest.LLType, dest.LLValue, align);
        }
        
        public void WriteInst( string inst ) {
            ll.Write("  ");
            ll.WriteLine(inst);
        }
        
        public void WriteInst( string inst, params object[] args ) {
            ll.Write("  ");
            ll.WriteLine(inst, args);
        }

        // both operands must have LLVM type i32 (i.e. int type)
        public LLVMValue WriteIntInst(string opcode, LLVMValue lhs, LLVMValue rhs)
        {
            lhs = Dereference(lhs);
            rhs = Dereference(rhs);
            string rv = "%" + nextUnnamedIndex++;
            ll.WriteLine("  {0} = {1} i32 {2}, {3}", rv, opcode, lhs.LLValue, rhs.LLValue);
            return new LLVMValue("i32", rv, false);
        }

        public LLVMValue WriteIntInst(NodeType tag, LLVMValue lhs, LLVMValue rhs)
        {
            string op = null;
            switch (tag)
            {
                case NodeType.Add: op = "add"; break;
                case NodeType.Sub: op = "sub"; break;
                case NodeType.Mul: op = "mul"; break;
                case NodeType.Div: op = "div"; break;
                case NodeType.Mod: op = "srem"; break;
            }
            return WriteIntInst(op, lhs, rhs);
        }


        // compare two int or char values -- comparing two dfferent kinds of pointer is unsupported
        public LLVMValue WriteCompInst(string cmp, LLVMValue lhs, LLVMValue rhs)
        {
            string rv;
            lhs = Dereference(lhs);
            rhs = Dereference(rhs);
            if (lhs.LLType == "i8" && rhs.LLType == "i32")
            {
                lhs = ForceIntValue(lhs);
            }
            else if (lhs.LLType == "i32" && rhs.LLType == "i8")
            {
                rhs = ForceIntValue(rhs);
            }
            // we are comparing two int values here
            rv = "%" + nextUnnamedIndex++;
            ll.WriteLine("  {0} = icmp {1} i32 {2}, {3}", rv, cmp, lhs.LLValue, rhs.LLValue);
            return new LLVMValue("i1", rv, false);
        }

        public LLVMValue WriteCompInst(NodeType tag, LLVMValue lhs, LLVMValue rhs)
        {
            string cmp = null;
            switch (tag)
            {
                case NodeType.Equals: cmp = "eq"; break;
                case NodeType.NotEquals: cmp = "ne"; break;
                case NodeType.GreaterOrEqual: cmp = "sge"; break;
                case NodeType.GreaterThan: cmp = "sgt"; break;
                case NodeType.LessOrEqual: cmp = "sle"; break;
                case NodeType.LessThan: cmp = "slt"; break;
            }
            Debug.Assert(cmp != null);
            return WriteCompInst(cmp, lhs, rhs);
        }

        public LLVMValue ForceIntValue(LLVMValue src)
        {
            string rv;
            src = Dereference(src);
            if (src.LLType == "i32")
                return src;
            if (src.LLType == "i8")
            {
                rv = "%" + nextUnnamedIndex++;
                ll.WriteLine("  {0} = zext i8 {1} to i32", rv, src.LLValue);
                return new LLVMValue("i32", rv, false);
            }
            throw new Exception("unhandled case for LLVM.ForceIntValue");
        }

        // Generates code to coerce int to char, char to int, or
        // any class type to any other class type
        public LLVMValue Coerce( LLVMValue src, CbType srcType, CbType destType ) {
            src = Dereference(src);
            if (srcType == destType)
                return src;
            string rv = "%" + nextUnnamedIndex++;
            if (destType == CbType.Int) {
                // widen from char to int
                ll.WriteLine("  {0} = zext {1} to i32", rv, src);
                return new LLVMValue("i32", rv, false);   
            }
            if (destType == CbType.Char) {
                // narrow from int to to char
                ll.WriteLine("  {0} = trunc {1} to i8", rv, src);
                return new LLVMValue("i8", rv, false);
            }
            if (destType is CbClass) {
                string t = GetTypeDescr(destType);
                ll.WriteLine("  {0} = bitcast {1} to {2}", rv, src, t);
                return new LLVMValue(t, rv, false);
            }
            throw new Exception("bad call to llvm.Coerce");
        }

        public LLVMValue Dereference(LLVMValue src)
        {
            if (!src.IsReference) return src;
            string rv = "%" + nextUnnamedIndex++;
            ll.WriteLine("  {0} = load {1}* {2}", rv, src.LLType, src.LLValue);
            return new LLVMValue(src.LLType, rv, false);
        }
    }
}