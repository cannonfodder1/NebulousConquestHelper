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

			Helper.Registry = ComponentRegistry.Load("ComponentRegistry");
			Game game = Game.Load("TestGame");
			game.SpawnFleets();
			game.SpawnResources();

			Fleet oak = game.CreateNewFleet("Conquest - TF Oak", "Sph-L4", Game.ConquestTeam.GreenTeam);

			Console.WriteLine("Fuel: " + oak.Fuel);
			Console.WriteLine("Restores: " + oak.Restores);

			oak.RestockFromLocation();
			oak.IssueMoveOrder("Sat-L3");

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

			Mapping.CreateSystemMap("SystemMap_Overview.png", game.System, game.DaysPassed, false, false);
			Mapping.CreateSystemMap("SystemMap_Situation.png", game.System, game.DaysPassed, true, false);
			Mapping.CreateSystemMap("SystemMap_Logistics.png", game.System, game.DaysPassed, true, true);

			game.SaveGame();

			// test code above, feel free to remove
		}

		private void RunBot()
		{
			DiscordBot.RunBotAsync().GetAwaiter().GetResult();
		}
	}
}
