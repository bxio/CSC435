using System;

class List {
    public List next;
    public virtual void Print() {}
}

class Other: List {
    public char c;
    public override void Print() {
        Console.Write(' ');
        Console.Write(c);
        if (next != null) next.Print();
    }
}

class Digit: List {
    public int d;
    public override void Print() {
        Console.Write(' ');
        Console.Write(d);
        if (next != null) next.Print();
    }
}

class Lists {
    public static void Main() {
        List ccc;
        string s;
        Console.WriteLine("enter some text => ");
        s = Console.ReadLine();
        ccc = null;
        int i;
        i = 0;
        while(i < s.Length) {
            char ch;
            ch = s[i];  i++;
            List elem;
            if (ch >= '0' && ch <= '9') {
		        Digit elemD;
                elemD = new Digit();  elemD.d = ch - '0';
                elem = elemD;
            } else {
		        Other elemO;
                elemO = new Other();  elemO.c = ch;
                elem = elemO;
            }
            elem.next = ccc;
            ccc = elem;
        }
        Console.WriteLine("\nReversed text =");
        ccc.Print();
        Console.WriteLine("\n");
    }
}

class TestCase{
	
	public static void Main()
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
		
	}
	
	public static void casting(int x)
	{
		int x;
		x = 2+3;
		char y;
		char ysdf;
		char ygfhj;
		char yjhkhj;
		char ghjkky;
		char gfhghdjhy;
		y = (int)x + 2;
		z = 5;
		y = (int)5 + (int)x;
		z = (int)(5+2) + (int)(x+5);
	}
	
	public static void Method1(int parameter1, int parameter2, int parameter3)
	{
		x = 1;
		Console.Write("This is a tester method for testing methods...");
	}

	public virtual void Method1(string parameter, char parameter2)
	{
		y = parameter;
		y++;
	}

	public override int Method1(string nonsensical, string lol, int d, char f)
	{
		yourmom = 4;
		x = "loldfgfkgjfklg";
	}

	public static void testIfWhileBreakCond()
	{
		
		y = 0;
		string runningman;
		runningman	= "";
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

	
	public static void testDecls()
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
		x = new RandomObject();
	}
}
