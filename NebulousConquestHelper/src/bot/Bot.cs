using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
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
    public class Bot
    {
        public readonly EventId BotEventId = new EventId(42, "Nebulous-QB");

        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
		
		public Game Game { get; set; }	

		public Bot(Game game)
        {
			Game = game;
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

			BotConfig cfgjson = JsonConvert.DeserializeObject<BotConfig>(json);
			if (string.IsNullOrEmpty(cfgjson.Token))
			{
				Console.WriteLine("Missing token in config");
				return;
			}
			DiscordConfiguration cfg = new DiscordConfiguration
			{
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,
				Intents = DiscordIntents.AllUnprivileged 
					| DiscordIntents.DirectMessages 
					| DiscordIntents.DirectMessageTyping 
					| DiscordIntents.GuildMessages 
					| DiscordIntents.GuildMessageTyping,

				AutoReconnect = true,
				MinimumLogLevel = LogLevel.Debug,
			};

			Client = new DiscordClient(cfg);

			Client.Ready += Client_Ready;
			Client.GuildAvailable += Client_GuildAvailable;
			Client.ClientErrored += Client_ClientErrored;
            Client.InteractionCreated += Client_InteractionCreated;


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

        private async Task Client_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
        {
			Client.Logger.LogInformation(BotEventId, $"{e.Interaction.User.Username} sent an interaction: {e.Interaction.Data.Name}");

            try
            {
				await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
			}
            catch (Exception ex)
            {
				Client.Logger.LogError(BotEventId, "Exception when responding to interaction");
                throw ex;
            }

            try
            {
				if (e.Interaction.Data.Name == "map")
				{
					Mapping.CreateSystemMap(Game.System, Game.DaysPassed);
					using (FileStream fs = new FileStream(Helper.DATA_FOLDER_PATH + "SystemMap.png", FileMode.Open, FileAccess.Read))
					{
						await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
							.WithContent("Here is the map for the current game state:")
							.AddFile("SystemMap.png", fs));
					}
				}
			}
            catch (Exception ex)
            {
				await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
					new DiscordInteractionResponseBuilder()
					.WithContent("Failed to retrieve map"));
				throw ex;
            }
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
}
