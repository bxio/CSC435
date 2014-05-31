/* CbParser.y */

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
%token      Kwd_static Kwd_string Kwd_System Kwd_using Kwd_virtual Kwd_void Kwd_while
%token      PLUSPLUS MINUSMINUS Ident Number StringConst SingleChar

%%

/* *************************************************************************
   *                                                                       *
   *         PRODUCTION RULES AND ASSOCIATED SEMANTIC ACTIONS              *
   *                                                                       *
   ************************************************************************* */

Program:        UsingList ClassList
        ;

UsingList:      /* empty */
		|		Kwd_using Kwd_System ';'
        |       Kwd_using Ident ';' UsingList
        ;

ClassList:	    ClassList ClassDecl
		|		ClassDecl
		;

ClassDecl:		Kwd_class Ident '{'  DeclList  '}'
		|		Kwd_class Ident ':' Ident '{'  DeclList  '}' // To support Inheritance
		;

DeclList:       DeclList ConstDecl
        |       DeclList MethodDecl
        |       DeclList FieldDecl
        |       /* empty */
        ;

ConstDecl:      Kwd_public Kwd_const Type Ident '=' InitVal ';'
        ;

InitVal:        Number
        |       StringConst
        ;

FieldDecl:      Kwd_public Type IdentList ';'
        ;

IdentList:      IdentList ',' Ident
        |       Ident
        ;

MethodDecl:     MethodSecurity MethodDeclModifier MethodDeclType Ident '(' OptFormals ')' Block
        ;

MethodSecurity:	Kwd_public
		|		/* empty */
		;

MethodDeclModifier: Kwd_static
				|				Kwd_virtual
				|				Kwd_override
				;
MethodDeclType: Kwd_void
				|				Type
				;
OptFormals:     /* empty */
        |       FormalPars
        ;

FormalPars:     Type Ident
        |       FormalPars ',' Type Ident
        ;


Type:           TypeName
        |       TypeName '[' ']'
        ;

TypeName:       Ident
        |       Kwd_int
        |       Kwd_string
		|		Kwd_char
        ;

Statement:      Designator '=' Expr ';'
        |       Designator '(' OptActuals ')' ';'
        |       Designator PLUSPLUS ';'
        |       Designator MINUSMINUS ';'
        |       Kwd_if '(' Expr ')' Statement OptElsePart
        |       Kwd_while '(' Expr ')' Statement
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

OptElsePart:    Kwd_else Statement
        |       /* empty */
        ;

Block:          '{' DeclsAndStmts '}'
        ;

LocalDecl:      Type IdentList ';'
		|		Type Ident '=' Expr ';'
        ;

DeclsAndStmts:   /* empty */
        |       DeclsAndStmts Statement
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
		|		ExprValue
        |       Kwd_new Ident '(' OptActuals ')'
        |       Kwd_new TypeName '[' Expr ']'
        |       '(' Expr ')'
		|		Kwd_null
        ;

ExprValue:		'(' Type ')' ExprValue1
		|		ExprValue1		
		;
		
ExprValue1:		Designator
        |       Designator '(' OptActuals ')'
		|       Number
		|		SingleChar
        |       StringConst
        |       StringConst '.' Ident // Ident must be "Length"
        |  		'-' Expr %prec UMINUS			
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

//returns number of errors found in lexing the file.
public int errorCount {get {return lexer.errorCount;}}
//returns a boolean signifying whether ther is an error
public bool hasError {get{return lexer.hasError;}}
