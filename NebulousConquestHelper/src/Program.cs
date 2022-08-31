using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using NebulousConquestHelper.src.bot;
using Newtonsoft.Json;
using System;
using System.Drawing;
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

			// test code above, feel free to remove

			Program program = new Program();
			program.RunBotAsync().GetAwaiter().GetResult();
        }

		public async Task RunBotAsync()
        {
			String configFileName = "src/data/config.json";
			String json = "";
			if (!File.Exists(configFileName))
			{
				Console.WriteLine("config.json is missing. Copy config.example.json and put in the discord bot token");
				return;
			}
			using (FileStream fs = File.OpenRead(configFileName))
			using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync();

            ConfigJson cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
			if (string.IsNullOrEmpty(cfgjson.Token))
            {
				Console.WriteLine("Missing token in config");
				return;
            }
            DiscordConfiguration cfg = new DiscordConfiguration
			{
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				MinimumLogLevel = LogLevel.Debug,
			};

			Client = new DiscordClient(cfg);

			Client.Ready += Client_Ready;
			Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientErrored;

			
			var ccfg = new CommandsNextConfiguration
			{
				StringPrefixes = new[] { cfgjson.CommandPrefix },
				EnableDms = true,
				EnableMentionPrefix = true
			};

			Commands = Client.UseCommandsNext(ccfg);

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

			Commands.RegisterCommands<BotCommands>();

			Commands.SetHelpFormatter<BotHelpFormatter>();

			await Client.ConnectAsync();

			await Task.Delay(-1);
		}

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
		{
			// Log details
			e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

			// Check for lacking permissions and respond
			if (e.Exception is ChecksFailedException ex)
			{
				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} You do not have the permissions required to execute this command.",
					Color = new DiscordColor(0xFF0000) // red
				};
				await e.Context.RespondAsync(embed);
			}
		}

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
		{
			e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

			return Task.CompletedTask;
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
