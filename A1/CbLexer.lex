%namespace FrontEnd
%tokentype Tokens

%{

	/* *********************************************
		 *                                           *
		 *         Errors and Error handling         *
		 *                                           *
		 ********************************************* */
	public bool hasError = false; //we'll begin with the (probably wrong) assumption that you know what you are doing.
	public int errorCount = 0;

	public override void yyerror( string msg, params object[] args ) {
		Console.WriteLine("{0}: ", yyline);
		errorCount++;
		hasError = true;
		if (args == null || args.Length == 0){
			Console.WriteLine("{0}", msg);
		}
		else{
			Console.WriteLine(msg, args);
		}
	}

	public void yyerror( int lineNum, string msg, params object[] args ) {
		Console.WriteLine("{0}: {1}", msg, args);
		errorCount++;
		hasError = true;
	}

	public int LineNumber { get{return yyline;} }


	/* ************************************
		 *                                  *
		 *         Comment handling         *
		 *                                  *
		 ************************************ */

	public int levelsOfComment = 0;

	public void foundSingleLineComment(){
		if(cbc.debug_flag){
			Console.WriteLine("Single line Comment {0}", yyline);
		}
	}

	public void startMultiLineComment(){
		levelsOfComment++;
		if(cbc.debug_flag){
			Console.WriteLine("Multiline comment start {0}", yyline);
		}
	}

	public void finishMultiLineComment(){
		levelsOfComment--;
		if(cbc.debug_flag){
			Console.WriteLine("Multiline comment end {0}, levels {1}", yyline, levelsOfComment);
		}
	}

	public void checkNestedComments(){
		if(cbc.debug_flag){
			Console.WriteLine("---Reached end of file.---");
		}
		if(levelsOfComment > 0){
			//more '/*' than '*/'
			yyerror("Improperly nested comment, too many opening '/*' blocks.");
		}else if(levelsOfComment < 0){
			//more '*/' than '/*'
			yyerror("Improperly nested comment, too many closing '*/' blocks.");
		}
	}

	/* *********************************************
		 *                                           *
		 *         Tokens and Token Printing         *
		 *                                           *
		 ********************************************* */

	public bool foundToken = false;

	public void printTokenToFile(String tokenName){
		if(cbc.tokens_flag){
			using (StreamWriter w = new StreamWriter("tokens.txt", true)){
				w.Write("Token.Kwd_{0} ",tokenName);
				foundToken = true;
			}
		}
	}

	public void printIdentToFile(String identName){
		if(cbc.tokens_flag){
			using (StreamWriter w = new StreamWriter("tokens.txt", true)){
				if(foundToken){
					w.WriteLine(", Ident \"{0}\" ",identName);
					foundToken = false;
				}else{
					w.WriteLine("Token \"{0}\" ",identName);
				}
			}
		}
	}

	// printing for strings, chars, and keywords
	public void tok_output_file(String type, String token){
		if (cbc.tokens_flag){
			using (StreamWriter w = new StreamWriter("tokens.txt", true)){
				bool str = type.Equals("string",StringComparison.OrdinalIgnoreCase);
				bool chars = type.Equals("SingleChar",StringComparison.OrdinalIgnoreCase);
				bool constant = type.Equals("StringConst",StringComparison.OrdinalIgnoreCase);
				bool ident = type.Equals("Ident",StringComparison.OrdinalIgnoreCase);
				bool number = type.Equals("Number",StringComparison.OrdinalIgnoreCase);

				if (str || chars || ident || constant){
					w.WriteLine("Token.{0}, text = {1}",type,token);
				}else if (number){
					w.WriteLine("Token.{0}, num = {1}", type,token);
				}else{
					w.WriteLine("Token.\"{0}\"",token);
				}
			}
		}
	}

%}

special [\?@\#\\`~]
space [ \n\r\t]
opchar [|&!>\<\.:,+\-*/%=\(\)\{\}\[\]\;\^\'\"] // must escape "-" as it signifies a range

%%

{space}     {}
([/][/]){1}[^\n]*        {foundSingleLineComment();} //Single line comments.
([/][*]){1}[^(\*/)]*		{startMultiLineComment();}//multiline comments
([\*][/]){1}				{finishMultiLineComment();}//multiline comments
<<EOF>>							{checkNestedComments();}

break       {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_break;}
char        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_char;}
class       {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_class;}
const       {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_const;}
else        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_else;}
if          {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_if;}
int         {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_int;}
new         {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_new;}
null        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_null;}
out         {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_out;}
override    {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_override;}
public      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_public;}
return      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_return;}
static      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_static;}
string      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_string;}
using       {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_using;}
virtual     {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_virtual;}
void        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_void;}
while       {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.Kwd_while;}

(\+\+)      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.PLUSPLUS;}
(\-\-)      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.MINUSMINUS;}
(==)        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.EQEQ;}
(\|\|)      {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.OROR;}
(&&)        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.ANDAND;}
(!=)        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.NOTEQ;}
(>=)        {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.GTEQ;}
(\<=)       {last_token_text=yytext;tok_output_file("any",yytext);return (int)Tokens.LTEQ;}

0|(\-)?[1-9][0-9]*|0x[0-9a-fA-F][0-9a-fA-F]*    {last_token_text=yytext;tok_output_file("Number",yytext);return (int)Tokens.Number;}
[a-zA-Z][a-zA-Z0-9_]*   {last_token_text=yytext;tok_output_file("Ident",yytext);return (int)Tokens.Ident;}
{opchar}|{special}        {tok_output_file("any",yytext); return (int)(yytext[0]);}
['](.?|(\\n)?|(\\r)?|(\\t)?|(\\\")?|(\\\')?)[']		{last_token_text=yytext;tok_output_file("SingleChar",yytext);return (int)Tokens.SingleChar;}
["].*["]			{last_token_text=yytext;tok_output_file("StringConst",yytext);return (int)Tokens.StringConst;}

.              { yyerror("illegal character ({0})", yytext); }
%%

public string last_token_text = "";
