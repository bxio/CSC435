/* LLVM-SSAMethods.cs
 *
 * Methods which support the SSA scheme in LLVM code
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

        // shows which integer was appended to a name most recently
        // to make the name unique
        IDictionary<string, int> SSANumbering = new Dictionary<string,int>();

        // Creates a new unique name for a local variable
        public string CreateSSAName( string baseName ) {
            string result;
            int num = 0;
            SSANumbering.TryGetValue(baseName, out num);
            num++;
            result = baseName + "." + num;
            SSANumbering[baseName] = num;
            return result;
        }

        // Given identification of a Cb method's local variable in the symbol table,
        // return the LLVM temporary which holds its latest value
        public LLVMValue AccessLocalVariable( SymTabEntry local ) {
            return new LLVMValue(GetTypeDescr(local.Type), local.SSAName, false);
        }

        // Provides access to the names of values generated by phi functions
        // when output by a Join operation
        public struct strpair {
          public string a, b;
          public void printValues(){
            Console.WriteLine("a={0},b={1}", a,b);
          }

        }
        public List<strpair> GeneratedNames = null;

        // Merges two symbol tables at a join point in the code
        // Each symbol table contains the latest version used for each variable
        // in the SSA naming scheme
        // Phi instructions are generated when two different names must be compined
        public SymTab Join(string pred1, SymTab tab1, string pred2, SymTab tab2)
        {
            IEnumerator<SymTabEntry> list1 = tab1.GetEnumerator();
            IEnumerator<SymTabEntry> list2 = tab2.GetEnumerator();
            SymTab result = new SymTab();
            GeneratedNames = new List<strpair>();
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
                        string newName = "%" + CreateSSAName(e1.Name);//nextTemporary();
                        ll.WriteLine("  {0} = phi {1} [{2}, %{3}], [{4}, %{5}]",
                            newName, GetTypeDescr(e1.Type),
                            e1.SSAName, pred1, e2.SSAName, pred2);
                        ne.SSAName = newName;
                        strpair newpair = new strpair();
                        newpair.a = newName;
                        newpair.b = e1.SSAName;
                        GeneratedNames.Add(newpair);
                    }
                }
                else
                    throw new Exception("Attempt to join inconsistent symbol tables");
            }
            return result;
        }

        public LLVMValue JoinTemporary(string pred1, LLVMValue version1, string pred2, LLVMValue version2)
        {
            Debug.Assert(version1.LLType == version2.LLType);
            string newIdent = JoinTemporary(pred1, version1.LLValue, pred2, version2.LLValue, version1.LLType);
            return new LLVMValue(version1.LLType, newIdent, false);
        }

        public string JoinTemporary(string pred1, string identifier1, string pred2, string identifier2, string type)
        {
            string rv = nextTemporary();
            ll.WriteLine(" {0} = phi {1} [{2}, {3}], [{4}, {5}]",
                rv,
                type,
                identifier1,
                pred1,
                identifier2,
                pred2);
            return rv;
        }
    }
}
