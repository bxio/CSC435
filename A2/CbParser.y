/* CbParser.y */

// The grammar has one shift-reduce conflict for the if-then-else ambiguity.
// As long as the parser shifts in the conflict state, the language will be
// parsed correctly.
//
// Because the cast operation syntax is hard to express as a LALR(1) grammar,
// the grammar rules below accept some syntax for expressions which is nonsense.
// There are two kinds of nonsense:
//  1.  Any expression can be used instead of a typename in a cast. For example:
//             (x+1)y
//  2.  An index can be omitted from an array access. For example,
//              a = ARR[]+2;
// A later semantic pass over the AST must check for both kinds of nonsense and
// produce error messages if discovered.
//
// The grammar tricks used to support casts were obtained from here:
//      http://msdn.microsoft.com/en-us/library/aa245175(v=vs.60).aspx
//
// Author:  Nigel Horspool
// Date:    June 2014

%namespace  FrontEnd
%tokentype  Tokens
%output=CbParser.cs
%YYSTYPE    AST     // set datatype of $$, $1, $2... attributes

// All tokens which can be used as operators in expressions
// they are ordered by precedence level (lowest first)
%right      '='
%left       OROR
%left       ANDAND
%nonassoc   EQEQ NOTEQ
%nonassoc   '>' GTEQ '<' LTEQ
%left       '+' '-'
%left       '*' '/' '%'

// All other named tokens (i.e. the single character tokens are omitted)
// The order in which they are listed here does not matter.

// Keywords
%token      Kwd_break Kwd_char Kwd_class Kwd_const Kwd_else Kwd_if Kwd_int
%token      Kwd_new Kwd_null Kwd_override Kwd_public Kwd_return
%token      Kwd_static Kwd_string Kwd_using Kwd_virtual Kwd_void Kwd_while

// Other tokens
%token      PLUSPLUS MINUSMINUS Ident CharConst IntConst StringConst


%%

/* *************************************************************************
   *                                                                       *
   *         PRODUCTION RULES AND ASSOCIATED SEMANTIC ACTIONS              *
   *                                                                       *
   ************************************************************************* */

// STOCK (Program)
Program:        UsingList ClassList
                { Tree = AST.NonLeaf(NodeType.Program, $1.LineNumber, $1, $2); }
        ;

// STOCK (UsingList)
UsingList:      /* empty */
          { $$ = AST.Kary(NodeType.UsingList, LineNumber); }
        |       UsingList Kwd_using Identifier ';'
          { $1.AddChild($3);  $$ = $1; }
        ;

// STOCK (ClassList)
ClassList:      ClassDecl
          { $$ = AST.Kary(NodeType.ClassList, LineNumber); }
        |       ClassList ClassDecl
          { $1.AddChild($2);  $$ = $1; }
        ;

// GOOD (Class)
ClassDecl:      Kwd_class Identifier  '{'  DeclList  '}'
                { $$ = AST.NonLeaf(NodeType.Class, $2.LineNumber, $2, null, $4); }
        |       Kwd_class Identifier  ':' Identifier  '{'  DeclList  '}'
                { $$ = AST.NonLeaf(NodeType.Class, $2.LineNumber, $2, $4, $6); }
        ;

// MAYBE (MemberList)
DeclList:       /* empty */
        {$$ = AST.Kary(NodeType.MemberList, LineNumber);}
        |       DeclList ConstDecl
        {$$ = AST.Kary(NodeType.MemberList, LineNumber, $1, $2);}
        |       DeclList FieldDecl
        {$$ = AST.Kary(NodeType.MemberList, LineNumber, $1, $2);}
        |       DeclList MethodDecl
        {$$ = AST.Kary(NodeType.MemberList, LineNumber, $1, $2);}
        ;

// GOOD (Const)
ConstDecl:      Kwd_public Kwd_const Type Identifier '=' InitVal ';'
        { $$ = AST.NonLeaf(NodeType.Const, $2.LineNumber, $3, $4, $6); }
        ;

