using System;
using System.Collections.Generic;
using System.IO;

namespace NebulousConquestHelper
{
	class Program
	{
		static void Main(string[] args)
		{
			Helper.Init();

			IntegrationTest();
		}

		// TODO rework integration test to use asserts for readability
		// TODO make the text output of the program more organized and readable
		private static void IntegrationTest()
		{
			Game game = Game.Load("TestGame");
			game.LoadAllFleets();
			game.SetupResources();

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

			oak.PrintDamageReport();

			Console.WriteLine(game.System.FindLocationByName("Sph-L4").PrintResources());
			Console.WriteLine(game.System.FindLocationByName("Hui Xing").PrintResources());

			Fleet ash = game.CreateNewFleet("Conquest - TF Ash", "Sat-L3", Game.ConquestTeam.GreenTeam);
			ash.RestockFromLocation(false);
			Console.WriteLine(game.System.FindLocationByName("Sat-L3").PrintResources());

			ash.PrintDamageReport();

			ash.RestoreAllShips();
			ash.IssueRepairOrder();

			for (int i = 0; i < 1; i++)
			{
				game.Advance(out error);
				if (error != Game.ConquestTurnError.NONE)
				{
					Console.WriteLine(error);
					break;
				}
			}

			ash.PrintDamageReport();
			Console.WriteLine(game.System.FindLocationByName("Sat-L3").PrintResources());

			ash.IssueIdleOrder();

			for (int i = 0; i < 7; i++)
			{
				game.Advance(out error);
				if (error != Game.ConquestTurnError.NONE)
				{
					Console.WriteLine(error);
					break;
				}
			}

			ash.PrintDamageReport();
			Console.WriteLine(game.System.FindLocationByName("Sat-L3").PrintResources());

			ash.IssueIdleOrder();
			oak.IssueIdleOrder();

			Console.WriteLine("Pre Fuel: " + oak.Fuel + "/" + oak.GetFuelCapacity());
			Console.WriteLine("Pre Rest: " + oak.Restores + "/" + oak.GetRestoreCapacity());
			Console.WriteLine("Pre Fuel: " + ash.Fuel + "/" + ash.GetFuelCapacity());
			Console.WriteLine("Pre Rest: " + ash.Restores + "/" + ash.GetRestoreCapacity());

			Fleet contorta = game.MergeIntoFleet(ash, oak, "Conquest - TF Contorta");

			Console.WriteLine("Mid Fuel: " + contorta.Fuel + "/" + contorta.GetFuelCapacity());
			Console.WriteLine("Mid Rest: " + contorta.Restores + "/" + contorta.GetRestoreCapacity());

			File.Delete(contorta.BackingFile.Path.Directory + "\\" + "Conquest - TF Pinus.fleet");

			List<string> shipsToSplit = new List<string>();
			shipsToSplit.Add("Moral Failing");
			shipsToSplit.Add("Apply Damage");
			Fleet pinus = game.SplitFromFleet(contorta, shipsToSplit, "Conquest - TF Pinus");

			Console.WriteLine("Aft Fuel: " + contorta.Fuel + "/" + contorta.GetFuelCapacity());
			Console.WriteLine("Aft Rest: " + contorta.Restores + "/" + contorta.GetRestoreCapacity());
			Console.WriteLine("Aft Fuel: " + pinus.Fuel + "/" + pinus.GetFuelCapacity());
			Console.WriteLine("Aft Rest: " + pinus.Restores + "/" + pinus.GetRestoreCapacity());

			Mapping.CreateSystemMap("SystemMap_Overview.png", game.System, game.DaysPassed, false, false);
			Mapping.CreateSystemMap("SystemMap_Situation.png", game.System, game.DaysPassed, true, false);
			Mapping.CreateSystemMap("SystemMap_Logistics.png", game.System, game.DaysPassed, true, true);

			game.SaveGame("TestGame");
		}
	}
}
