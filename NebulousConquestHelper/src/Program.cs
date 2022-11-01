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

			Helper.Registry = (ComponentRegistry)Helper.ReadXMLFile(
				typeof(ComponentRegistry),
				new FilePath(Helper.DATA_FOLDER_PATH + "ComponentRegistry.xml")
			);
			Game game = (Game)Helper.ReadXMLFile(
				typeof(Game),
				new FilePath(Helper.DATA_FOLDER_PATH + "TestGame.scenario"),
				Game.Init
			);

			game.CreateNewFleet("Conquest - TF Oak.fleet", "Badnarik", Game.ConquestTeam.GreenTeam);

			foreach (Location loc in game.System.OrbitingLocations)
			{
				Console.WriteLine(loc.Name + " - " + loc.PresentFleets.Count + " Fleets");
			}

			game.System.FindLocationByName("Hui Xing").AddLagrangeStation(3, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Hui Xing").AddLagrangeStation(4, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Hui Xing").AddLagrangeStation(5, Location.LocationSubType.StationMining);

			game.System.FindLocationByName("Badnarik").AddLagrangeStation(3, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Badnarik").AddLagrangeStation(4, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Badnarik").AddLagrangeStation(5, Location.LocationSubType.StationMining);

			game.System.FindLocationByName("Druj").AddLagrangeStation(3, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Druj").AddLagrangeStation(4, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Druj").AddLagrangeStation(5, Location.LocationSubType.StationMining);

			game.System.FindLocationByName("Sphinx").AddLagrangeStation(3, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Sphinx").AddLagrangeStation(4, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Sphinx").AddLagrangeStation(5, Location.LocationSubType.StationMining);

			game.System.FindLocationByName("Mazurten").AddLagrangeStation(3, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Mazurten").AddLagrangeStation(4, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Mazurten").AddLagrangeStation(5, Location.LocationSubType.StationMining);

			game.System.FindLocationByName("Satanaze").AddLagrangeStation(3, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Satanaze").AddLagrangeStation(4, Location.LocationSubType.StationMining);
			game.System.FindLocationByName("Satanaze").AddLagrangeStation(5, Location.LocationSubType.StationMining);

			game.System.FindLocationByName("Hui Xing").Resources.Add(new Resource(ResourceType.Fuel, 0, 100, 0));
			game.System.FindLocationByName("Hui Xing").Resources.Add(new Resource(ResourceType.Parts, 7, 0, 5));

			game.Fleets[0].Fuel = 1000;
			game.Fleets[0].IssueMoveOrder("Satanaze");
			Game.ConquestTurnError error;
			for (int i = 0; i < 21; i++)
			{
				game.Advance(out error);
				if (error != Game.ConquestTurnError.NONE)
				{
					Console.WriteLine(error);
					break;
				}
			}

			Console.WriteLine(game.Fleets[0].Fuel);
			Console.WriteLine(game.System.FindLocationByName("Hui Xing").Resources[0].Stockpile);

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