// GOOD (Multiple Const)
InitVal:        IntConst
        { $$ = AST.Leaf(NodeType.IntConst, LineNumber, lexer.yytext); }
        |       CharConst
        { $$ = AST.Leaf(NodeType.CharConst, LineNumber, lexer.yytext); }
        |       StringConst
        { $$ = AST.Leaf(NodeType.StringConst, LineNumber, lexer.yytext); }
        ;

// GOOD (Field)
FieldDecl:      Kwd_public Type IdentList ';'
        { $$ = AST.NonLeaf(NodeType.Field, LineNumber, $2, $3); }
        ;

// MAYBE (IdList)
IdentList:      IdentList ',' Identifier
        { $1.AddChild($3); $$ = $1; }
        |       Identifier
        { $$ = AST.Kary(NodeType.IdList, LineNumber); $$.AddChild($1); }
        ;

// GOOD (Method)
MethodDecl:     Kwd_public MethodAttr MethodType Identifier '(' OptFormals ')' Block
        { $$ = AST.NonLeaf(NodeType.Method, $4.LineNumber, $3, $4, $6, $8, $2); }
        ;

// GOOD (method attr)
MethodAttr:     Kwd_static
        { $$ = AST.Leaf(NodeType.Static, LineNumber,lexer.yytext); }
        |       Kwd_virtual
        { $$ = AST.Leaf(NodeType.Virtual, LineNumber,lexer.yytext); }
        |       Kwd_override
        { $$ = AST.Leaf(NodeType.Override, LineNumber,lexer.yytext); }
        ;

// GOOD (VoidType)
MethodType:     Kwd_void
        { $$ = AST.Leaf(NodeType.VoidType, LineNumber, lexer.yytext); }
        |       Type
        { $$ = $1; }
        ;

// GOOD (non-terminal)
OptFormals:     /* empty */
        { $$ = null; }
        |       FormalPars
        { $$ = $1; }
        ;

// GOOD (FormalList)
FormalPars:     FormalDecl
        { $$ = AST.Kary(NodeType.FormalList,$1.LineNumber, $1); }
        |       FormalPars ',' FormalDecl
        { $1.AddChild($3); $$ = $1; }
        ;

// GOOD (Formal)
FormalDecl:     Type Identifier
        { $$ = AST.NonLeaf(NodeType.Formal, LineNumber, $1, $2); }
        ;

// GOOD (Array)
Type:           TypeName
        { $$ = $1; }
        |       TypeName '[' ']'
        { $$ = AST.NonLeaf(NodeType.Array, LineNumber, $1); }
        ;

// GOOD (Non-terminal)
TypeName:       Identifier
        { $$ = $1; }
        |       BuiltInType
        { $$ = $1; }
        ;

// GOOD (Built-in Types)
BuiltInType:    Kwd_int
        { $$ = AST.Leaf(NodeType.IntType, LineNumber); }
        |       Kwd_string
        { $$ = AST.Leaf(NodeType.StringType, LineNumber); }
        |       Kwd_char
        { $$ = AST.Leaf(NodeType.CharType, LineNumber); }
        ;

// GOOD (Multiple NodeTypes)
Statement:      Designator '=' Expr ';'
        { $$ = AST.NonLeaf(NodeType.Assign, LineNumber, $1, $3); }
        |       Designator '(' OptActuals ')' ';'
        { $$ = AST.NonLeaf(NodeType.Call, LineNumber, $1, $3); }
        |       Designator PLUSPLUS ';'
        { $$ = AST.NonLeaf(NodeType.PlusPlus, LineNumber, $1); }
        |       Designator MINUSMINUS ';'
        { $$ = AST.NonLeaf(NodeType.MinusMinus, LineNumber, $1); }
        |       Kwd_if '(' Expr ')' Statement Kwd_else Statement
        { $$ = AST.NonLeaf(NodeType.If, LineNumber, $3, $5, $7); }
        |       Kwd_if '(' Expr ')' Statement
        { $$ = AST.NonLeaf(NodeType.If, LineNumber, $3, $5, null); }
        |       Kwd_while '(' Expr ')' Statement
        { $$ = AST.NonLeaf(NodeType.While, LineNumber, $3, $5); }
        |       Kwd_break ';'
        { $$ = AST.Leaf(NodeType.Break, LineNumber); }
        |       Kwd_return ';'
        { $$ = AST.NonLeaf(NodeType.Return, LineNumber, null); }
        |       Kwd_return Expr ';'
        { $$ = AST.Kary(NodeType.Return, LineNumber, $2); }
        |       Block
    { $$ = AST.Kary(NodeType.Block, LineNumber, $1); }
        |       ';'
    { $$ = AST.Leaf(NodeType.Empty, LineNumber); }
        ;

