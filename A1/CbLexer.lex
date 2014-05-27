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
parentheses [\(\)\{\}]
%%
{space}     {}

break       {last_token_text=yytext;return (int)Tokens.Kwd_break;}
char        {last_token_text=yytext;return (int)Tokens.Kwd_char;}
class       {last_token_text=yytext;return (int)Tokens.Kwd_class;}
const       {last_token_text=yytext;return (int)Tokens.Kwd_const;}
else        {last_token_text=yytext;return (int)Tokens.Kwd_else;}
if          {last_token_text=yytext;return (int)Tokens.Kwd_if;}
int         {last_token_text=yytext;return (int)Tokens.Kwd_int;}
new         {last_token_text=yytext;return (int)Tokens.Kwd_new;}
null        {last_token_text=yytext;return (int)Tokens.Kwd_null;}
out         {last_token_text=yytext;return (int)Tokens.Kwd_out;}
override    {last_token_text=yytext;return (int)Tokens.Kwd_override;}
public      {last_token_text=yytext;return (int)Tokens.Kwd_public;}
return      {last_token_text=yytext;return (int)Tokens.Kwd_return;}
static      {last_token_text=yytext;return (int)Tokens.Kwd_static;}
string      {last_token_text=yytext;return (int)Tokens.Kwd_string;}
using       {last_token_text=yytext;return (int)Tokens.Kwd_using;}
virtual     {last_token_text=yytext;return (int)Tokens.Kwd_virtual;}
void        {last_token_text=yytext;return (int)Tokens.Kwd_void;}
while       {last_token_text=yytext;return (int)Tokens.Kwd_while;}

++          {last_token_text=yytext;return (int)Tokens.PLUSPLUS;}
--          {last_token_text=yytext;return (int)Tokens.MINUSMINUS;}
0|[1-9][0-9]*|0x[0-9a-fA-F][0-9a-fA-F]*    {last_token_text=yytext;return (int)Tokens.Number;}
[a-zA-Z][a-zA-Z0-9_]*   {last_token_text=yytext;return (int)Tokens.Ident;}
{opchar}        {return (int)(yytext[0]);}
[()]			{return (int)(yytext[0]);}
/\*.*\*/        {}

.               { yyerror("illegal character ({0})", yytext); }
.+              {last_token_text=yytext;return (int)Tokens.StringConst;}
%%

public string last_token_text = "";

