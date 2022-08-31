using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace NebulousConquestHelper
{
    class Program
	{
		public readonly EventId BotEventId = new EventId(42, "Nebulous-QB");

		public DiscordClient Client { get; set; }
		public CommandsNextExtension Commands { get; set; }


		static void Main(string[] args)
        {
			FilePath path = new FilePath(Helper.DATA_FOLDER_PATH + "TestGame.conquest");
			GameInfo game = (GameInfo)Helper.ReadXMLFile(typeof(GameInfo), path, GameInfo.init);

			// test code below, feel free to remove

			foreach (LocationInfo loc in game.System.OrbitingLocations)
            {
				Console.WriteLine(loc.Name + " - " + loc.PresentFleets.Count);
				Console.WriteLine(loc.OrbitalStartDegrees + " -> " + loc.GetCurrentDegrees(13 * 7));
            }

			Mapping.CreateSystemMap(game.System);

			// test code above, feel free to remove

			Program program = new Program();
			program.RunBotAsync().GetAwaiter().GetResult();
        }

		public async Task RunBotAsync()
        {
			var configFileName = "src/data/config.json";
			var json = "";
			if (!File.Exists(configFileName))
			{
				Console.WriteLine("config.json is missing. Copy config.example.json and put in the discord bot token");
				return;
			}
			using (var fs = File.OpenRead(configFileName))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync();

			var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
			if (string.IsNullOrEmpty(cfgjson.Token))
            {
				Console.WriteLine("Missing token in config");
				return;
            }
			var cfg = new DiscordConfiguration
			{
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				MinimumLogLevel = LogLevel.Debug,
			};

			this.Client = new DiscordClient(cfg);

			this.Client.Ready += this.Client_Ready;
			this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientErrored;

			/*
			var ccfg = new CommandsNextConfiguration
			{
				StringPrefixes = new[] { cfgjson.CommandPrefix },
				EnableDms = true,
				EnableMentionPrefix = true
			};

			this.Commands = this.Client.UseCommandsNext(ccfg);*/

			await this.Client.ConnectAsync();

			await Task.Delay(-1);
		}

        private Task Client_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
		{
			sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

			return Task.CompletedTask;
		}

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
		{
			sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

			return Task.CompletedTask;
		}

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
		{
			sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

			return Task.CompletedTask;
		}
    }

	public struct ConfigJson
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }
	}
}
