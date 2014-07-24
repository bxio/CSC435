// Sample program for testing use of arrays
//
// It uses recursion instead of while loops
// It uses k = k+1  instead of  k++
//
// Program to generate first 20 Fibonacci numbers.

using System;

class Fibs {
    public int[] nums;
    public const int max = 20;

    public virtual void NextFib( int k ) {
        nums[k] = nums[k-1] + nums[k-2];
        k = k + 1;
        //Console.Write("* k = ");
        //Console.WriteLine(k);
        if (k < nums.Length) NextFib(k);
    }

    public virtual void Print( int k ) {
        Console.Write(nums[k]);
        k = k + 1;
        if (k < nums.Length) {
            Console.Write(", ");
            Print(k);
        } else {
            Console.WriteLine(".");
        }
    }

    public virtual void Run() {
        nums = new int[max];
        //Console.Write("* nums.Length= ");
        //Console.WriteLine(nums.Length);
        nums[0] = 1;
        nums[1] = 1;
        NextFib(2);
        Print(0);
    }
	
	public static void UnaryMinus(){
		Console.WriteLine("In UnaryMinus()");
		int k;
		k = 3;
		Console.Write("k before: ");
		Console.WriteLine(k);
		k = -k;
		Console.Write("k after: ");
		Console.WriteLine(k);
	}

	public static void PlusPlusMinusMinus(){
		Console.WriteLine("In PlusPlusMinusMinus()");
		int s;
		s = 100;
		
		Console.Write("s before increment: ");
		Console.WriteLine(s);	
		s++;
		Console.Write("s after increment: ");
		Console.WriteLine(s);	

		s--;
		s--;
		Console.Write("s after two decrements: ");
		Console.WriteLine(s);
	}
	
	public static void AndOr(){
		if ()
		{
		
		}
	}
	
    public static void Main() {
        UnaryMinus();
		PlusPlusMinusMinus();
		Fibs f;
        f = new Fibs();
        f.Run();
		
    }

	
}

