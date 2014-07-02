/*  CbVisitor.cs

    Defines a Type-Checking Visitor class for the CFlat AST
	
	June 2014
*/

using System;
using System.IO;
using System.Collections.Generic;

namespace FrontEnd {

	public class TCVisitor1 : Visitor {
		
		NameSpace temp = null;
		
		// constructor
		public TCVisitor1( ) 
		{
		}	
		
		public override void Visit(AST_kary node, object data) {
			switch (node.Tag)
			{
			case NodeType.ClassList:
				// Traverse through all classes
				for (int i=0; i<node.NumChildren; i++) 
				{
					node[i].Accept(this,data);
				}
				break;
			case NodeType.MemberList:
				// Traverse through consts, methods, fields of class
				for (int i=0; i<node.NumChildren; i++) 
				{
					node[i].Accept(this,data);
				}
				break;
			default:
				throw new Exception("Unexpected tag: "+node.Tag);		
			}
		}

		public override void Visit(AST_leaf node, object data) {
			throw new Exception("Unexpected tag: "+node.Tag);
		}

		public override void Visit(AST_nonleaf node, object data) {
			switch (node.Tag)
			{
			case NodeType.Program:
				temp = data as NameSpace;
				node[1].Accept(this,data);
				break;
			case NodeType.Class:
				// Retrieve CbType object from top-level symbol table
				NameSpace ns = (NameSpace)data;
				AST_leaf classNameId = node[0] as AST_leaf;
				string className = classNameId.Sval;
				object ctd = ns.LookUp(className); // returns a CbClass object?
				
				// Visit each member declaration (methods, fields, consts)
				node[2].Accept(this,ctd);
				break;
			case NodeType.Const:
				// get ID of const
				AST_leaf constNameId = node[1] as AST_leaf;
				string constName = constNameId.Sval;
				
				// Look up the const (CbMember) from the CbClass object
				CbClass cb = (CbClass)data;
				CbConst con = cb.Members[constName] as CbConst;
				
				// get type of the const and assign it to CbConst object
				AST_leaf constType = node[0] as AST_leaf;
				con.Type = checkType(constType) as CbType; // local method
				break;			
			case NodeType.Field:
				// Get a reference to the IdList node
				AST_kary fields = node[1] as AST_kary;
				
				// Loop through each child of the IdList
				for (int i=0; i<fields.NumChildren; i++)
				{
					// get ID of field
					AST_leaf fieldNameId = fields[i] as AST_leaf;
					string fieldName = fieldNameId.Sval;
					
					// Look up the field (CbMember) from the CbClass object
					CbClass c = (CbClass)data;
					CbField field = c.Members[fieldName] as CbField;
					
					// get type of the field and assign it to the CbField object
					AST_leaf fieldType = node[0] as AST_leaf;
					field.Type = checkType(fieldType) as CbType; // local method
				}
				break;
			case NodeType.Method:				
				// get ID of const
				AST_leaf methodNameId = node[1] as AST_leaf;
				string methodName = methodNameId.Sval;
				
				// Look up the const (CbMethod) from the CbClass object
				CbClass cb1 = (CbClass)data;
				CbMethod method = cb1.Members[methodName] as CbMethod;
				
				// If return type is non-void, input a return type in the CbMethod object	
				AST_leaf typeName = node[0] as AST_leaf;
				method.ResultType = checkType(typeName) as CbType;
				
				// Initialize list of argument types
				method.ArgType = new List<CbType>();
				
				// Populate the list
				AST_kary formalPara = node[2] as AST_kary;
				for (int i=0; i < formalPara.NumChildren; i++)
				{
					CbType t = checkType(formalPara[i][0]) as CbType;
					method.ArgType.Add(t);
				}
				break;
			default:
				throw new Exception("Unexpected tag: "+node.Tag);
			}
		}

		public object checkType(AST n)
		{
			switch (n.Tag)
			{
			case NodeType.IntType:
				return CbType.Int;
			case NodeType.CharType:
				return CbType.Char;
			case NodeType.StringType:
				return CbType.String;	
			case NodeType.VoidType:
				return CbType.Void;
			case NodeType.Ident:
				return CbType.Null;		
			default:
				return CbType.Error;
			}
		}
	}

}
