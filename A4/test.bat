@echo off
del "cbc.exe"
del "Fibs.ll"
del "Fibs.s"
del "Fibs"
echo ------- Compiling -------
csc /debug:full /r:QUT.ShiftReduceParser.dll CbLexer.cs CbParser.cs  CbTopLevel.cs CbAST.cs CbType.cs CbVisitor.cs CbPrVisitor.cs CbTLVisitor.cs CbSymTab.cs CbTypeCheckVisitor1.cs CbTypeCheckVisitor2.cs LLVM.cs LLVM-Arrays.cs LLVM-ConstantHandling.cs LLVM-CreateClassDefn.cs LLVM-Definitions.cs LLVM-SSAMethods.cs LLVM-UtilityMethods.cs LLVMVisitor1.cs LLVMVisitor2.cs LLVM-WriteMethods.cs cbc.cs
echo ------- Testing Conditionals -------
cbc.exe ./tests/test_conditional.cb
echo ------- Testing Basic Expressions -------
cbc.exe ./tests/test_incdec.cb
echo ------- Testing Unary Expressions -------
cbc.exe ./tests/test_unary.cb
echo ------- Testing While -------
cbc.exe ./tests/test_while.cb

