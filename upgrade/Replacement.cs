using Newtonsoft.Json;

namespace Upgrade
{
	class Replacement
	{
		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("from")]
		public string From { get; set; }

		[JsonProperty("to")]
		public string To { get; set; }
	}
}

