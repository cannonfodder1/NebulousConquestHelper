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
				Console.WriteLine(loc.Name + " - " + loc.PresentFleets.Count);
				Console.WriteLine(loc.OrbitalStartDegrees + " -> " + loc.GetCurrentDegrees(13 * 7));
            }

			Mapping.CreateSystemMap(game.System);

			game.SaveGame();

			// test code above, feel free to remove

			Program program = new Program();
			program.RunBot();
        }

        private void RunBot()
        {
			DiscordBot = new Bot();
			DiscordBot.RunBotAsync().GetAwaiter().GetResult();
        }
    }

}
