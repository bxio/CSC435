del "cbc.exe"
del "Fibs.ll"
del "Fibs.s"
del "Fibs"

csc /debug:full /r:QUT.ShiftReduceParser.dll CbLexer.cs CbParser.cs  CbTopLevel.cs CbAST.cs CbType.cs CbVisitor.cs CbPrVisitor.cs CbTLVisitor.cs CbSymTab.cs CbTypeCheckVisitor1.cs CbTypeCheckVisitor2.cs LLVM.cs LLVM-Arrays.cs LLVM-ConstantHandling.cs LLVM-CreateClassDefn.cs LLVM-Definitions.cs LLVM-SSAMethods.cs LLVM-UtilityMethods.cs LLVMVisitor1.cs LLVMVisitor2.cs LLVM-WriteMethods.cs cbc.cs

cbc.exe Fibs.cs