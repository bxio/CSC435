@echo off
del cbc.exe
gplex.exe Cblexer.lex
echo -------Gplex Completed, Starting Gppg-------
gppg /gplex CbParser.y > main.cs
echo -------Gppg Completed, compiling csc-------
csc /r:QUT.ShiftReduceParser.dll cbc.cs Cblexer.cs main.cs
echo -------All Done, running -------
cbc -debug -tokens test.cs
