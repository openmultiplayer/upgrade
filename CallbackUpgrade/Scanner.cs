using Newtonsoft.Json;
using PCRE;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CallbackUpgrade
{
	class Scanner
	{
		private Dictionary<string, string> defines_ = null;

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
				foreach (var kv in value)
				{
					sb.Append("\n  (?<");
					sb.Append(kv.Key);
					sb.Append(">\n    ");
					sb.Append(kv.Value);
					sb.Append("\n  )");
				}
				sb.Append("\n)\n");
				RegexDefine = sb.ToString();
			}
		}

		[JsonProperty("replacements")]
		public Replacement[] Replacements { get; set; }

		[JsonIgnore]
		public string RegexDefine { get; private set; }

		public Diff[] Report(string name)
		{
			List<Diff> ret = new List<Diff>();
			using (StreamReader fhnd = File.OpenText(name))
			{
				string contents = fhnd.ReadToEnd();
				foreach (var rep in Replacements)
				{
					// Now we go for the most efficient scanning method we can (based on very little
					// reading of the documentation).  Each replacement is done separately.
					var regex = new PcreRegex(RegexDefine + rep.From, PcreOptions.Compiled | PcreOptions.Extended | PcreOptions.MultiLine);
					using var buffer = regex.CreateMatchBuffer();
					foreach (var match in buffer.Matches(contents))
					{
						var diff = new Diff {
							Description = rep.Description,
							From = match.Value.ToString(),
							To = rep.To,
							Line = match.Index
						};
						ret.Add(diff);
					}
				}
			}
			// Returns a list of the replacements to be made.
			return ret.ToArray();
		}

		public int Replace(string name)
		{
			// Actually does the replacements.
			return 0;
		}
	}
}

