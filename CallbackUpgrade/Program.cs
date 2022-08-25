using System;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace CallbackUpgrade
{
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

		private static void ScanDir(string root, string[] types, Scanner scanner, bool report, bool recurse)
		{

			foreach (var type in types)
			{
				string pattern = "*." + type;
				foreach (var file in Directory.EnumerateFiles(root, pattern))
				{
					// Loop over all the files and do the replacements.
					Console.WriteLine("Scanning file: " + file);
					Console.WriteLine("");
					if (report)
					{
						Diff[] diffs = scanner.Report(file);
						switch (diffs.Length)
						{
						case 0:
							Console.WriteLine("  No replacements found.");
							break;
						case 1:
							Console.WriteLine("  1 replacement found:");
							break;
						default:
							Console.WriteLine("  " + diffs.Length + " replacements found:");
							break;
						}
						foreach (var diff in diffs)
						{
							Console.WriteLine("");
							Console.WriteLine("    Line " + diff.Line + " - " + diff.Description + ":");
							Console.WriteLine("");
							Console.WriteLine("      " + diff.From.Replace("\n", "\n      ") + "");
							Console.WriteLine("");
							Console.WriteLine("    >>>");
							Console.WriteLine("");
							Console.WriteLine("      " + diff.To.Replace("\n", "\n      ") + "");
						}
					}
					else
					{
						int diffs = scanner.Replace(file);
						switch (diffs)
						{
						case 0:
							Console.WriteLine("  No replacements made.");
							break;
						case 1:
							Console.WriteLine("  1 replacement made.");
							break;
						default:
							Console.WriteLine("  " + diffs + " replacements made.");
							break;
						}
					}
					Console.WriteLine("");
				}
			}
			if (recurse)
			{
				foreach (var dir in Directory.EnumerateDirectories(root, "*"))
				{
					// Recurse in to REAL child directories.
					DirectoryInfo info = new DirectoryInfo(dir);
					if (!info.Attributes.HasFlag(FileAttributes.ReparsePoint))
					{
						ScanDir(dir, types, scanner, report, recurse);
					}
				}
			}
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
			string directory = Path.GetFullPath(args.Last());
			if (!Directory.Exists(directory))
			{
				Console.WriteLine("\"" + directory + "\" is not a directory.");
				return;
			}
			Scanner scanners;
			using (StreamReader fhnd = File.OpenText(file))
			{
				JsonSerializer serializer = new JsonSerializer();
				scanners = (Scanner)serializer.Deserialize(fhnd, typeof (Scanner));
			}
			// Descend.
			ScanDir(directory, types, scanners, report, true);
		}
	}
}

