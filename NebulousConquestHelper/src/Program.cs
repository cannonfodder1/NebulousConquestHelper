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
    class Program
	{
		private Bot DiscordBot { get; set; }

		static void Main(string[] args)
		{
			// test code below, feel free to remove

			Helper.registry = (ComponentRegistry)Helper.ReadXMLFile(
				typeof(ComponentRegistry),
				new FilePath(Helper.DATA_FOLDER_PATH + "ComponentRegistry.xml")
			);
			GameInfo game = (GameInfo)Helper.ReadXMLFile(
				typeof(GameInfo),
				new FilePath(Helper.DATA_FOLDER_PATH + "TestGame.scenario"),
				GameInfo.Init
			);

			game.CreateNewFleet("Conquest - TF Oak.fleet", "Badnarik");

			foreach (LocationInfo loc in game.System.OrbitingLocations)
            {
				Console.WriteLine(loc.Name + " - " + loc.PresentFleets.Count + " Fleets");
            }

			game.Fleets[0].IssueMoveOrder("Satanaze");
			GameInfo.ConquestTurnError error;
			for (int i = 0; i < 21; i++)
			{
				game.Advance(out error);
			}

			Mapping.CreateSystemMap(game.System, game.DaysPassed);

			game.SaveGame();

			// test code above, feel free to remove

			Program program = new Program();
			program.DiscordBot = new Bot(game);
			program.RunBot();
        }

        private void RunBot()
        {
			DiscordBot.RunBotAsync().GetAwaiter().GetResult();
        }
    }

}
