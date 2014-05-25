%namespace FrontEnd
%tokentype Tokens

%{
  public int lineNum = 1;

  public int LineNumber { get{ return lineNum; } }

  public override void yyerror( string msg, params object[] args ) {
    Console.WriteLine("{0}: ", lineNum);
    if (args == null || args.Length == 0) {
      Console.WriteLine("{0}", msg);
    }
    else {
      Console.WriteLine(msg, args);
    }
  }

  public void yyerror( int lineNum, string msg, params object[] args ) {
    Console.WriteLine("{0}: {1}", msg, args);
  }

%}

quotes [\'\"]
space [ \n\r\t]
opchar [+\-*/%=] // must escape "-" as it signifies a range

%%
{space}          {}

Kwd_break       {last_token_text=yytext;return (int)Tokens.Kwd_break;}
Kwd_class       {last_token_text=yytext;return (int)Tokens.Kwd_class;}
Kwd_const       {last_token_text=yytext;return (int)Tokens.Kwd_const;}
Kwd_else        {last_token_text=yytext;return (int)Tokens.Kwd_else;}
Kwd_if          {last_token_text=yytext;return (int)Tokens.Kwd_if;}
Kwd_int         {last_token_text=yytext;return (int)Tokens.Kwd_int;}
Kwd_new         {last_token_text=yytext;return (int)Tokens.Kwd_new;}
Kwd_null        {last_token_text=yytext;return (int)Tokens.Kwd_null;}
Kwd_out         {last_token_text=yytext;return (int)Tokens.Kwd_out;}
Kwd_override    {last_token_text=yytext;return (int)Tokens.Kwd_override;}
Kwd_public      {last_token_text=yytext;return (int)Tokens.Kwd_public;}
Kwd_return      {last_token_text=yytext;return (int)Tokens.Kwd_return;}
Kwd_static      {last_token_text=yytext;return (int)Tokens.Kwd_static;}
Kwd_string      {last_token_text=yytext;return (int)Tokens.Kwd_string;}
Kwd_using       {last_token_text=yytext;return (int)Tokens.Kwd_using;}
Kwd_virtual     {last_token_text=yytext;return (int)Tokens.Kwd_virtual;}
Kwd_void        {last_token_text=yytext;return (int)Tokens.Kwd_void;}
Kwd_while       {last_token_text=yytext;return (int)Tokens.Kwd_while;}

++              {last_token_text=yytext;return (int)Tokens.PLUSPLUS;}
--              {last_token_text=yytext;return (int)Tokens.MINUSMINUS;}
0|[1-9][0-9]*|0x[0-9a-fA-F][0-9a-fA-F]*    {last_token_text=yytext;return (int)Tokens.Number;}
[a-zA-Z][a-zA-Z0-9_]*   {last_token_text=yytext;return (int)Tokens.Ident;}
{opchar}        {return (int)(yytext[0]);}
[()]			{return (int)(yytext[0]);}
/\*.*\*/        {}

.               { yyerror("illegal character ({0})", yytext); }

%%

public string last_token_text = "";

