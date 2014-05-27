@echo off
gplex.exe Cblexer.lex
echo -------Gplex Completed, Starting Gppg-------
gppg /gplex CbParser.y > main.cs
echo -------Gppg Completed-------
csc /r:QUT.ShiftReduceParser.dll cbc.cs Cblexer.cs main.cs
echo -------All Done!-------
cbc test.cs
