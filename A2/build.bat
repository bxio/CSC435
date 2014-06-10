@echo off

gplex CbLexer.lex
echo -------Gplex Completed, Starting Gppg-------
gppg /gplex CbParser.y
echo -------Gppg Completed, compiling csc-------
csc /debug:full /r:QUT.ShiftReduceParser.dll CbLexer.cs CbParser.cs CbAST.cs CbType.cs CbTopLevel.cs CbVisitor.cs CbPrVisitor.cs cbc.cs
pause
cbc -ast CbExample.cs
