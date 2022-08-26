using Newtonsoft.Json;
using PCRE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Upgrade
{
	class Entry
	{
		[JsonProperty("enum")]
		public string Enum { get; set; }

		[JsonProperty("code")]
		public string Code { get; set; }

		[JsonIgnore()]
		public string[] EnumValues
		{
			get
			{
				// This is simple because we control the input so can be strict.
				string values = Enum.Split('{')[1].Split('}')[0];
				return values.Split(',').Select((v) => v.Trim()).ToArray();
			}
		}

		[JsonIgnore()]
		public string TagName
		{
			get
			{
				// This is simple because we control the input so can be strict.
				return Enum.Split(':')[0].Substring(5);
			}
		}

		[JsonIgnore()]
		public string FunctionName
		{
			get
			{
				// This regex is simple because we control the input so can be strict.
				var regex = new PcreRegex("(?:native|stock) (?:\\w+\\:)?(\\w+)");
				return regex.Match(Code).Groups[1];
			}
		}
	}

	// Generate the regex required to convert old code to new code.
	class Generator
	{
		[JsonProperty("generators")]
		public Entry[] Generators { get; set; }

		public Generator()
		{
		}
	}
}

