/*  TCVisitor2.cs

    Second stage of full type-checking on AST

    We now visit and type-check all the parts of the AST which were not
    checked in the first stage of full type-checking.

    Author: Nigel Horspool

    Dates: 2012-2014
*/

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace FrontEnd {


public class TCVisitor2: Visitor {
    NameSpace ns;  // namespace for all top-level names and names opened with 'using' clauses
    CbClass currentClass;  // current class being checked (null if there isn't one)
    CbMethod currentMethod;  // current method being checked (null if there isn't one)
    SymTab sy;   // one instance of SymTab used for all method body checking
    int loopNesting;  // current depth of nesting of while loops

    // constructor
    public TCVisitor2( ){
      ns = NameSpace.TopLevelNames;  // get the top-level namespace
      currentMethod = null;
      sy = new SymTab();
      loopNesting = 0;
    }



    /*public void complain(int line_number, string message){
      System.Console.WriteLine("Error["+line_number+"]: "+message);
    }*/

    // Note: the data parameter for the Visit methods is never used
    // It is always null (or whatever is passed on the initial call)

  public override void Visit(AST_kary node, object data){
    switch(node.Tag){
    case NodeType.ClassList:
        // visit each class declared in the program
        for(int i=0; i<node.NumChildren; i++){
            node[i].Accept(this, data);
        }
        break;
    case NodeType.MemberList:
        // visit each member of the current class
        for(int i=0; i<node.NumChildren; i++){
            node[i].Accept(this, data);
        }
        break;
    case NodeType.Block:
        sy.Enter();
        // visit each statement or local declaration
        for(int i=0; i<node.NumChildren; i++){
            node[i].Accept(this, data);
        }
        sy.Exit();
        break;
    case NodeType.ActualList:
        for(int i=0; i<node.NumChildren; i++){
            node[i].Accept(this, data);
        }
        break;
      }
  }

  public override void Visit( AST_nonleaf node, object data ){
    switch(node.Tag){
      case NodeType.Program:
        node[1].Accept(this, data);  // visit class declarations
      break;

      case NodeType.Class:
        AST_leaf classNameId = node[0] as AST_leaf;
        string className = classNameId.Sval;
        currentClass = ns.LookUp(className) as CbClass;
        Debug.Assert(currentClass != null);
        performParentCheck(currentClass,node.LineNumber);  // check Object is ultimate ancestor
        // now check the class's members
        AST_kary memberList = node[2] as AST_kary;
        for(int i=0; i<memberList.NumChildren; i++){
          memberList[i].Accept(this,data);
        }
        currentClass = null;
        break;

        case NodeType.Const:
          node[0].Accept(this,data); // Get type of value
          node[2].Accept(this,data); // get assignment value
          if(!isAssignmentCompatible(node[0].Type,node[2].Type)){
            Start.SemanticError(node.LineNumber, "invalid initialization for const");
          }
        break;

        case NodeType.Field:

        break;

        case NodeType.Method:
          // get the method's type description
          string methname = ((AST_leaf)(node[1])).Sval;
          currentMethod = currentClass.Members[methname] as CbMethod;
          sy.Empty();
          // add each formal parameter to the symbol table
          AST_kary formals = (AST_kary)node[2];
          for(int i=0; i<formals.NumChildren; i++){
              AST_nonleaf formal = (AST_nonleaf)formals[i];
              string name = ((AST_leaf)formal[1]).Sval;
              SymTabEntry newBinding = sy.Binding(name, formal[1].LineNumber);
              newBinding.Type = formal[0].Type;
          }
          sy.Enter();
          // now type-check the method body
          node[3].Accept(this,data);
          // finally check that static/virtual/override are used correctly
          checkOverride(node);
          currentMethod = null;
        break;

        case NodeType.LocalDecl:
          node[0].Accept(this,data);  // get type for the locals
          AST_kary locals = node[1] as AST_kary;
          for(int i=0; i<locals.NumChildren; i++){
              AST_leaf local = locals[i] as AST_leaf;
              string name = local.Sval;
              SymTabEntry en = sy.Binding(name, local.LineNumber);
              en.Type = node[0].Type;
          }
          break;

        case NodeType.Assign:
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(node[0].Kind != CbKind.Variable)
              Start.SemanticError(node.LineNumber, "target of assignment is not a variable");
          if(!isAssignmentCompatible(node[0].Type, node[1].Type))
              Start.SemanticError(node.LineNumber, "invalid types in assignment statement");
        break;

        case NodeType.If: // Done
          node[0].Accept(this,data);
          CbType nodeType_If = node[0].Type;
          if(node[0].Type != CbType.Bool){
            Start.SemanticError(node.LineNumber,"If statement conditional must evaluate to type int not type {0}",nodeType_If.ToString());
          }
          node[1].Accept(this,data);
          node[2].Accept(this,data);
        break;

        case NodeType.While: // Done
          node[0].Accept(this,data);
          CbType nodeType_While = node[0].Type;
          if(node[0].Type != CbType.Bool){
            Start.SemanticError(node.LineNumber,"While statement condition must evaluate to type int not type {0}",nodeType_While.ToString());
          }
          loopNesting++;
          node[1].Accept(this,data);
          loopNesting--;
        break;

        case NodeType.Return: // CHECK ME, DONE?
          if(node[0] == null){
            if(currentMethod.ResultType != CbType.Void){
              Start.SemanticError(node.LineNumber, "missing return value for method :'(");
            }
            break;
          }
          node[0].Accept(this,data);

          // Check if current type of value == method type
          if(node[0].Type != currentMethod.ResultType){
            // if not, check if the return value is compatible w/ return type
            if( !isAssignmentCompatible(currentMethod.ResultType, node[0].Type) ){
              Start.SemanticError(node.LineNumber, String.Format("expected return type of {0} but got type of {1} instead.",currentMethod.ResultType.ToString(),node[0].Type.ToString()));
              node.Type = CbType.Error;
              break;
            }
          }
          node.Type = node[0].Type;
        break;

        case NodeType.Call: // CHECK ME, DONE?
          node[0].Accept(this,data); // method name (could be a dotted expression)
          node[1].Accept(this,data); // actual parameters           

          // Make list to keep track of chain dot calls
          IList<AST> chainCall = new List<AST>();
          
          // Traverse through Call tree to get names of dot calls
          AST nodePtr = node[0];
          for(;;)
          {
                if (nodePtr.Tag == NodeType.Dot){
                    chainCall.Add(nodePtr[1]);
                }else if (nodePtr.Tag == NodeType.Index){
                    Console.WriteLine("Not tested yet");
                    chainCall.Add(nodePtr[1]);
                }else{
                    //Console.Write(nodePtr.Tag);
                    chainCall.Add(nodePtr);
                    break;
                }
                nodePtr = nodePtr[0]; // traverse the pointer
          }
            
          // Look up caller in symbol table
          SymTabEntry symbol = sy.LookUp(((AST_leaf)chainCall[chainCall.Count-1]).Sval);
          if(symbol == null){
           //complain(node.LineNumber,"unknown method name'"+((AST_leaf)node[0][1]).Sval+"'");
            node.Type = CbType.Error;
            break;
          }

          // Check call has same number of parameters as method definition
          AST_kary meth_params = (AST_kary)node[1];
          CbMethodType tMeth = symbol.Type as CbMethodType;
          
          // if true, then tMeth must be an object making the call
          if (tMeth == null)
          {
            CbClass someClass = symbol.Type as CbClass;
            Console.WriteLine("Name: "+someClass.Name);
            
            // search the associated method for the class
            CbMethod methd = someClass.FindMember(((AST_leaf)chainCall[0]).Sval) as CbMethod;
            if (methd == null){
                Start.SemanticError(node.LineNumber,"{0}: No such method in class {1} to be called",((AST_leaf)chainCall[0]).Sval,someClass.Name);
                node.Type = CbType.Error;
                break;
            }
            tMeth = methd.Type as CbMethodType;
          }
          
          if(meth_params.NumChildren != tMeth.Method.ArgType.Count){
            Start.SemanticError(node.LineNumber,"Number of arguments does not match the number in the method definition.");
            node.Type = CbType.Error;
            break;
          }

          // Compare types
          int incompatType = 0;
          for (int i=0; i<meth_params.NumChildren; i++){
            if( isAssignmentCompatible(meth_params[i].Type, tMeth.Method.ArgType[i]) == false)
              incompatType = 1;
          }

          // Output error msg for incompatible types
          if(incompatType != 0){
            Start.SemanticError(node.LineNumber,"Call arguments have incompatible types.");
            node.Type = CbType.Error;
            break;
          }
          node.Type = tMeth.Method.ResultType;
        break;

        case NodeType.Dot:
          node[0].Accept(this,data);
          string rhs = ((AST_leaf)node[1]).Sval;
          CbClass lhstype = node[0].Type as CbClass;
          if(lhstype != null){
            // rhs needs to be a member of the lhs class
            CbMember mem;
            if(lhstype.Members.TryGetValue(rhs,out mem)){
              node.Type = mem.Type;
              if(mem is CbField){
                node.Kind = CbKind.Variable;
              }
              if(node[0].Kind == CbKind.ClassName){
                // mem has to be a static member
                if(!mem.IsStatic){
                  Start.SemanticError(node[1].LineNumber, "static member required");
                }
              }else{
                // mem has to be an instance member
                if(mem.IsStatic){
                  Start.SemanticError(node[1].LineNumber, "member cannot be accessed via a reference, use classname instead");
                  }
                }
              }else{
                Start.SemanticError(node[1].LineNumber, "member {0} not found in class {1}", rhs, lhstype.Name);
                node.Type = CbType.Error;
              }
            break;
          }

          if(rhs == "Length"){
            // lhs has to be an array or a string
            if(node[0].Type != CbType.String && !(node[0].Type is CFArray))
                Start.SemanticError(node[1].LineNumber, "member Length not found");
            node.Type = CbType.Int;
            break;
          }
          CbNameSpaceContext lhsns = node[0].Type as CbNameSpaceContext;
          if(lhsns != null){
            lhstype = lhsns.Space.LookUp(rhs) as CbClass;
            if(lhstype != null){
              node.Type = lhstype;
              node.Kind = CbKind.ClassName;
              break;
            }
          }
          Start.SemanticError(node[1].LineNumber, "member {0} does not exist", rhs);
          node.Type = CbType.Error;
        break;

        case NodeType.Cast:
          checkTypeSyntax(node[0]);
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(!isCastable(node[0].Type, node[1].Type)){
            Start.SemanticError(node[1].LineNumber, "invalid cast");
          }
          node.Type = node[0].Type;
        break;

        case NodeType.NewArray: // CHECK ME
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          /* TODO: ... check types */

          //check array type
          if(node[0].Type != CbType.Int && node[0].Type != CbType.Char && node[0].Type != CbType.String){
            // Check if it is a classname
            if(node[0].Kind != CbKind.ClassName){
              Start.SemanticError(node[0].LineNumber, "Invalid type for the array");
              node.Type = CbType.Error;
            }
          }

          //check size type
          if(node[1].Type != CbType.Int){
              Start.SemanticError(node[1].LineNumber, "Array size value must be type Int");
              node.Type = CbType.Error;
          }else{
            node.Type = CbType.Array(node[0].Type);
          }
        break;

        case NodeType.NewClass: // Done?
          node[0].Accept(this,data);
          /* TODO ... check that operand is a class */
          if(!(node[0].Type is CbClass)){
              Start.SemanticError(node[0].LineNumber, "{0} is not a class type");
              node.Type = CbType.Error;
          }else{
              node.Type = node[0].Type;
          }
        break;

        case NodeType.PlusPlus: // FIX ME
        case NodeType.MinusMinus: // FIX ME
          node[0].Accept(this,data);// CHECK ME
          /* TODO ... check types and operand must be a variable */
          //Make sure it's an integer type
          if(!isIntegerType(node[0].Type)){
              Start.SemanticError(node[0].LineNumber, "decrement and increment can only be done on integer variables");
              node.Type = CbType.Error;
          }else if(node[0].Kind != CbKind.Variable){ //ensure it's a variable
              Start.SemanticError(node[0].LineNumber, "decrement and increment can only be done on variables");
              node.Type = CbType.Error;
          }else{
              node.Type = node[0].Type;
          }
        break;

        case NodeType.UnaryPlus: // FIX ME
          node[0].Accept(this,data);
          if(isIntegerType(node[0].Type)){
            node.Type = CbType.Int;
          }else{
            Start.SemanticError(node.LineNumber, "UNARYPLUS: Incompatible type: {0}", node[0].Type);
            node.Type = CbType.Error;
          }
        break;

        case NodeType.UnaryMinus: // FIX ME
          node[0].Accept(this,data);
          if(isIntegerType(node[0].Type)){
            node.Type = CbType.Int;
          }else{
            Start.SemanticError(node.LineNumber, "UNARYMINUS: Incompatible type: {0}", node[0].Type);
            node.Type = CbType.Error;
          }
        break;

        case NodeType.Index: // FIX ME
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          /* TODO ... check types */
          node.Type = CbType.Error;  // FIX THIS
        break;

        case NodeType.Add: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) == true && isIntegerType(node[1].Type) == true){
            node.Type = CbType.Int;
          }else if(isStringConcatType(node[0].Type, node[1].Type)){
            node.Type = CbType.String;
          }else{
            Start.SemanticError(node.LineNumber, "ADD: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
          }
        break;

        case NodeType.Sub: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "SUB: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
          }
          node.Type = CbType.Int;
        break;

        case NodeType.Mul: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "MUL: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
          }
          node.Type = CbType.Int;
        break;

        case NodeType.Div: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "DIV: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
          }
          node.Type = CbType.Error;
        break;

        case NodeType.Mod: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "MOD: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
          }
          node.Type = CbType.Int;
        break;

        case NodeType.Equals: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);

          if( !(node[0].Type == node[1].Type) ){
            if( !(isIntegerType(node[0].Type) && isIntegerType(node[0].Type)) )
            {
            Start.SemanticError(node.LineNumber, "EQUALS: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            }
          }
        node.Type = CbType.Bool;
        break;

        case NodeType.NotEquals: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);

          if( !(node[0].Type == node[1].Type) ){
            if( !(isIntegerType(node[0].Type) && isIntegerType(node[0].Type)) ){
              Start.SemanticError(node.LineNumber, "NOTEQUALS: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            }
          }
          node.Type = CbType.Bool;
        break;

        case NodeType.LessThan: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "LESSTHAN: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
            break;
          }
          node.Type = CbType.Bool;
        break;

        case NodeType.GreaterThan: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true)
          {
            Start.SemanticError(node.LineNumber, "GREATERTHAN: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
            break;
          }
          node.Type = CbType.Bool;
        break;

        case NodeType.LessOrEqual: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "LESSOREQUAL: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
            break;
          }
          node.Type = CbType.Bool;
        break;
        case NodeType.GreaterOrEqual: // done
          node[0].Accept(this,data);
          node[1].Accept(this,data);
          if(isIntegerType(node[0].Type) != true || isIntegerType(node[1].Type) != true){
            Start.SemanticError(node.LineNumber, "GREATEROREQUAL: Incompatible types for: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
            break;
          }
          node.Type = CbType.Bool;
        break;

        case NodeType.And: // done
          // Traverse children first
          node[0].Accept(this,data);
          node[1].Accept(this,data);

          if(node[0].Type != CbType.Bool && node[1].Type != CbType.Bool)
          {
            Start.SemanticError(node.LineNumber, "ANDAND: Incompatible types: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
            break;
          }
          node.Type = CbType.Bool;
        break;

        case NodeType.Or: // done
          // Traverse children first
            node[0].Accept(this,data);
            node[1].Accept(this,data);

          if(node[0].Type != CbType.Bool && node[1].Type != CbType.Bool){
            Start.SemanticError(node.LineNumber, "OROR: Incompatible types: {0}, {1}", node[0].Type, node[1].Type);
            node.Type = CbType.Error;
            break;
          }
          node.Type = CbType.Bool;
        break;

        default:
          throw new Exception("Unexpected tag: "+node.Tag);
      }
    }

  public override void Visit(AST_leaf node, object data){
      switch(node.Tag){
      case NodeType.Ident:
        string name = node.Sval;
        SymTabEntry local = sy.LookUp(name);
        if(local != null){
            node.Type = local.Type;
            node.Kind = CbKind.Variable;
            return;
        }
        CbMember mem;
        if(currentClass.Members.TryGetValue(name,out mem)){
            node.Type = mem.Type;
            if(mem is CbField)
                node.Kind = CbKind.Variable;
            break;
        }
        CbClass t = ns.LookUp(name) as CbClass;
        if(t != null){
            node.Type = t;
            node.Kind = CbKind.ClassName;
            break;
        }
        NameSpace lhsns = ns.LookUp(name) as NameSpace;
        if(lhsns != null){
            node.Type = new CbNameSpaceContext(lhsns);
            break;
        }
        node.Type = CbType.Error;;
        Start.SemanticError(node.LineNumber, "{0} is unknown", name);
      break;

      case NodeType.Break:
        if(loopNesting <= 0){
          Start.SemanticError(node.LineNumber, "break can only be used inside a loop");
        }
      break;

      case NodeType.Null:
        node.Type = CbType.Null;
      break;
      case NodeType.IntConst:
        node.Type = CbType.Int;
      break;
      case NodeType.StringConst:
        node.Type = CbType.String;
      break;
      case NodeType.CharConst:
        node.Type = CbType.Char;
      break;
      case NodeType.Empty:
      break;
      case NodeType.IntType:
        node.Type = CbType.Int;
      break;
      case NodeType.CharType:
        node.Type = CbType.Char;
      break;
      case NodeType.StringType:
        node.Type = CbType.String;
      break;
      default:
        throw new Exception("Unexpected tag: "+node.Tag);
      }
    }

    private void performParentCheck(CbClass c, int lineNumber){//Definately CHECK ME
      /* TODO
         code to check that c's ultimate ancestor is Object.
         Be careful not to get stuck if the parent relationship
         contains a cycle.
         The lineNumber parameter is used in error messages.
      */
      List<CbClass> InheritingPath = new List<CbClass>();
      CbClass ptr = c;
      do{
        InheritingPath.Add(ptr);
        ptr = ptr.Parent;
        if(ptr == CbType.Object){
          //success
          break;
        }

        if(ptr == null){
          //fail
          Start.SemanticError(lineNumber, "Class inheriting path broken.");
          break;
        }

        foreach (CbClass ancestor in InheritingPath){
          if(ptr == ancestor){
            Start.SemanticError(lineNumber, "Circular inheritance detected.");
            break;
          }
        }
      }while(true);
    }


    private bool isAssignmentCompatible(CbType dest, CbType src){
      if(dest == CbType.Error || src == CbType.Error) return true;
      if(dest == src) return true;
      if(dest == CbType.Int) return isIntegerType(src);
      CbClass d = dest as CbClass;
      CbClass s = src as CbClass;
      if(d != null){
          if(src == CbType.Null) return true;
          if(s == null) return false;
          if(isAncestor(d,s)) return true;
      }
      return false;
    }

    private void checkTypeSyntax(AST n){
      /* TODO
         code to check whether n is the subtree that has appropriate AST
         structure for a Cb type. It could be a builtin type (int, char,
         string), a class, or an array whose elements have a valid type.
      */
      switch(n.Tag){
      case NodeType.IntType:
        break;
      case NodeType.CharType:
        break;
      case NodeType.StringType:
        break;
      case NodeType.Ident:
        String name = ((AST_leaf)n).Sval;
        CbClass t = ns.LookUp(name) as CbClass;
        if(t == null){
          Start.SemanticError(n.LineNumber, "Invalid Type");
        }
        break;
      default:
        Start.SemanticError(n.LineNumber, "Invalid Type");
        break;
      }
    }

    private bool isCastable(CbType dest, CbType src){
      if(isIntegerType(dest) && isIntegerType(src)) return true;
      if(dest == CbType.Error || src == CbType.Error) return true;
      CbClass d = dest as CbClass;
      CbClass s = src as CbClass;
      if(isAncestor(d,s)) return true;
      if(isAncestor(s,d)) return true;
      return false;
    }

    // returns true if type t can be used where an integer is needed
    private bool isIntegerType(CbType t){
      return t == CbType.Int || t == CbType.Char || t == CbType.Error;
    }

  // returns true if type t1 and t2 are compatible for string concat
  private bool isStringConcatType(CbType t1, CbType t2){
    if(t1 == CbType.String){
      if(isIntegerType(t2) || t2 == CbType.String)
        return true;
    }
    else if(t2 == CbType.String){
      if(isIntegerType(t1) || t1 == CbType.String)
        return true;
    }
    return false;
  }

    // tests if T1 == T2 or T1 is an ancestor of T2 in hierarchy
    private bool isAncestor( CbClass T1, CbClass T2 ){
      while(T1 != T2){
          T2 = T2.Parent;
          if(T2 == null) return false;
      }
      return true;
    }

    private void checkOverride(AST_nonleaf node){
      // search for a member in any ancestor with same name
      /* TODO
         code to check whether any ancestor class contains a member with
         the same name. If so, it has to be a method with the identical
         signature.
         If there is a method with the same signature, then neither method
         is allowed to be static. (Not part of Cb language.)
         Otherwise, currentMethod must be flagged as override (not virtual).
      */

      string name = currentMethod.Name;
      NodeType child_indTag = node[node.NumChildren - 1].Tag;
      bool hasAncestor = false;
      CbClass checkAncestor = currentClass.Parent;
      do{
        if(checkAncestor == null){
          break;
        }
        if(checkAncestor.Members.ContainsKey(name)){
          CbMember anc_curr = checkAncestor.Members[name];
          if(anc_curr is CbMethod){
            hasAncestor = true;
            CbMethod anc_check = anc_curr as CbMethod;
            if(currentMethod.ResultType != anc_check.ResultType){
              Start.SemanticError(node[0].LineNumber, "{0}: shares a name with a method in a parent class witha  different signature", name);
              break;
            } else if(anc_check.IsStatic || currentMethod.IsStatic){
              Start.SemanticError(node[0].LineNumber, "static methods cannot override or be overridden");
              break;
            } else if(child_indTag != NodeType.Override){
              Start.SemanticError(node[0].LineNumber, "{0}: shares a name with a constant or field in a parent class", name);
            }
          }
        }
        checkAncestor = checkAncestor.Parent;
      } while (true);
      if(!(hasAncestor) && (child_indTag == NodeType.Override)){
        Start.SemanticError(node[0].LineNumber, "{0}: no method found in a parent class to override", name);
      }
    }



    private string printTagToStr(AST node){
      StringBuilder builder = new StringBuilder();
      StringWriter writer = new StringWriter(builder);
      writer.Write("{0}:{1}_{2}", node.Tag, node.LineNumber, node.Type);
      return builder.ToString();
    }
}

}
