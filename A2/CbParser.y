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

Program:        UsingList ClassList
                { Tree = AST.NonLeaf(NodeType.Program, $1.LineNumber, $1, $2); }
        ;

UsingList:      /* empty */
			    { $$ = AST.Kary(NodeType.UsingList, LineNumber); }
        |       UsingList Kwd_using Identifier ';'
			    { $1.AddChild($3);  $$ = $1; }
        ;

ClassList:      ClassDecl
			    { $$ = AST.Kary(NodeType.ClassList, LineNumber); }
        |       ClassList ClassDecl
			    { $1.AddChild($2);  $$ = $1; }
        ;

ClassDecl:      Kwd_class Identifier  '{'  DeclList  '}'
                { $$ = AST.NonLeaf(NodeType.Class, $2.LineNumber, $2, null, $4); }
        |       Kwd_class Identifier  ':' Identifier  '{'  DeclList  '}'
                { $$ = AST.NonLeaf(NodeType.Class, $2.LineNumber, $2, $4, $6); }
        ;

DeclList:       /* empty */
		//		{$$ = null;}
        |       DeclList ConstDecl
		//		{$1.AddChild($2); $$ = $1;}
        |       DeclList FieldDecl
		//		{$1.AddChild($2); $$ = $1;}
        |       DeclList MethodDecl
		//		{$1.AddChild($2); $$ = $1;}
        ;
ConstDecl:      Kwd_public Kwd_const Type Identifier '=' InitVal ';'
				{ $$ = AST.NonLeaf(NodeType.Class, $2.LineNumber, $3, $4, $6); }
        ;

// FIXME
InitVal:        IntConst
		//		{ $$ = AST.Leaf(NodeType.IntConst, $1.LineNumber, $1); }
        |       CharConst
		//		{ $$ = AST.Leaf(NodeType.IntConst, $1.LineNumber, $1); }
        |       StringConst
		//		{ $$ = AST.Leaf(NodeType.IntConst, $1.LineNumber, $1); }
        ;

FieldDecl:      Kwd_public Type IdentList ';'
		//		{ $$ = AST.NonLeaf(NodeType.Field, $2.LineNumber, $2, $3); }
        ;

IdentList:      IdentList ',' Identifier
				{ $1.AddChild($3); $$ = $1; }
        |       Identifier
        { $$ = AST.Kary(NodeType.IdList, LineNumber); $$.AddChild($1); }
        ;

MethodDecl:     Kwd_public MethodAttr MethodType Identifier '(' OptFormals ')' Block
				{ $$ = AST.NonLeaf(NodeType.Method, LineNumber, null, $4, $6, $8); }
        ;

MethodAttr:     Kwd_static
        |       Kwd_virtual
        |       Kwd_override
        ;

MethodType:     Kwd_void
        |       Type
        ;

OptFormals:     /* empty */
        |       FormalPars
        ;

FormalPars:     FormalDecl
        |       FormalPars ',' FormalDecl
        ;

FormalDecl:     Type Identifier
				{ $$ = AST.NonLeaf(NodeType.Formal, LineNumber, $1, $2); }
        ;

Type:           TypeName
				{ $$ = $1; }
        |       TypeName '[' ']'
        { $$ = AST.NonLeaf(NodeType.Array, LineNumber, $1); }
        ;

TypeName:       Identifier
        |       BuiltInType
        ;

BuiltInType:    Kwd_int
        |       Kwd_string
        |       Kwd_char
        ;

