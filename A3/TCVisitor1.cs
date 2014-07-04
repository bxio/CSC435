/*  CbTCVisitor1.cs

		Fills in Missing Type Information

		Based On:

			Defines a Top-Level Symbol Table Visitor class for the CFlat AST

			Author: Nigel Horspool

			Dates: 2012-2014
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd {


// Traverses the AST to add top-level names into the top-level
// symbol table.
// Incomplete type descriptions of new classes are createdn too,
// these descriptions specify only the parent class and the names
// of members (but each is associated with a minimal field, method
// or const type description as appropriate).
public class TCVisitor1: Visitor {
	private Dictionary<string, AST> pendingClassDefns = null;
	private NameSpace currentNameSpace = null;

	// constructor
	public TCVisitor1( ){

	}

	public override void Visit(AST_kary node, object data){
		Dictionary<string, AST> savedList;
		switch(node.Tag){
		case NodeType.UsingList: //Do Nothing
			currentNameSpace = (NameSpace)data;
			Debug.Assert(data != null && data is NameSpace);
				break;

		case NodeType.ClassList:
			currentNameSpace = (NameSpace)data;
			Debug.Assert(data != null && data is NameSpace);
				// traverse each class
				for(int i=0; i<node.NumChildren; i++){
					node[i].Accept(this, data);
				}
				break;

		case NodeType.MemberList:
			Debug.Assert(data != null && data is CbClass);
				// add each member to the current class, by continuing traversal
				for(int i=0; i<node.NumChildren; i++){
					node[i].Accept(this, data);
				}
				break;

		default:
			throw new Exception("Unexpected tag: "+node.Tag);
		}
	}

	public override void Visit( AST_nonleaf node, object data ){
				switch(node.Tag){
				case NodeType.Program: //Modified in-line with hints
					Debug.Assert(data != null && data is NameSpace);
					currentNameSpace = (NameSpace)data;
					node[1].Accept(this, data);  // visit class declarations
					break;

				case NodeType.Class: //Modified in-line with hints
					Debug.Assert(data != null && data is NameSpace);
					currentNameSpace = (NameSpace)data;
					NameSpace ns = (NameSpace)data;
					AST_kary memberList = node[2] as AST_kary;
					// Look up CbType type description
					AST_leaf classNameId = node[0] as AST_leaf;
					string className = classNameId.Sval;
					object ctd = ns.LookUp(className);
					Debug.Assert(ctd is CbClass);
					CbClass classTypeDefn = (CbClass)ctd;
					// Visit each member of the class, passing CbType as a parameter
					//memberList.Accept(this, classTypeDefn);
					for(int i=0; i<memberList.NumChildren; i++){
						memberList[i].Accept(this,classTypeDefn);
					}
					break;

				case NodeType.Const: //Modified in-line with hints
					Debug.Assert(data != null && data is CbClass);
					CbClass c1 = (CbClass)data;
					// find const in class
					AST_leaf cid = (AST_leaf)(node[1]);
					CbConst cdef = (CbConst)c1.FindMember(cid.Sval);
					CbMethod cmed = new CbMethod("temp", false, CbType.Void, new List<CbType>());
					node[0].Accept(this, cmed);
					cdef.Type = cmed.ArgType[0];
					node[0].Type = cmed.ArgType[0];
					break;

				case NodeType.Field:
					Debug.Assert(data != null && data is CbClass);
					CbClass c2 = (CbClass)data;
					// find field in class
					AST_kary fields = (AST_kary)(node[1]);
					for(int i=0; i<fields.NumChildren; i++){
						AST_leaf id = fields[i] as AST_leaf;
						CbField fdef = (CbField)c2.FindMember(id.Sval);
						CbMethod cmed2 = new CbMethod("temp", false, CbType.Void, new List<CbType>());
						node[0].Accept(this, cmed2);
						fdef.Type = cmed2.ArgType[0];
						id.Type = cmed2.ArgType[0];
					}
					break;

				case NodeType.Method:
					Debug.Assert(data != null && data is CbClass);
					CbClass c3 = (CbClass)data;
					// look up method in current class
					AST_leaf mid = (AST_leaf)(node[1]);
					CbMethod mdef = (CbMethod)c3.FindMember(mid.Sval);
					// store return type (node 0)

					if(node[0] != null && mdef != null){
						CbMethod cmed3 = new CbMethod("temp", false, CbType.Void, new List<CbType>());
						node[0].Accept(this, cmed3);
						mdef.ResultType = cmed3.ArgType[0];
						node[0].Type = cmed3.ArgType[0];
					}else{
						mdef.ResultType = CbType.Void;
					}
					// store arguments type
					mdef.ArgType = new List<CbType>();
					AST_kary formalList = node[2] as AST_kary;
					for(int i=0; i<formalList.NumChildren; i++){
						formalList[i].Accept(this,mdef);
					}
					break;

				case NodeType.Formal:
					node[0].Accept(this, data);
					break;

				case NodeType.Array:
					switch(((AST_leaf)node[0]).Tag){
						case NodeType.IntType:
							((CbMethod)data).ArgType.Add(CbType.Array(CbType.Int));
							break;

						case NodeType.CharType:
							((CbMethod)data).ArgType.Add(CbType.Array(CbType.Char));
							break;

						case NodeType.StringType:
							((CbMethod)data).ArgType.Add(CbType.Array(CbType.String));
							break;

						case NodeType.VoidType:
							((CbMethod)data).ArgType.Add(CbType.Array(CbType.Void));
							break;

						case NodeType.Ident:
							object classIdent = currentNameSpace.LookUp(((AST_leaf)node[0]).Sval);
							Debug.Assert(classIdent is CbClass);
							((CbMethod)data).ArgType.Add(CbType.Array((CbType)classIdent));
							break;

						default:
							break;

					}
					break;

				default:
						throw new Exception("Unexpected tag: "+node.Tag);
				}
		}

	public override void Visit(AST_leaf node, object data){
	switch(node.Tag){
		case NodeType.IntType:
			((CbMethod)data).ArgType.Add(CbType.Int);
			node.Type = CbType.Int;
			break;

		case NodeType.CharType:
			((CbMethod)data).ArgType.Add(CbType.Char);
			node.Type = CbType.Char;
			break;

		case NodeType.StringType:
			((CbMethod)data).ArgType.Add(CbType.String);
			node.Type = CbType.String;
			break;

		case NodeType.VoidType:
			((CbMethod)data).ArgType.Add(CbType.Void);
			node.Type = CbType.Void;
			break;

		case NodeType.Ident: //check current namespace
			object classIdent = currentNameSpace.LookUp(node.Sval);
			Debug.Assert(classIdent is CbClass);
			((CbMethod)data).ArgType.Add((CbType)classIdent);
			node.Type = (CbType)classIdent;
			break;

		default:
			throw new Exception("Unexpected tag: "+node.Tag);
		}
	}

	private void openNameSpace( AST ns2open, NameSpace currentNS ){
			string nsName = ((AST_leaf)ns2open).Sval;
			object r = currentNS.LookUp(nsName);
			if (r == null){
				Start.SemanticError(ns2open.LineNumber, "namespace {0} not found", nsName);
				return;
			}
			NameSpace c = r as NameSpace;
			if (r == null){
				Start.SemanticError(ns2open.LineNumber, "{1} is not a namespce", nsName);
				return;
			}

			foreach(object def in c.Members){
				Debug.Assert(def is CbClass);
				currentNS.AddMember((CbClass)def);
			}
	}

}

}
