using Newtonsoft.Json;
using System.Collections.Generic;
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
			// Returns a list of the replacements to be made.
			return new Diff[] { };
		}

		public int Replace(string name)
		{
			// Actually does the replacements.
			return 0;
		}
	}
}

