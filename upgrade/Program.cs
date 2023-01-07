﻿/*
 *  This Source Code Form is subject to the terms of the Mozilla Public License,
 *  v. 2.0. If a copy of the MPL was not distributed with this file, You can
 *  obtain one at http://mozilla.org/MPL/2.0/.
 *
 *  The original code is copyright (c) 2022, open.mp team and contributors.
 */

using System;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Upgrade
{
	class Program
	{
		private static int CountLines(string text)
		{
			return 1 + text.Count((c) => c == '\n');
		}

		private static string MakeDiff(Diff diff, ref int change)
		{
			string from = diff.From.Replace("\n", "\n    -").Replace("\t", "    ");
			string to = diff.To.Replace("\n", "\n    +").Replace("\t", "    ");
			int ilines = CountLines(from);
			int olines = CountLines(to);
			StringBuilder sb = new StringBuilder("    @@ -");
			sb.Append(diff.Line);
			sb.Append(',');
			sb.Append(ilines);
			sb.Append(" +");
			sb.Append(diff.Line + change);
			sb.Append(',');
			sb.Append(olines);
			sb.Append(" @@ ");
			sb.Append(diff.Description);
			sb.Append("\n    -");
			sb.Append(from);
			sb.Append("\n    +");
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

		private static void DoOneFile(string file, Scanner scanner, bool report, int debug, List<Task> tasks, Encoding encoding)
		{
			Console.WriteLine("Scanning file: " + file);
			Console.WriteLine("");
			if (report)
			{
				IOrderedEnumerable<Diff> diffs = scanner.Report(file, debug != 0, encoding).OrderBy((d) => d.Line);
				if (diffs.Count() == 0)
				{
					Console.WriteLine("    No replacements found.");
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
				tasks.Add(scanner.Replace(file, encoding));
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

		private static void ScanDir(string root, string[] types, string[] excludes, Scanner scanner, bool report, bool recurse, int debug, List<Task> tasks, Encoding encoding)
		{
			foreach (var type in types)
			{
				string pattern = "*." + type;
				foreach (var file in Directory.EnumerateFiles(root, pattern))
				{
					if (!excludes.Contains(Path.GetFileNameWithoutExtension(file)))
					{
						// Loop over all the files and do the replacements.
						DoOneFile(file, scanner, report, debug, tasks, encoding);
					}
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
						ScanDir(dir, types, excludes, scanner, report, recurse, debug, tasks, encoding);
					}
				}
			}
		}

		static void RunGenerate(string[] args)
		{
			// `1` by default for `--generate`.
			int argc = 1;
			int debug = 0;
			if (args.Contains("--debug"))
			{
				argc += 2;
				debug = int.Parse(ArgOrDefault(args, "--debug", "0"));
			}
			string output = "";
			if (args.Length > argc)
			{
				// There's a trailing argument that's not part of a `--` argument.
				output = args.Last();
			}
			Generator generator;
			using (StreamReader fhnd = File.OpenText("_generate.json"))
			{
				JsonSerializer serializer = new JsonSerializer();
				generator = (Generator)serializer.Deserialize(fhnd, typeof(Generator));
				generator.Dump(output);
			}
		}

		static void RunScan(string[] args)
		{
			string file = ArgOrDefault(args, "--scans", "upgrade.json");
			Encoding encoding = Encoding.ASCII;
			string codepage = ArgOrDefault(args, "--codepage", "");
			if (codepage != "")
			{
				try
				{
					encoding = Encoding.GetEncoding(codepage);
				}
				catch (ArgumentException e)
				{
					Console.WriteLine("");
					Console.WriteLine("Unknown codepage \"" + codepage + "\".");
					Console.WriteLine("");
					Console.WriteLine("Valid codepages:");
					Console.WriteLine("");
					foreach (EncodingInfo info in Encoding.GetEncodings())
					{
						Console.WriteLine("    " + info.Name + "\t- " + info.DisplayName);
					}
					Console.WriteLine("");
					return;
				}
			}
			string[] types = ArgOrDefault(args, "--types", "pwn,p,pawn,inc,own").Split(',');
			string[] excludes = ArgOrDefault(args, "--exclude", "y_compilerdata_codepage").Split(',');
			bool report = args.Contains("--report");
			int debug = int.Parse(ArgOrDefault(args, "--debug", "0"));
			string directory = Path.GetFullPath(args.Last());
			if (!File.Exists(directory) && !Directory.Exists(directory))
			{
				Console.WriteLine("Input file/dir \"" + directory + "\" does not exist.");
				return;
			}
			Scanner defines;
			Scanner scanner;
			// Get generic shared defines.
			using (StreamReader fhnd = File.OpenText("_define.json"))
			{
				JsonSerializer serializer = new JsonSerializer();
				defines = (Scanner)serializer.Deserialize(fhnd, typeof(Scanner));
			}
			// Get defines specific to this file.
			if (!file.EndsWith(".json"))
			{
				file = file + ".json";
			}
			if (!File.Exists(file))
			{
				Console.WriteLine("Definitions file \"" + file + "\" does not exist.");
				return;
			}
			using (StreamReader fhnd = File.OpenText(file))
			{
				JsonSerializer serializer = new JsonSerializer();
				scanner = (Scanner)serializer.Deserialize(fhnd, typeof(Scanner));
			}
			// Merge them, preferring specific ones over generic ones.
			foreach (var kv in defines.Defines)
			{
				scanner.Defines.TryAdd(kv.Key, kv.Value);
			}
			scanner.UpdateDefines();
			List<Task> tasks = new List<Task>();
			if (File.Exists(directory))
			{
				// Do one.
				DoOneFile(directory, scanner, report, debug, tasks, encoding);
			}
			else
			{
				// Descend.
				ScanDir(directory, types, excludes, scanner, report, true, debug, tasks, encoding);
			}
			Task.WaitAll(tasks.ToArray());
		}

		static void Main(string[] args)
		{
			if (args.Length == 0 || args.Contains("--help"))
			{
				// Display the help message.
				Console.WriteLine("Upgrades a lot of code from SA:MP to open.mp\n");
				Console.WriteLine("Usage:\n");
				Console.WriteLine("upgrade --generate [options] [output]\n");
				Console.WriteLine("    Options:\n");
				Console.WriteLine("    --generate    - Generate the regex to match the functions in `_generate.json`.");
				Console.WriteLine("    --debug level - Enable debugging output.");
				Console.WriteLine("    output        - Filename to write to (default console).\n");
				Console.WriteLine("upgrade [options] input\n");
				Console.WriteLine("    Options:\n");
				Console.WriteLine("    --report             - Show changes to make, but don't make them.");
				Console.WriteLine("    --scans file         - Load defines and replacements from `file` (default `upgrade`).");
				Console.WriteLine("    --types file,types   - File types to replace in.  Default `pwn,p,pawn,inc,own`.");
				Console.WriteLine("    --debug level        - Enable debugging output.");
				Console.WriteLine("    --codepage name      - What codepage to run the scans in.");
				Console.WriteLine("    --exclude file,names - Files to ignore (default `y_compilerdata_codepage`).");
				Console.WriteLine("    --help               - Show this message and exit.");
				Console.WriteLine("    input                - File to scan, or directory to recurse through.\n");
			}
			else if (args.Contains("--generate"))
			{
				RunGenerate(args);
			}
			else
			{
				RunScan(args);
			}	
		}
	}
}

