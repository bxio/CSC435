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
			public enum MethodAttrTag { None, Static, Virtual, Override };
			public MethodAttrTag methodAttrs;
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
					TravelInfo statusData = (TravelInfo)(data);
					if((statusData.RequestReturn == true) && (statusData.InsideField != null)){
					//Since we are in a field decl, we should add all names
						for (int i = 0; i < n.NumChildren; ++i){
							n[i].Accept(this, data);
							CbField field = new CbField(statusData.Ident, statusData.InsideField);
							statusData.InClass.AddMember(field);
						}
					}else{
						Skip(n, data);
					}
					break;
				}
				case NodeType.Formal:{
					//formal declarations
					TravelInfo statusData = (TravelInfo)(data);
					if((statusData.RequestReturn == true) && (statusData.InsideMethod != null)){
						n[0].Accept(this, data);
						CbType ParType = DetermineType(statusData);
						statusData.InsideMethod.ArgType.Add(ParType);
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
				TravelInfo statusData = (TravelInfo)(data);
				if(statusData.RequestReturn == true){
					switch(n.Tag){
						case NodeType.Ident:{
							statusData.Ident = n.Sval;
							break;
						}
						case NodeType.IntType:{
							statusData.Ident = null;
							statusData.basicType = CbType.Int;
							break;
						}
						case NodeType.CharType:{
							statusData.Ident = null;
							statusData.basicType = CbType.Char;
							break;
						}
						case NodeType.StringType:{
							statusData.Ident = null;
							statusData.basicType = CbType.String;
							break;
						}
						case NodeType.VoidType:{
							statusData.Ident = null;
							statusData.basicType = CbType.Void;
							break;
						}
						case NodeType.Static:{
							statusData.methodAttrs = TravelInfo.MethodAttrTag.Static;
							break;
						}
						case NodeType.Override:{
							statusData.methodAttrs = TravelInfo.MethodAttrTag.Override;
							break;
						}
						case NodeType.Virtual:{
							statusData.methodAttrs = TravelInfo.MethodAttrTag.Virtual;
							break;
						}
						default:
							Skip(n, data);
						break;
					}
				}
			}
		}

		public override void Visit(AST_nonleaf n, object data){
			switch(n.Tag){
				case NodeType.Class:{
					TravelInfo statusData = new TravelInfo();
					statusData.RequestReturn = true;
					String className;
					CbClass basecls = null;
					n[0].Accept(this, statusData);
					className = statusData.Ident;
					if(n[1] != null){
						n[1].Accept(this, statusData);
						basecls = (CbClass)NameSpace.TopLevelNames.LookUp(statusData.Ident);
					}
					CbClass cls = new CbClass(className, basecls);
					statusData.InClass = cls;
					statusData.RequestReturn = false;
					statusData.InsideField = null;
					NameSpace.TopLevelNames.AddMember(cls);
					if(n[2] != null){
						n[2].Accept(this, statusData);
					}
					break;
				}
				case NodeType.Field:{
					TravelInfo statusData = (TravelInfo)(data);
					//retrieve the type from TravelInfo class
					statusData.RequestReturn = true;
					n[0].Accept(this, statusData);
					CbType fieldType = DetermineType(statusData);
					statusData.InsideField = fieldType;
					//populate the members of the ident list
					n[1].Accept(this, statusData);
					statusData.RequestReturn = false;
					statusData.InsideField = null;
					break;
				}
				case NodeType.Array:{
					TravelInfo statusData = (TravelInfo)(data);
					if(statusData.RequestReturn == true)
					//set array flag
					statusData.IsArray = true;
					n[0].Accept(this, data);
					break;
				}
				case NodeType.Const:{
					TravelInfo statusData = (TravelInfo)(data);
					//retrieve the type from TravelInfo class
					statusData.RequestReturn = true;
					n[0].Accept(this, statusData);
					CbType fieldType = DetermineType(statusData);
					statusData.InsideConst = fieldType;
					//retrieve the identifier from TravelInfo class
					n[1].Accept(this, statusData);
					CbConst cons = new CbConst(statusData.Ident, fieldType);
					statusData.InClass.AddMember(cons);
					statusData.RequestReturn = false;
					statusData.InsideConst = null;
					break;
				}
				case NodeType.Method:{
					TravelInfo statusData = (TravelInfo)(data);
					//retrieve the method attribute
					statusData.RequestReturn = true;
					n[0].Accept(this, data);
					//method return type
					n[1].Accept(this, data);
					CbType rtType = DetermineType(statusData);
					//method name
					if(n[2] != null){
						n[2].Accept(this, data);
					}
					string methodname = statusData.Ident;
					//Create then dispatch to formal list
					CbMethod method = new CbMethod(methodname, statusData.methodAttrs == TravelInfo.MethodAttrTag.Static, rtType, new List<CbType>());
					statusData.InsideMethod = method;
					if(n[3] != null){
						n[3].Accept(this, data);
					}

					statusData.InClass.AddMember(method);
					statusData.RequestReturn = false;
					n[4].Accept(this, data);
					statusData.InsideMethod = null;
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

		private CbType DetermineType(TravelInfo statusData){
			CbType result = null;
			//Is it an ident?
			if(statusData.Ident == null){
				result = statusData.basicType;
			}else{
				result = (CbType)NameSpace.TopLevelNames.LookUp(statusData.Ident);
				statusData.Ident = null;
			}
			//is it an array?
			if(statusData.IsArray == true){
				CbType tmp = new CFArray(result);
				result = tmp;
				statusData.IsArray = false;
			}
			return result;
		}

		//lots of testing required on this method
		private void AddSystemTokens(){
			CbMethod method = null;
			IList<CbType> parlst = null;
			{
				//the basic class
				CbClass StringClass = CbType.String;
				parlst = new List<CbType>();
				parlst.Add(CbType.Int);
				method = new CbMethod("Substring", false, CbType.String, parlst);
				StringClass.AddMember(method);

				method = new CbMethod("Length", false, CbType.Int, null);
				StringClass.AddMember(method);

				NameSpace.TopLevelNames.AddMember(StringClass);
				NameSpace.TopLevelNames.AddMember(CbType.Object);
			}
			{
				//the console class
				CbClass cls = new CbClass("Console", null);
				parlst = new List<CbType>();
				parlst.Add(CbType.String);
				method = new CbMethod("Write", true, CbType.Void, parlst);
				cls.AddMember(method);

				parlst = new List<CbType>();
				parlst.Add(CbType.String);
				method = new CbMethod("WriteLine", true, CbType.Void, parlst);
				cls.AddMember(method);

				method = new CbMethod("ReadLine", true, CbType.String, null);

				NameSpace.TopLevelNames.AddMember(cls);
			}
		}
	}
}
