using System;

class TestCase{
	static void Main(String [] args) 
	{
		Method1(3,5,2);
		Method1("NICE",'x');
		Method1("Nonsense", "Steam", 67, 'd');

		int x;
		x--;
		x = -4 + -4;
		
		Console.Write('\n');
		Console.Write('\r');
		Console.Write('\t');
		Console.Write('\'');		
		Console.Write('\"');
		Console.Write("\n\r\t\'\" ");		
		
		// First nested comment
		/* Comments are the best! */
		
		// Second nested comment
		/*  /* Gotta love nested comments! */ */
		
		// Third nested comment
		/*/*
		/*
		TAKE IT TO THE NEXT LEVEL SON
		*/
		*/*/			
	}
	
	static void casting()
	{
		int x;
		x = 2+3;
		//char y;
		//y = (int)x + 2;
		
		int y = 5;
		y = (int)5 + (int)x;
		//z = (int)(5+2) + (int)(x+5);	
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

	static void testIfWhileBreakCond()
	{
		int y = 0;
		string runningman = "";
		if (y < 3) 
		{
			while (true) 
			{
				
				if (y == 3)
				{
					break;
				}
				else
				{
					runningman = "LET\'s GO!";
					CallHome();
				}
				
			}
		}
		
		if (b >= 6)
			doThis("dffb");
			
		if (b == 5)
			doThis("ghjg");
			
		if (b == -4 || g != null)
			donotdo("Your Mother");
			
		if (b == 4 && c == 5 || 3 == f && d == 23)
			shoveapiein("Your Face");		
	}
	
	static void testDecls()
	{
		int a;
		int b, c, d;
		a = 3;
		string e;
		e = "ASSIGNED A STRING";
		int [] f;
		f = new int[20];
		int [] g, h, i;
		char j;
		RandomObject x,y,z;
		RandomObject x = new RandomObject(YourMom,Blows);
		int x = 3;	
	}
}