// GOOD (non-terminal)
OptActuals:     /* empty */
        { $$ = null; }
        |       ActPars
        { $$ = $1; }
        ;

// GOOD (ActualList)
ActPars:        ActPars ',' Expr
        { $1.AddChild($3); $$ = $1; }
        |       Expr
        { $$ = AST.Kary(NodeType.ActualList, LineNumber, $1); $$.AddChild($1); }
        ;

// TEST
Block:          '{' DeclsAndStmts '}'
        { $$ = $2; }
        ;

// TEST (LocalDecl)
LocalDecl:      TypeName IdentList ';'
        { $$ = AST.NonLeaf(NodeType.LocalDecl, LineNumber, $1, $2); }
        |       Identifier '[' ']' IdentList ';'
        { AST typehere = AST.NonLeaf(NodeType.Array, LineNumber, $1);
          $$ = AST.NonLeaf(NodeType.LocalDecl, LineNumber, typehere, $4); }
        |       BuiltInType '[' ']' IdentList ';'
        { AST typehere = AST.NonLeaf(NodeType.Array, LineNumber, $1);
          $$ = AST.NonLeaf(NodeType.LocalDecl, LineNumber, typehere, $4); }
        ;
// TEST
DeclsAndStmts:   /* empty */
        { $$ = AST.Kary(NodeType.Block, LineNumber); }
        |       DeclsAndStmts Statement
        { $1.AddChild($2); $$ = $1; }
        |       DeclsAndStmts LocalDecl
        { $1.AddChild($2); $$ = $1; }
        ;

// GOOD
Expr:   Expr OROR Expr
      { $$ = AST.NonLeaf(NodeType.Or, LineNumber, $1, $3); }
    |   Expr ANDAND Expr
      { $$ = AST.NonLeaf(NodeType.And, LineNumber, $1, $3); }
    |   Expr EQEQ Expr
      { $$ = AST.NonLeaf(NodeType.Equals, LineNumber, $1, $3); }
    |   Expr NOTEQ Expr
      { $$ = AST.NonLeaf(NodeType.NotEquals, LineNumber, $1, $3); }
    |   Expr LTEQ Expr
      { $$ = AST.NonLeaf(NodeType.LessOrEqual, LineNumber, $1, $3); }
    |   Expr '<' Expr
      { $$ = AST.NonLeaf(NodeType.LessThan, LineNumber, $1, $3); }
    |   Expr GTEQ Expr
      { $$ = AST.NonLeaf(NodeType.GreaterOrEqual, LineNumber, $1, $3); }
    |   Expr '>' Expr
      { $$ = AST.NonLeaf(NodeType.GreaterThan, LineNumber, $1, $3); }
    |   Expr '+' Expr
      { $$ = AST.NonLeaf(NodeType.Add, LineNumber, $1, $3); }
    |   Expr '-' Expr
      { $$ = AST.NonLeaf(NodeType.Sub, LineNumber, $1, $3); }
    |   Expr '*' Expr
      { $$ = AST.NonLeaf(NodeType.Mul, LineNumber, $1, $3); }
    |   Expr '/' Expr
      { $$ = AST.NonLeaf(NodeType.Div, LineNumber, $1, $3); }
    |   Expr '%' Expr
      { $$ = AST.NonLeaf(NodeType.Mod, LineNumber, $1, $3); }
    |       UnaryExpr
    ;

