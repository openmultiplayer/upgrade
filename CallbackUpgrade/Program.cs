using System;
using System.Linq;
using PCRE;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

namespace CallbackUpgrade
{
	public class Replacement
	{
		public string description { get; set; }
		public string from { get; set; }
		public string to { get; set; }
	}

	public class Scans
	{
		public Dictionary<string, string> defines { get; set; }
		public Replacement[] replacements { get; set; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			using (StreamReader file = File.OpenText(@"../../../callback-upgrade.json"))
			{
				JsonSerializer serializer = new JsonSerializer();
				Scans scans = (Scans)serializer.Deserialize(file, typeof (Scans));
			}
			if (args.Length == 0 || args.Contains("--help"))
			{
				// Display the help message.
				Console.WriteLine("Upgrades a lot of code from SA:MP to open.mp");
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

