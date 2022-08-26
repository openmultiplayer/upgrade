using Newtonsoft.Json;
using PCRE;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CallbackUpgrade
{
	class Scanner
	{
		private Dictionary<string, string> defines_ = null;

		private void UpdateCaptureGroups()
		{
			// The defines are (incorrectly) counted in the match numbers.  Adjust the replacements.
			int adjust = defines_.Count;
			var regex = new PcreRegex("\\$(\\d+)", PcreOptions.Compiled);
			foreach (var i in replacements_)
			{
				i.To = regex.Replace(i.To, (m) =>
				{
					int idx = int.Parse(m[1]) + adjust;
					return "$" + idx;
				});
			}
		}

		[JsonProperty("defines")]
		public Dictionary<string, string> Defines
		{
			get
			{
				return defines_;
			}
			
			set
			{
				defines_ = value;
				// Construct the regex version of these defines.
				StringBuilder sb = new StringBuilder("(?(DEFINE)");
				foreach (var kv in defines_)
				{
					sb.Append("\n  (?<");
					sb.Append(kv.Key);
					sb.Append(">\n    ");
					sb.Append(kv.Value);
					sb.Append("\n  )");
				}
				sb.Append("\n)\n");
				RegexDefine = sb.ToString();
				if (!(replacements_ is null))
				{
					UpdateCaptureGroups();
				}
			}
		}

		private Replacement[] replacements_;

		[JsonProperty("replacements")]
		public Replacement[] Replacements
		{
			get
			{
				return replacements_;
			}

			set
			{
				replacements_ = value;
				if (!(defines_ is null))
				{
					UpdateCaptureGroups();
				}
			}
		}

		[JsonIgnore]
		public string RegexDefine { get; private set; }
		
		private int GetLineNumber(string text, int pos)
		{
			// Just count the number of newlines between here and the start.
			int count = 1;
			while (pos != 0)
			{
				if (text[--pos] == '\n')
				{
					++count;
				}
			}
			return count;
		}

		public IEnumerable<Diff> Report(string name)
		{
			// This does things the slow way, with a replacement function and a second regex call
			// inside it.  This is so we can report accurately.
			List<Diff> ret = new List<Diff>();
			using StreamReader fhnd = File.OpenText(name);
			string contents = fhnd.ReadToEnd();
			foreach (var rep in Replacements)
			{
				// Now we go for the most efficient scanning method we can (based on very little
				// reading of the documentation).  Each replacement is done separately.
				var regex = new PcreRegex(RegexDefine + rep.From, PcreOptions.Compiled | PcreOptions.Extended | PcreOptions.MultiLine);
				contents = regex.Replace(contents, (match) =>
				{
					int line = GetLineNumber(contents, match.Index);
					string from = match.Value;
					string to = regex.Replace(from, rep.To);
					ret.Add(new Diff {
						Description = rep.Description,
						Line = line,
						From = from,
						To = to
					});
					return to;
				});
			}
			// Returns a list of the replacements to be made.
			return ret;
		}

		public async Task<int> Replace(string name)
		{
			// Actually does the replacements.
			List<Diff> ret = new List<Diff>();
			string contents = "";
			using (StreamReader fhnd = File.OpenText(name))
			{
				contents = await fhnd.ReadToEndAsync();
				foreach (var rep in Replacements)
				{
					// Now we go for the most efficient scanning method we can (based on very little
					// reading of the documentation).  Each replacement is done separately.
					var regex = new PcreRegex(RegexDefine + rep.From, PcreOptions.Compiled | PcreOptions.Extended | PcreOptions.MultiLine);
					contents = regex.Replace(contents, rep.To);
				}
			}
			await File.WriteAllTextAsync(name, contents);
			// It turns out that counting the replacements is hard when we want to be fast.
			return 0;
		}
	}
}