// GOOD
UnaryExpr:      '-' Expr
        { $$ = AST.NonLeaf(NodeType.UnaryMinus, LineNumber, $2); }
        //|       '+' Expr
        //{ $$ = AST.NonLeaf(NodeType.UnaryPlus, LineNumber, $2); }
        |       UnaryExprNotUMinus
        {$$ = $1;}
        ;
// TESTME
UnaryExprNotUMinus:
                Designator
        { $$ = $1; }
        |       Designator '(' OptActuals ')'
        { $$ = AST.NonLeaf(NodeType.Call, LineNumber, $1, $3); }
        |       Kwd_null
        { $$ = AST.Leaf(NodeType.Null, LineNumber); }
        |       IntConst
        { $$ = AST.Leaf(NodeType.IntConst, LineNumber, lexer.yytext); }
        |       CharConst
        { $$ = AST.Leaf(NodeType.CharConst, LineNumber, lexer.yytext); }
        |       StringConstant
        { $$ = $1; }
        |       StringConstant '.' Identifier // Identifier must be "Length"
        { $$ = AST.NonLeaf(NodeType.Dot, LineNumber, $1, $3); }
        |       Kwd_new Identifier '(' ')'
        { $$ = AST.NonLeaf(NodeType.NewClass, LineNumber, $2); }
        |       Kwd_new TypeName '[' Expr ']'
        { $$ = AST.NonLeaf(NodeType.NewArray, LineNumber, $2, $4); }
        |       '(' Expr ')'
        { $$ = $2; }
        |       '(' Expr ')' UnaryExprNotUMinus                 // cast
        { $$ = AST.NonLeaf(NodeType.Cast, LineNumber, $2, $4); }
        |       '(' BuiltInType ')' UnaryExprNotUMinus          // cast
        { $$ = AST.NonLeaf(NodeType.Cast, LineNumber, $2, $4); }
        |       '(' BuiltInType '[' ']' ')' UnaryExprNotUMinus  // cast
        { AST typehere = AST.NonLeaf(NodeType.Array, LineNumber, $2); $$ = AST.NonLeaf(NodeType.Cast, $2.LineNumber, typehere, $6); }
        ;

// TESTME
StringConstant: StringConst
        { $$ = AST.Leaf(NodeType.StringConst, LineNumber, lexer.yytext); }
        ;

// TESTME
Designator:     Identifier Qualifiers
        { $$ = AST.NonLeaf(NodeType.Designator, LineNumber, $1, $2); }
        ;

// TESTME
Qualifiers:     '.' Identifier Qualifiers
        {AST DotNote = AST.NonLeaf(NodeType.Dot, LineNumber, null, $2); $3.AddChild(DotNote); $$ = $3; }
        |       '[' Expr ']' Qualifiers
        { AST indexNote = AST.NonLeaf(NodeType.Index, LineNumber, null, $2); $4.AddChild(indexNote); $$ = $4; }
        |       '[' ']' Qualifiers   // needed for cast syntax
        //What is this heck?
        { AST arrayNote = AST.Leaf(NodeType.Array, LineNumber); $3.AddChild(arrayNote); $$ = $3; }
        |       /* empty */
        { $$ = AST.Kary(NodeType.Qualifiers, LineNumber); }
        ;

// TESTME
Identifier:     Ident   { $$ = AST.Leaf(NodeType.Ident, LineNumber, lexer.yytext); }
        ;

%%

// returns the AST constructed for the Cb program
public AST Tree { get; private set; }

private Scanner lexer;

// returns the lexer's current line number
public int LineNumber {
    get{ return lexer.LineNumber == 0? 1 : lexer.LineNumber; }
}

// Use this function for reporting non-fatal errors discovered
// while parsing and building the AST.
// An example usage is:
//    yyerror( "Identifier {0} has not been declared", idname );
public void yyerror( string format, params Object[] args ) {
    Console.Write("{0}: ", LineNumber);
    Console.WriteLine(format, args);
}

// The parser needs a suitable constructor
public Parser( Scanner src ) : base(null) {
    lexer = src;
    Scanner = src;
}


