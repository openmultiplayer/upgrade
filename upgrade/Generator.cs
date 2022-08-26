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
		public Dictionary<string, int> EnumValues
		{
			get
			{
				// This is simple because we control the input so can be strict.
				string all = Enum.Split('{')[1].Split('}')[0];
				IEnumerable<string> entries = all.Split(',').Select((v) => v.Trim());

				int idx = 0;
				Dictionary<string, int> ret = new Dictionary<string, int>();
				foreach (var entry in entries)
				{
					var parts = entry.Split('=');
					if (parts.Length == 1)
					{
					}
					else if (int.TryParse(parts[1].Trim(), out int nv))
					{
						idx = nv;
					}
					else
					{
						idx = ret[parts[1].Trim()];
					}
					ret.Add(parts[0].Trim(), idx);
					idx = idx + 1;
				}
				return ret;
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

		// Get the function name we are putting the custom tag in to.
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

		// Does this declaration have a return tag?
		[JsonIgnore()]
		public string ReturnTag
		{
			get
			{
				// This regex is simple because we control the input so can be strict.
				var regex = new PcreRegex("(?:native|stock) (\\w+\\:)?");
				return regex.Match(Code).Groups[1];
			}
		}
		
		// Does this declaration have a return tag?
		[JsonIgnore()]
		public bool HasReturnTag
		{
			get
			{
				return ReturnTag != "";
			}
		}

		// Get the parameter index we are putting the custom tag in to.
		[JsonIgnore()]
		public IEnumerable<int> ReplaceIndexes
		{
			get
			{
				// This is simple because we control the input so can be strict.
				int cur = 0;
				for (; ;)
				{
					cur = Code.IndexOf('$', cur) + 1;
					if (cur == 0)
					{
						break;
					}
					yield return Code.Substring(0, cur).Count((c) => c == ',');
				}
			}
		}

		public string GetValueName(int value)
		{
			return EnumValues.FirstOrDefault((kv) => kv.Value == value).Key;
		}

		public int GetNameValue(string name)
		{
			return EnumValues.GetValueOrDefault(name, -1);
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

