/home/nigelh/LLVM/bin/llc -o Fibs.s Fibs.ll
gcc -o Fibs Fibs.s
./Fibs
