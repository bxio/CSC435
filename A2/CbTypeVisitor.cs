using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd{
  public class TypeVisitor : Visitor{
    private TextWriter f;
    private int indent = 0;

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
      switch (n.Tag){
        case NodeType.UsingList:{
          //All of using list children should be identifiers
          AddSystemTokens();
          break;
        }
        case NodeType.IdList:{
          TravelInfo status = (TravelInfo)(data);
          if ((status.RequestReturn == true) && (status.InsideField != null)){
          //Since we are in a field decl, we should add all declared names
            for (int i = 0; i < n.NumChildren; ++i){
              n[i].Accept(this, data);
              CbField field = new CbField(status.Ident, status.InsideField);
              status.InClass.AddMember(field);
            }
          }else{
            SkipKary(n, data);
          }
          break;
        }
        case NodeType.Formal:{
          TravelInfo status = (TravelInfo)(data);
          if ((status.RequestReturn == true) && (status.InsideMethod != null)){
            n[0].Accept(this, data);
            CbType ParType = DetermineType(status);
            status.InsideMethod.ArgType.Add(ParType);
          }else{
            SkipKary(n, data);
          }
          break;
        }
        default:{
          //just break
          SkipKary(n, data);
          break;
        }
      }
    }

    public override void Visit(AST_leaf n, object data){
      if (data != null){
        TravelInfo status = (TravelInfo)(data);
        if (status.RequestReturn == true){
          switch (n.Tag){
            case NodeType.Ident:{
              status.Ident = n.Sval;
              break;
            }
            case NodeType.IntType:{
              status.Ident = null;
              status.basicType = CbType.Int;
              break;
            }
            case NodeType.CharType:{
              status.Ident = null;
              status.basicType = CbType.Char;
              break;
            }
            case NodeType.StringType:{
              status.Ident = null;
              status.basicType = CbType.String;
              break;
            }
            case NodeType.VoidType:{
              status.Ident = null;
              status.basicType = CbType.Void;
              break;
            }
            case NodeType.Static:{
              status.methodAttrs = TravelInfo.MethodAttrTag.Static;
              break;
            }
            case NodeType.Override:{
              status.methodAttrs = TravelInfo.MethodAttrTag.Override;
              break;
            }
            case NodeType.Virtual:{
              status.methodAttrs = TravelInfo.MethodAttrTag.Virtual;
              break;
            }
            default:
              SkipKary(n, data);
            break;
          }
        }
      }
    }

    public override void Visit(AST_nonleaf n, object data){
      switch (n.Tag){
        case NodeType.Class:{
          TravelInfo status = new TravelInfo();
          status.RequestReturn = true;
          String classname;
          CbClass basecls = null;
          n[0].Accept(this, status);
          classname = status.Ident;
          if (n[1] != null){
            n[1].Accept(this, status);
            basecls = (CbClass)NameSpace.TopLevelNames.LookUp(status.Ident);
          }
          CbClass cls = new CbClass(classname, basecls);
          status.InClass = cls;
          status.RequestReturn = false;
          status.InsideField = null;
          NameSpace.TopLevelNames.AddMember(cls);
          if (n[2] != null) n[2].Accept(this, status);
          break;
        }
        case NodeType.Field:{
          TravelInfo status = (TravelInfo)(data);
          //retrieve the type
          status.RequestReturn = true;
          n[0].Accept(this, status);
          CbType fieldType = DetermineType(status);
          status.InsideField = fieldType;
          //go to ident list to fill in members
          n[1].Accept(this, status);
          status.RequestReturn = false;
          status.InsideField = null;
          break;
        }
        case NodeType.Array:{
          TravelInfo status = (TravelInfo)(data);
          if (status.RequestReturn == true)
          status.IsArray = true;
          n[0].Accept(this, data);
          break;
        }
        case NodeType.Const:{
          TravelInfo status = (TravelInfo)(data);
          //retrieve the type
          status.RequestReturn = true;
          n[0].Accept(this, status);
          CbType fieldType = DetermineType(status);
          status.InsideConst = fieldType;
          //retrieve the identifier
          n[1].Accept(this, status);
          CbConst cons = new CbConst(status.Ident, fieldType);
          status.InClass.AddMember(cons);
          status.RequestReturn = false;
          status.InsideConst = null;
          break;
        }
        case NodeType.Method:{
          TravelInfo status = (TravelInfo)(data);
          //retrieve the method attr
          status.RequestReturn = true;
          n[0].Accept(this, data);
          //retrieve the method rt type
          n[1].Accept(this, data);
          CbType rtType = DetermineType(status);
          //retrieve the method name
          if(n[2] != null){
            n[2].Accept(this, data);
          }
          string methodname = status.Ident;
          //Create the method and dispatch to the formal list
          CbMethod method = new CbMethod(methodname, status.methodAttrs == TravelInfo.MethodAttrTag.Static, rtType, new List<CbType>());
          status.InsideMethod = method;
          if (n[3] != null){
            n[3].Accept(this, data);
          }

          status.InClass.AddMember(method);
          status.RequestReturn = false;
          n[4].Accept(this, data);
          status.InsideMethod = null;
          break;
        }
        default:{
          SkipNonleaf(n, data);
          break;
        }
      }
    }

    private void SkipKary(AST_kary n, object data){
      for (int i = 0; i < n.NumChildren; ++i){
        if (n[i] != null) n[i].Accept(this, data);
      }
    }

    private void SkipKary(AST_leaf n, object data){
      for (int i = 0; i < n.NumChildren; ++i){
        if (n[i] != null) n[i].Accept(this, data);
      }
    }

    private void SkipNonleaf(AST_nonleaf n, object data){
      for (int i = 0; i < n.NumChildren; ++i){
        if (n[i] != null) n[i].Accept(this, data);
      }
    }

    private CbType DetermineType(TravelInfo status){
      CbType result = null;
      //check if it's an ident
      if (status.Ident == null){
        result = status.basicType;
      }else{
        result = (CbType)NameSpace.TopLevelNames.LookUp(status.Ident);
        status.Ident = null;
      }
      //check if it's an array
      if (status.IsArray == true){
        CbType ArrayWarp = new CFArray(result);
        result = ArrayWarp;
        status.IsArray = false;
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
