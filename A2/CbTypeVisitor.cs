using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd{
	public class TypeVisitor : Visitor{
		private TextWriter f;
		private int indent = 0;
		//a class to keep track of information as we make our way through the tree.
		private class TravelInfo{
			public CbClass InClass = null;
			public CbType InsideField = null;
			public CbType InsideConst = null;
			public CbMethod InsideMethod = null;
			public bool RequestReturn = false;
			public string Ident = null;
			public CbType basicType = null;
			public bool IsArray = false;
			public enum MethodAttribute { None, Static, Virtual, Override };
			public MethodAttribute methodAttrs;
		}

		public TypeVisitor(TextWriter output){
			f = output; indent = 0;
		}

		public TypeVisitor(){
			f = Console.Out; indent = 0;
		}

		private string indentString(int indent){
			return " ".PadRight(2 * indent);
		}

		public override void Visit(AST_kary n, object data){
			switch(n.Tag){
				case NodeType.UsingList:{
					//All children in usinglist should be idents
					AddSystemTokens();
					break;
				}
				case NodeType.IdList:{
					TravelInfo statusInfo = (TravelInfo)(data);
					if((statusInfo.RequestReturn == true) && (statusInfo.InsideField != null)){
					//Since we are in a field decl, we should add all names
						for (int i = 0; i < n.NumChildren; ++i){
							n[i].Accept(this, data);
							CbField field = new CbField(statusInfo.Ident, statusInfo.InsideField);
							statusInfo.InClass.AddMember(field);
						}
					}else{
						Skip(n, data);
					}
					break;
				}
				case NodeType.Formal:{
					//formal declarations
					TravelInfo statusInfo = (TravelInfo)(data);
					if((statusInfo.RequestReturn == true) && (statusInfo.InsideMethod != null)){
						n[0].Accept(this, data);
						CbType ParType = DetermineType(statusInfo);
						statusInfo.InsideMethod.ArgType.Add(ParType);
					}else{
						Skip(n, data);
					}
					break;
				}
				default:{
					//just break
					Skip(n, data);
					break;
				}
			}
		}

		public override void Visit(AST_leaf n, object data){
			if(data != null){
				TravelInfo statusInfo = (TravelInfo)(data);
				if(statusInfo.RequestReturn == true){
					switch(n.Tag){
						case NodeType.Ident:{
							statusInfo.Ident = n.Sval;
							break;
						}case NodeType.IntType:{
							statusInfo.Ident = null;
							statusInfo.basicType = CbType.Int;
							break;
						}case NodeType.CharType:{
							statusInfo.Ident = null;
							statusInfo.basicType = CbType.Char;
							break;
						}case NodeType.StringType:{
							statusInfo.Ident = null;
							statusInfo.basicType = CbType.String;
							break;
						}case NodeType.VoidType:{
							statusInfo.Ident = null;
							statusInfo.basicType = CbType.Void;
							break;
						}case NodeType.Static:{
							statusInfo.methodAttrs = TravelInfo.MethodAttribute.Static;
							break;
						}case NodeType.Override:{
							statusInfo.methodAttrs = TravelInfo.MethodAttribute.Override;
							break;
						}case NodeType.Virtual:{
							statusInfo.methodAttrs = TravelInfo.MethodAttribute.Virtual;
							break;
						}default:
							Skip(n, data);
						break;
					}
				}
			}
		}

		public override void Visit(AST_nonleaf n, object data){
			switch(n.Tag){
				case NodeType.Class:{
					TravelInfo statusInfo = new TravelInfo();
					statusInfo.RequestReturn = true;
					String className;
					CbClass basecls = null;
					n[0].Accept(this, statusInfo);
					className = statusInfo.Ident;
					if(n[1] != null){
						n[1].Accept(this, statusInfo);
						basecls = (CbClass)NameSpace.TopLevelNames.LookUp(statusInfo.Ident);
					}
					CbClass cls = new CbClass(className, basecls);
					statusInfo.InClass = cls;
					statusInfo.RequestReturn = false;
					statusInfo.InsideField = null;
					NameSpace.TopLevelNames.AddMember(cls);
					if(n[2] != null){
						n[2].Accept(this, statusInfo);
					}
					break;
				}case NodeType.Field:{
					TravelInfo statusInfo = (TravelInfo)(data);
					//retrieve the type from TravelInfo class
					statusInfo.RequestReturn = true;
					n[0].Accept(this, statusInfo);
					CbType fieldType = DetermineType(statusInfo);
					statusInfo.InsideField = fieldType;
					//populate the members of the ident list
					n[1].Accept(this, statusInfo);
					statusInfo.RequestReturn = false;
					statusInfo.InsideField = null;
					break;
				}case NodeType.Array:{
					TravelInfo statusInfo = (TravelInfo)(data);
					if(statusInfo.RequestReturn == true)
					//set array flag
					statusInfo.IsArray = true;
					n[0].Accept(this, data);
					break;
				}case NodeType.Const:{
					TravelInfo statusInfo = (TravelInfo)(data);
					//retrieve the type from TravelInfo class
					statusInfo.RequestReturn = true;
					n[0].Accept(this, statusInfo);
					CbType fieldType = DetermineType(statusInfo);
					statusInfo.InsideConst = fieldType;
					//retrieve the identifier from TravelInfo class
					n[1].Accept(this, statusInfo);
					CbConst cons = new CbConst(statusInfo.Ident, fieldType);
					statusInfo.InClass.AddMember(cons);
					statusInfo.RequestReturn = false;
					statusInfo.InsideConst = null;
					break;
				}case NodeType.Method:{
					TravelInfo statusInfo = (TravelInfo)(data);
					//retrieve the method attribute
					statusInfo.RequestReturn = true;
					n[0].Accept(this, data);
					//method return type
					n[1].Accept(this, data);
					CbType rtType = DetermineType(statusInfo);
					//method name
					if(n[2] != null){
						n[2].Accept(this, data);
					}

					string methodname = statusInfo.Ident;
					//Create then dispatch to formal list
					CbMethod method = new CbMethod(methodname, statusInfo.methodAttrs == TravelInfo.MethodAttribute.Static, rtType, new List<CbType>());
					statusInfo.InsideMethod = method;
					if(n[3] != null){
						n[3].Accept(this, data);
					}

					statusInfo.InClass.AddMember(method);
					statusInfo.RequestReturn = false;
					n[4].Accept(this, data);
					statusInfo.InsideMethod = null;
					break;
				}
				default:{
					Skip(n, data);
					break;
				}
			}
		}

		//Accepts (skips) the rest of the children for the given node.
		private void Skip(AST_kary n, object data){
			for (int i = 0; i < n.NumChildren; i++){
				if(n[i] != null) n[i].Accept(this, data);
			}
		}

		private void Skip(AST_leaf n, object data){
			for (int i = 0; i < n.NumChildren; i++){
				if(n[i] != null) n[i].Accept(this, data);
			}
		}

		private void Skip(AST_nonleaf n, object data){
			for (int i = 0; i < n.NumChildren; i++){
				if(n[i] != null) n[i].Accept(this, data);
			}
		}

		//grabs the type from the status data.
		private CbType DetermineType(TravelInfo statusInfo){
			CbType result = null;
			//Is it an ident?
			if(statusInfo.Ident == null){
				result = statusInfo.basicType;
			}else{
				result = (CbType)NameSpace.TopLevelNames.LookUp(statusInfo.Ident);
				statusInfo.Ident = null;
			}
			//is it an array?
			if(statusInfo.IsArray == true){
				CbType tmp = new CFArray(result);
				result = tmp;
				statusInfo.IsArray = false;
			}
			return result;
		}

		//lots of testing required on this method
		private void AddSystemTokens(){
			CbMethod method = null;
			IList<CbType> partList = null;
			{
				//the basic class
				CbClass StringClass = CbType.String;
				partList = new List<CbType>();
				partList.Add(CbType.Int);
				method = new CbMethod("Substring", false, CbType.String, partList);
				StringClass.AddMember(method);

				method = new CbMethod("Length", false, CbType.Int, null);
				StringClass.AddMember(method);

				NameSpace.TopLevelNames.AddMember(StringClass);
				NameSpace.TopLevelNames.AddMember(CbType.Object);
			}
			{
				//the console class
				CbClass cls = new CbClass("Console", null);
				partList = new List<CbType>();
				partList.Add(CbType.String);
				method = new CbMethod("Write", true, CbType.Void, partList);
				cls.AddMember(method);

				partList = new List<CbType>();
				partList.Add(CbType.String);
				method = new CbMethod("WriteLine", true, CbType.Void, partList);
				cls.AddMember(method);

				method = new CbMethod("ReadLine", true, CbType.String, null);

				NameSpace.TopLevelNames.AddMember(cls);
			}
		}
	}
}
