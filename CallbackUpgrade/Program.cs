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
		private static string ArgOrDefault(string[] args, string name, string def)
		{
			int idx = Array.FindIndex(args, (n) => n == name) + 1;
			if (idx == 0 || idx == args.Length)
			{
				return def;
			}
			return args[idx];
		}

		static void Main(string[] args)
		{
			if (args.Length == 0 || args.Contains("--help"))
			{
				// Display the help message.
				Console.WriteLine("Upgrades a lot of code from SA:MP to open.mp");
				Console.WriteLine("");
				Console.WriteLine("Usage:");
				Console.WriteLine("");
				Console.WriteLine("    callback_upgrade [--report] [--scans file] [--types types] [--help] directory");
				Console.WriteLine("");
				Console.WriteLine("  --report - Show changes to make, but don't make them.");
				Console.WriteLine("  --scans file - Load defines and replacements from `file` (default `callback-upgrade.json`).");
				Console.WriteLine("  --types types - File types to replace in.  Default `pwn,p,pawn,inc,own`.");
				Console.WriteLine("  --help - Show this message and exit.");
				Console.WriteLine("  directory - Root directory in which to run the scan.");
				return;
			}
			string file = ArgOrDefault(args, "--scans", "../../../callback-upgrade.json");
			string[] types = ArgOrDefault(args, "--types", "pwn,p,pawn,inc,own").Split(',');
			bool report = args.Contains("--report");
			string directory = args.Last();
			if (!Directory.Exists(directory))
			{
				Console.WriteLine("\"" + directory + "\" is not a directory.");
				return;
			}
			Scans scans;
			using (StreamReader fhnd = File.OpenText(file))
			{
				JsonSerializer serializer = new JsonSerializer();
				scans = (Scans)serializer.Deserialize(fhnd, typeof (Scans));
			}
		}
	}
}

