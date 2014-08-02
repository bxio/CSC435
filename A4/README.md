CSC 435 Summer 2014
Assignment 4
------

Team Members:

* Bill Xiong V00737042
* Wesley Chow V00727646

The submission contains the following:

* build.bat
* LLVM-Arrays.cs
* LLVM-ConstantHandling.cs
* LLVM-CreateClassDefn.cs
* LLVM-Definitions.cs
* LLVM-SSAMethods.cs
* CbAST.cs
* LLVM-UtilityMethods.cs
* CbLLVM-Definitions.cs
* LLVM-WriteMethods.cs
* CbLLVM-WriteMethods.cs
* LLVM.cs
* CbLLVM.cs
* LLVMVisitor1.cs
* CbLLVMVisitor1.cs
* LLVMVisitor2.cs
* CbLLVMVisitor2.cs
* QUT.ShiftReduceParser.dll
* CbLexer.cs
* README.md
* CbLexer.lex
* build.bat
* CbParser.cs
* cbc.cs
* CbParser.y
* cbc.exe
* CbPrVisitor.cs
* cbc.pdb
* CbSymTab.cs
* clean.bat
* CbTLVisitor.cs
* compile.bat
* CbTopLevel.cs
* corrections.txt
* CbType.cs
* gen.sh
* CbTypeCheckVisitor1.cs
* CbTypeCheckVisitor2.cs
* CbVisitor.cs
* while.cs
* Fibs.cs

Creating the executable
========================

1. Assuming you have generated the lexer and parser already (CbLexer.cs and CbParser.cs), run the build file to create a cbc.exe, which will generate your LL file (your IR code). Ensure you have the ShiftReduceParser.dll before running the build.bat file.

2. Next, you want to SSH into a lab computer and then run the gen.sh script in the same directory remotely to generate the executable for the test program. The command is

% ./gen.sh

You should end up with a binary named Fibs and the script will execute the program. *

*If you have an issue running gen.sh remotely, the file may have windows line-endings and you will have to do a little bit of editing. Below are instructions for how to fix that...
	1. 	While still in SSH, open up gen.sh with the program, vim.
	2. 	By default, you will be in viewing mode. Stay in this mode and type
			% :set fileformat=unix
		to change the file format to unix-like files.
	3. Save the file in viewing mode by typing
			% :wq
		which will save the current file and then quit vim.


Again, Please spare our marks.
