using System;

class TestMethods
{
	static void Main(String [] args) 
	{
		Method1(3,5,2);
		Method1("NICE",'x');
		Method1("Nonsense", "Steam", 67, 'd');
	}
	
	static void Method1(int parameter1, int parameter2, int parameter3)
	{
		int x = 1;
		Console.Write("This is a tester method for testing methods...");
	}
	
	virtual void Method1(string parameter, char parameter2)
	{
		int y = parameter;
		y++;
	}
	
	override int Method1(string nonsensical, string lol, int d, char f)
	{
		int yourmom = 4;
		string x = "loldfgfkgjfklg";
	}
}

