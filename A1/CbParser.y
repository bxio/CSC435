/* CbParser.y */
%using System.IO;
// The grammar shown in this file is INCOMPLETE!!
// It does not support class inheritance, it does not permit
// classes to contain methods (other than Main).
// Other language features may be missing too.

%namespace  FrontEnd
%tokentype  Tokens
%using System.IO;

// All tokens which can be used as operators in expressions
// they are ordered by precedence level (lowest first)
%right      '='
%left       OROR
%left       ANDAND
%nonassoc   EQEQ NOTEQ
%nonassoc   '>' GTEQ '<' LTEQ
%left       '+' '-'
%left       '*' '/' '%'
%left       UMINUS

// All other named tokens (i.e. the single character tokens are omitted)
// The order in which they are listed here does not matter.
%token      Kwd_break Kwd_char Kwd_class Kwd_const Kwd_else Kwd_if Kwd_int
%token      Kwd_new Kwd_null Kwd_out Kwd_override Kwd_public Kwd_return
%token		Kwd_static Kwd_string Kwd_using Kwd_virtual Kwd_void Kwd_while
%token      PLUSPLUS MINUSMINUS Ident Number StringConst

%%

/* *************************************************************************
   *                                                                       *
   *         PRODUCTION RULES AND ASSOCIATED SEMANTIC ACTIONS              *
   *                                                                       *
   ************************************************************************* */

Program:        UsingList ClassList
        ;

UsingList:      /* empty */
        |       Kwd_using Ident ';' UsingList
        ;

ClassList:	    ClassList ClassDecl
		|		ClassDecl
		;

ClassDecl:		Kwd_class Ident '{'  DeclList  '}'
		|		Kwd_class Ident '{'  DeclList  '}' ':' Ident // inheritance
		;

DeclList:       DeclList ConstDecl
        |       DeclList MethodDecl
        //|       DeclList FieldDeclList
        |       /* empty */
        ;

ConstDecl:      Kwd_public Kwd_const Type Ident '=' InitVal ';'
        ;

InitVal:        Number
        |       StringConst
        ;

//FieldDeclList:  FieldDeclList FieldDecl
//        |       /* empty */
//        ;

//FieldDecl:      Kwd_public Type IdentList ';'
 //       ;

IdentList:      IdentList ',' Ident
        |       Ident
        ;

MethodDecl:     MethodPublic Kwd_static Type Ident '(' OptFormals ')' Block
		|		MethodPublic Kwd_virtual Type Ident '(' OptFormals ')' Block
		|		MethodPublic Kwd_override Type Ident '(' OptFormals ')' Block
		|		MethodPublic Ident '(' OptFormals ')' Block // constructor
		|		MethodPublic Type Ident '(' OptFormals ')' Block
        ;

MethodPublic:	Kwd_public
		|		/* empty */
		;
OptFormals:     /* empty */
        |       FormalPars
        ;

FormalPars:     FormalDecl
        |       FormalPars ',' FormalDecl
        ;

FormalDecl:     Type Ident
        ;

Type:           TypeName
        |       TypeName '[' ']'
        ;

TypeName:       Ident
        |       Kwd_int
        |       Kwd_string
		|		Kwd_char
        |       Kwd_void
        ;

Statement:      Designator '=' Expr ';'
        |       Designator '(' OptActuals ')' ';'
        |       Designator PLUSPLUS ';'
        |       Designator MINUSMINUS ';'
        |       Kwd_if '(' Expr ')' '{' Statement '}' OptElsePart
        |       Kwd_while '(' Expr ')' '{' Statement '}'
        |       Kwd_break ';'
        |       Kwd_return ';'
        |       Kwd_return Expr ';'
        |       Block
        |       ';'
        ;

OptActuals:     /* empty */
        |       ActPars
        ;

ActPars:        ActPars ',' Expr
        |       Expr
        ;

OptElsePart:    Kwd_else '{' Statement '}'
        |       /* empty */
        ;

Block:          '{' DeclsAndStmts '}'
        ;

LocalDecl:      Ident IdentList ';'
				| 			Type Ident '=' Expr ';'
        |       Ident '[' ']' IdentList ';'
				;

DeclsAndStmts:   /* empty */
        |       DeclsAndStmts '{' Statement '}'
        |       DeclsAndStmts LocalDecl
        ;

Expr:           Expr OROR Expr
        |       Expr ANDAND Expr
        |       Expr EQEQ Expr
        |       Expr NOTEQ Expr
        |       Expr LTEQ Expr
        |       Expr '<' Expr
        |       Expr GTEQ Expr
        |       Expr '>' Expr
        |       Expr '+' Expr
        |       Expr '-' Expr
        |       Expr '*' Expr
        |       Expr '/' Expr
        |       Expr '%' Expr
        |       '-' Expr %prec UMINUS
        |       Designator
        |       Designator '(' OptActuals ')'
        |       Number
        |       StringConst
        |       StringConst '.' Ident // Ident must be "Length"
        |       Kwd_new Ident '(' ')'
        |       Kwd_new Ident '[' Expr ']'
        |       '(' Expr ')'
        ;

Designator:     Ident Qualifiers
        ;

Qualifiers:     '.' Ident Qualifiers
        |       '[' Expr ']' Qualifiers
        |       /* empty */
        ;

%%
  //string filename;
  FrontEnd.Scanner lexer;

  // define our own constructor for the Parser class
  public Parser( string filename, FrontEnd.Scanner lexer ): base(lexer) {
    //this.filename = filename;
    this.Scanner = this.lexer = lexer;
  }

  // Use this function for reporting non-fatal errors discovered
  // while parsing. An example usage is:
  //    yyerror( "Identifier {0} has not been declared", idname );
  public void yyerror( string format, params Object[] args ) {
    Console.Write("{0}: ", LineNumber);
    Console.WriteLine(format, args);
  }

  // returns the lexer's current line number
  public int LineNumber { get{return lexer.LineNumber;} }


