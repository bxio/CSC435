del CbLexer.cs CbParser.cs cbc.exe
@echo off
gplex CbLexer.lex
echo -------Gplex Completed, Starting Gppg-------
gppg /gplex CbParser.y
echo -------Gppg Completed, compiling csc-------
csc /debug:full /r:QUT.ShiftReduceParser.dll CbLexer.cs CbParser.cs  CbTopLevel.cs CbAST.cs CbType.cs CbVisitor.cs CbPrVisitor.cs CbTLVisitor.cs TCVisitor1.cs TCVisitor2.cs CbSymTab.cs cbc.cs
cbc.exe -ns FullTest.cs
