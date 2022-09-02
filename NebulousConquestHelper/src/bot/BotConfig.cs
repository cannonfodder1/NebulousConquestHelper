using Newtonsoft.Json;

namespace NebulousConquestHelper
{
	public struct BotConfig
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }
	}
}
