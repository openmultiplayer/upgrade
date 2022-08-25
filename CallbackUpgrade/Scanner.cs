using Newtonsoft.Json;
using System.Collections.Generic;

namespace CallbackUpgrade
{
	class Scanner
	{
		[JsonProperty("defines")]
		public Dictionary<string, string> Defines { get; set; }

		[JsonProperty("replacements")]
		public Replacement[] Replacements { get; set; }

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

