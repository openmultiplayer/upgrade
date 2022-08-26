using System;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CallbackUpgrade
{
	class Program
	{
		private static int CountLines(string text)
		{
			return 1 + text.Count((c) => c == '\n');
		}

		private static string MakeDiff(Diff diff, ref int change)
		{
			string from = diff.From.Replace("\n", "\n-");
			string to = diff.To.Replace("\n", "\n+");
			int ilines = CountLines(from);
			int olines = CountLines(to);
			StringBuilder sb = new StringBuilder("@@ -");
			sb.Append(diff.Line);
			sb.Append(',');
			sb.Append(ilines);
			sb.Append(" +");
			sb.Append(diff.Line + change);
			sb.Append(',');
			sb.Append(olines);
			sb.Append(" @@ ");
			sb.Append(diff.Description);
			sb.Append("\n-");
			sb.Append(from);
			sb.Append("\n+");
			sb.Append(to);
			change = change + olines - ilines;
			return sb.ToString();
		}

		private static string ArgOrDefault(string[] args, string name, string def)
		{
			int idx = Array.FindIndex(args, (n) => n == name) + 1;
			if (idx == 0 || idx == args.Length)
			{
				return def;
			}
			return args[idx];
		}

		private static void ScanDir(string root, string[] types, Scanner scanner, bool report, bool recurse, List<Task> tasks)
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
						IOrderedEnumerable<Diff> diffs = scanner.Report(file).OrderBy((d) => d.Line);
						switch (diffs.Count())
						{
						case 0:
							Console.WriteLine("  No replacements found.");
							break;
						case 1:
							Console.WriteLine("  1 replacement found:\n");
							break;
						default:
							Console.WriteLine("  " + diffs.Count() + " replacements found:\n");
							break;
						}
						// How many lines the output has grown or shrunk by.
						int change = 0;
						foreach (var diff in diffs)
						{
							Console.WriteLine(MakeDiff(diff, ref change));
						}
					}
					else
					{
						tasks.Add(scanner.Replace(file));
						//int diffs = scanner.Replace(file);
						//switch (diffs)
						//{
						//case 0:
						//	Console.WriteLine("  No replacements made.");
						//	break;
						//case 1:
						//	Console.WriteLine("  1 replacement made.");
						//	break;
						//default:
						//	Console.WriteLine("  " + diffs + " replacements made.");
						//	break;
						//}
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
						ScanDir(dir, types, scanner, report, recurse, tasks);
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
				Console.WriteLine("    upgrade [--report] [--scans file] [--types types] [--help] directory");
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
			List<Task> tasks = new List<Task>();
			ScanDir(directory, types, scanners, report, true, tasks);
			Task.WaitAll(tasks.ToArray());
		}
	}
}

