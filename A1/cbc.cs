using System;

class cbc
{

    boolean debug_flag = false;
    boolean tokens_flag = false;

	static void Main(string [] args)
	{
        
        if (args.Length == 0) 
        {
            Console.Write("input: cbc.exe [-debug] [-tokens] test.cs");
            return;
        }
        
        for (int i=0; i<args.Length - 1; i++) 
        {
            if (args[i].Equals("-debug", StringComparison.OrdinalIgnoreCase))
            {
                tokens_flag = true;
            }    
            else if (args[i].Equals("-tokens", StringComparison.OrdinalIgnoreCase))
            {
                tokens_flag = true;
            }
            else
            {
                Console.Write("Invalid token: ");
                Console.Write(args[i]);
            }
        }
        
	}

}