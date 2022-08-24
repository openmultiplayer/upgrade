using System;
using System.Linq;

namespace CallbackUpgrade
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 1 || args.Contains("--help"))
			{
				// Display the help message.
				Console.WriteLine("Upgrades forward/hook/public prototypes from SA:MP to open.mp");
				Console.WriteLine("");
				Console.WriteLine("Usage:");
				Console.WriteLine("");
				Console.WriteLine("    callback_upgrade [--report] [--help] directory");
				Console.WriteLine("");
				Console.WriteLine("  --report - Show changes to make, but don't make them.");
				Console.WriteLine("  --help - Show this message and exit.");
				Console.WriteLine("  directory - Where to run the scan.");
			}
			else
			{

			}
		}
	}
}

