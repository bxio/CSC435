using System;
using System.IO;
using System.Text;
using FrontEnd;

class cbc
{
    static bool debug_flag = false;
    static bool tokens_flag = false;

	// Reads in parameters, adjusts flags
	static int ReadFlags(string [] args){
        if (args.Length == 0)
        {
            Console.Write("input: cbc.exe [-debug] [-tokens] testprogram.cs");
            return -1;
        }

		for (int i=0; i<args.Length-1; i++)
        {
            if (args[i].Equals("-debug", StringComparison.OrdinalIgnoreCase))
            {
                debug_flag = true;
            }
            else if (args[i].Equals("-tokens", StringComparison.OrdinalIgnoreCase))
            {
                tokens_flag = true;
            }
            else
            {
                Console.Write("Invalid flag: ");
                Console.Write(args[i]);
                return -1;
            }
        }
		return 0;
	}

	static void Main(string [] args)
	{
		if (ReadFlags(args) != 0)
			return;

		FileStream tokens;
		if (tokens_flag)
			tokens = File.Create("tokens.txt");

		string InputFile = args[args.Length-1];

		FileStream fs = new FileStream(InputFile, FileMode.Open, FileAccess.Read);

		FrontEnd.Scanner scan = new FrontEnd.Scanner(fs);

        Parser parser = new Parser(InputFile,scan);

        parser.Parse();
	}

}
