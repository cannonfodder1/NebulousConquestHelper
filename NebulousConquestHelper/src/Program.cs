using System;
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
			/*
			game.CreateNewFleet("Conquest - TF Oak.fleet", "Badnarik", Game.ConquestTeam.GreenTeam);

			foreach (Location loc in game.System.OrbitingLocations)
			{
				Console.WriteLine(loc.Name + " - " + loc.PresentFleets.Count + " Fleets");
			}

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
			*/
			Mapping.CreateSystemMap("SystemMap_Overview.png", game.System, game.DaysPassed, false, false);
			Mapping.CreateSystemMap("SystemMap_Situation.png", game.System, game.DaysPassed, true, false);
			Mapping.CreateSystemMap("SystemMap_Logistics.png", game.System, game.DaysPassed, true, true);

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