Statement:      Designator '=' Expr ';'
        |       Designator '(' OptActuals ')' ';'
        { $$ = AST.NonLeaf(NodeType.Call, LineNumber, $1, $3); }
        |       Designator PLUSPLUS ';'
        { $$ = AST.NonLeaf(NodeType.PlusPlus, LineNumber, $1); }
        |       Designator MINUSMINUS ';'
        { $$ = AST.NonLeaf(NodeType.MinusMinus, LineNumber, $1); }
        |       Kwd_if '(' Expr ')' Statement Kwd_else Statement
        { $$ = AST.NonLeaf(NodeType.If, LineNumber, $3, $5, $6); }
        |       Kwd_if '(' Expr ')' Statement
        { $$ = AST.NonLeaf(NodeType.While, LineNumber, $3, $5); }
        |       Kwd_while '(' Expr ')' Statement
        { $$ = AST.NonLeaf(NodeType.While, LineNumber, $3, $5); }
        |       Kwd_break ';'
        { $$ = AST.Leaf(NodeType.Break, LineNumber); }
        |       Kwd_return ';'
        { $$ = AST.Kary(NodeType.Return, LineNumber); }
        |       Kwd_return Expr ';'
        { $$ = AST.Kary(NodeType.Return, LineNumber); $$.AddChild($2); }
        |       Block
        |       ';'
        ;

OptActuals:     /* empty */
				{ $$ = AST.Leaf(NodeType.Empty, LineNumber); }
        |       ActPars
        ;

ActPars:        ActPars ',' Expr
        |       Expr
        ;

Block:          '{' DeclsAndStmts '}'
        ;

LocalDecl:      TypeName IdentList ';'
        |       Identifier '[' ']' IdentList ';'
        |       BuiltInType '[' ']' IdentList ';'
        ;

DeclsAndStmts:   /* empty */
        |       DeclsAndStmts Statement
        |       DeclsAndStmts LocalDecl
        ;

Expr:		  Expr OROR Expr
		{ $$ = AST.NonLeaf(NodeType.Or, LineNumber, $1, $3); }
		| Expr ANDAND Expr
		{ $$ = AST.NonLeaf(NodeType.And, LineNumber, $1, $3); }
		| Expr EQEQ Expr
		{ $$ = AST.NonLeaf(NodeType.Equals, LineNumber, $1, $3); }
		| Expr NOTEQ Expr
		{ $$ = AST.NonLeaf(NodeType.NotEquals, LineNumber, $1, $3); }
		| Expr LTEQ Expr
		{ $$ = AST.NonLeaf(NodeType.LessOrEqual, LineNumber, $1, $3); }
		| Expr '<' Expr
		{ $$ = AST.NonLeaf(NodeType.LessThan, LineNumber, $1, $3); }
		| Expr GTEQ Expr
		{ $$ = AST.NonLeaf(NodeType.GreaterOrEqual, LineNumber, $1, $3); }
		| Expr '>' Expr
		{ $$ = AST.NonLeaf(NodeType.GreaterThan, LineNumber, $1, $3); }
		| Expr '+' Expr
		{ $$ = AST.NonLeaf(NodeType.Add, LineNumber, $1, $3); }
		| Expr '-' Expr
		{ $$ = AST.NonLeaf(NodeType.Sub, LineNumber, $1, $3); }
		| Expr '*' Expr
		{ $$ = AST.NonLeaf(NodeType.Mul, LineNumber, $1, $3); }
		| Expr '/' Expr
		{ $$ = AST.NonLeaf(NodeType.Div, LineNumber, $1, $3); }
		| Expr '%' Expr
		{ $$ = AST.NonLeaf(NodeType.Mod, LineNumber, $1, $3); }
    |       UnaryExpr
    ;

UnaryExpr:      '-' Expr
        |       UnaryExprNotUMinus
        ;

UnaryExprNotUMinus:
                Designator
        |       Designator '(' OptActuals ')'
        |       Kwd_null
        |       IntConst
        |       CharConst
        |       StringConst
        |       StringConst '.' Identifier // Identifier must be "Length"
        |       Kwd_new Identifier '(' ')'
        |       Kwd_new TypeName '[' Expr ']'
        |       '(' Expr ')'
        |       '(' Expr ')' UnaryExprNotUMinus                 // cast
        |       '(' BuiltInType ')' UnaryExprNotUMinus          // cast
        |       '(' BuiltInType '[' ']' ')' UnaryExprNotUMinus  // cast

        ;

Designator:     Identifier Qualifiers
        ;

Qualifiers:     '.' Identifier Qualifiers
        |       '[' Expr ']' Qualifiers
        |       '[' ']' Qualifiers   // needed for cast syntax
        |       /* empty */
        ;

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


