using System.Collections.Generic;

namespace CallbackUpgrade
{
	class Scanner
	{
		public Dictionary<string, string> defines { get; set; }
		public Replacement[] replacements { get; set; }

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

