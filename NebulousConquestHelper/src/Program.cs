using System;
using Utility;

namespace NebulousConquestHelper
{
	class Program
	{
		/*
		private Bot DiscordBot { get; set; }
		*/

		static void Main(string[] args)
		{
			IntegrationTest();
		}

		static void IntegrationTest()
		{
			Helper.mRegistry = MunitionRegistry.Load("MunitionRegistry");
			Helper.cRegistry = ComponentRegistry.Load("ComponentRegistry");

			Game game = Game.Load("TestGame");
			game.SpawnFleets();
			game.SpawnResources();

			Fleet oak = game.CreateNewFleet("Conquest - TF Oak", "Sph-L4", Game.ConquestTeam.GreenTeam);

			Console.WriteLine("Fuel: " + oak.Fuel);
			Console.WriteLine("Restores: " + oak.Restores);

			oak.RestockFromLocation();
			oak.IssueMoveOrder("Sat-L3");

			Console.WriteLine(game.System.FindLocationByName("Sph-L4").PrintResources());
			Console.WriteLine(game.System.FindLocationByName("Hui Xing").PrintResources());

			Game.ConquestTurnError error;
			for (int i = 0; i < 14; i++)
			{
				game.Advance(out error);
				if (error != Game.ConquestTurnError.NONE)
				{
					Console.WriteLine(error);
					break;
				}
			}

			Console.WriteLine("Fuel: " + oak.Fuel);
			Console.WriteLine("Restores: " + oak.Restores);

			Console.WriteLine(game.System.FindLocationByName("Sph-L4").PrintResources());
			Console.WriteLine(game.System.FindLocationByName("Hui Xing").PrintResources());

			Fleet ash = game.CreateNewFleet("Conquest - TF Ash", "Sat-L3", Game.ConquestTeam.OrangeTeam);
			ash.RestockFromLocation(false);
			Console.WriteLine(game.System.FindLocationByName("Sat-L3").PrintResources());

			Mapping.CreateSystemMap("SystemMap_Overview.png", game.System, game.DaysPassed, false, false);
			Mapping.CreateSystemMap("SystemMap_Situation.png", game.System, game.DaysPassed, true, false);
			Mapping.CreateSystemMap("SystemMap_Logistics.png", game.System, game.DaysPassed, true, true);

			game.SaveGame("TestGame");
		}

		/*
		private void RunBot()
		{
			DiscordBot.RunBotAsync().GetAwaiter().GetResult();
		}
		*/
	}
}
