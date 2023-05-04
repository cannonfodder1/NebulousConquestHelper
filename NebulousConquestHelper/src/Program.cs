using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
	class Program
	{
		static void Main(string[] args)
		{
			Helper.Init();

			IntegrationTest();
		}

		// TODO make the text output of the program more organized and readable
		private static void IntegrationTest()
		{
			Game game = Game.Load("TestGame");
			game.LoadAllFleets();
			game.SetupResources();

			// resupply and movement tests

			Fleet oak = game.CreateNewFleet("Conquest - TF Oak", "Sph-L4", Game.ConquestTeam.GreenTeam);

			Debug.Assert(0 == oak.Fuel);

			int oakFuelPre = oak.Fuel;
			int sphFuelPre = game.System.FindLocationByName("Sph-L4").GetResourceStockpile(ResourceType.Fuel);
			int oakRestoresPre = oak.Restores;
			int sphRestoresPre = game.System.FindLocationByName("Sph-L4").GetResourceStockpile(ResourceType.Restores);

			oak.RestockFromLocation();

			int oakFuelAft = oak.Fuel;
			int sphFuelAft = game.System.FindLocationByName("Sph-L4").GetResourceStockpile(ResourceType.Fuel);
			int oakRestoresAft = oak.Restores;
			int sphRestoresAft = game.System.FindLocationByName("Sph-L4").GetResourceStockpile(ResourceType.Restores);

			int oakFuelDiff = oakFuelPre - oakFuelAft;
			int sphFuelDiff = sphFuelPre - sphFuelAft;
			int oakRestoresDiff = oakRestoresPre - oakRestoresAft;
			int sphRestoresDiff = sphRestoresPre - sphRestoresAft;

			Debug.Assert(oakFuelDiff * -1 == sphFuelDiff);
			Debug.Assert(oakRestoresDiff * -1 == sphRestoresDiff);

			oakFuelPre = oak.Fuel;

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

			oakFuelAft = oak.Fuel;
			oakFuelDiff = oakFuelPre - oakFuelAft;
			int oakFuelDrain = (int)(oak.GetFuelConsumption() * Game.THRU_BELT_FUEL_MULT);

			Debug.Assert(oakFuelDrain == oakFuelDiff);

			// restore and repair tests

			game.System.FindLocationByName("Sat-L3").ControllingTeam = Game.ConquestTeam.GreenTeam;
			Fleet ash = game.CreateNewFleet("Conquest - TF Ash", "Sat-L3", Game.ConquestTeam.GreenTeam);

			int ashFuelPre = ash.Fuel;
			int satFuelPre = game.System.FindLocationByName("Sat-L3").GetResourceStockpile(ResourceType.Fuel);
			int ashRestoresPre = ash.Restores;
			int satRestoresPre = game.System.FindLocationByName("Sat-L3").GetResourceStockpile(ResourceType.Restores);

			ash.RestockFromLocation();
			int ashMissilesRestocked = 23;

			int ashFuelAft = ash.Fuel;
			int satFuelAft = game.System.FindLocationByName("Sat-L3").GetResourceStockpile(ResourceType.Fuel);
			int ashRestoresAft = ash.Restores;
			int satRestoresAft = game.System.FindLocationByName("Sat-L3").GetResourceStockpile(ResourceType.Restores);

			int ashFuelDiff = ashFuelPre - ashFuelAft;
			int satFuelDiff = satFuelPre - satFuelAft - ashMissilesRestocked;
			int ashRestoresDiff = ashRestoresPre - ashRestoresAft;
			int satRestoresDiff = satRestoresPre - satRestoresAft;

			Debug.Assert(ashFuelDiff * -1 == satFuelDiff);
			Debug.Assert(ashRestoresDiff * -1 == satRestoresDiff);

			ashRestoresPre = ashRestoresAft;
			satRestoresPre = satRestoresAft;
            ash.GetDamageReport(out int ashDamagedPre, out int ashDestroyedPre);

            ash.RestoreAllShips();

			ashRestoresAft = ash.Restores;
			satRestoresAft = game.System.FindLocationByName("Sat-L3").GetResourceStockpile(ResourceType.Restores);
			ash.GetDamageReport(out int ashDamagedAft, out int ashDestroyedAft);

			ashRestoresDiff = ashRestoresPre - ashRestoresAft;
			satRestoresDiff = satRestoresPre - satRestoresAft;

			Debug.Assert(ashDestroyedAft == 0);
			Debug.Assert(ashDestroyedPre == ashRestoresDiff + satRestoresDiff);

			ashDamagedPre = ashDamagedAft;

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

			int satPartsPre = 100;
			int satPartsAft = game.System.FindLocationByName("Sat-L3").GetResourceStockpile(ResourceType.Parts);
			int satPartsDiff = satPartsPre - satPartsAft;

			ash.GetDamageReport(out ashDamagedAft, out ashDestroyedAft);
			int ashDamagedDiff = ashDamagedPre - ashDamagedAft;

			Debug.Assert(satPartsDiff == ashDamagedDiff);

			ash.IssueIdleOrder();

			// fleet merging and splitting tests

			for (int i = 0; i < 6; i++)
			{
				game.Advance(out error);
				if (error != Game.ConquestTurnError.NONE)
				{
					Console.WriteLine(error);
					break;
				}
			}

			ash.IssueIdleOrder();
			oak.IssueIdleOrder();

			oakFuelPre = oak.Fuel;
			oakRestoresPre = oak.Restores;
			int oakFuelCapPre = oak.GetFuelCapacity();
			int oakRestoresCapPre = oak.GetRestoreCapacity();

			ashFuelPre = ash.Fuel;
			ashRestoresPre = ash.Restores;
			int ashFuelCapPre = ash.GetFuelCapacity();
			int ashRestoresCapPre = ash.GetRestoreCapacity();

			File.Delete(oak.BackingFile.Path.Directory + "\\" + "Conquest - TF Contorta.fleet");

			Fleet contorta = game.MergeIntoFleet(ash, oak, "Conquest - TF Contorta");

			int contortaFuelPre = contorta.Fuel;
			int contortaRestoresPre = contorta.Restores;
			int contortaFuelCapPre = contorta.GetFuelCapacity();
			int contortaRestoresCapPre = contorta.GetRestoreCapacity();

			Debug.Assert(contortaFuelPre == oakFuelPre + ashFuelPre);
			Debug.Assert(contortaRestoresPre == oakRestoresPre + ashRestoresPre);
			Debug.Assert(contortaFuelCapPre == oakFuelCapPre + ashFuelCapPre);
			Debug.Assert(contortaRestoresCapPre == oakRestoresCapPre + ashRestoresCapPre);

			File.Delete(contorta.BackingFile.Path.Directory + "\\" + "Conquest - TF Pinus.fleet");

			List<string> shipsToSplit = new List<string>();
			shipsToSplit.Add("Moral Failing");
			shipsToSplit.Add("Apply Damage");
			Fleet pinus = game.SplitFromFleet(contorta, shipsToSplit, "Conquest - TF Pinus");

			int pinusFuelAft = pinus.Fuel;
			int pinusRestoresAft = pinus.Restores;
			int pinusFuelCapAft = pinus.GetFuelCapacity();
			int pinusRestoresCapAft = pinus.GetRestoreCapacity();

			int contortaFuelAft = contorta.Fuel;
			int contortaRestoresAft = contorta.Restores;
			int contortaFuelCapAft = contorta.GetFuelCapacity();
			int contortaRestoresCapAft = contorta.GetRestoreCapacity();

			Debug.Assert(contortaFuelAft == contortaFuelPre - pinusFuelAft);
			Debug.Assert(contortaRestoresAft == contortaRestoresPre - pinusRestoresAft);
			Debug.Assert(contortaFuelCapAft == contortaFuelCapPre - pinusFuelCapAft);
			Debug.Assert(contortaRestoresCapAft == contortaRestoresCapPre - pinusRestoresCapAft);

			// create sample mapmodes

			Mapping.CreateSystemMap("SystemMap_Overview.png", game.System, game.DaysPassed, false, false);
			Mapping.CreateSystemMap("SystemMap_Situation.png", game.System, game.DaysPassed, true, false);
			Mapping.CreateSystemMap("SystemMap_Logistics.png", game.System, game.DaysPassed, true, true);

			game.SaveGame("TestGame");
		}
	}
}
