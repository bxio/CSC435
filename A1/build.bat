del Cbparser.lst
del CbParser.cs
del Cbparser.conflicts

gplex.exe Cblexer.lex
gppg /gplex CbParser.y > main.cs
csc /r:QUT.ShiftReduceParser.dll test.cs Cblexer.cs main.cs
