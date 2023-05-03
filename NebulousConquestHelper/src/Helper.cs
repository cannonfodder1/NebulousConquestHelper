﻿using static Munitions.Magazine;
using Munitions.ModularMissiles;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
	class Helper
	{
		// TODO work out a save system with this path being different for each save
		public const string DATA_FOLDER_PATH = "./src/data/";

		public static ComponentRegistry cRegistry;
		public static MunitionRegistry mRegistry;

		public static void Init()
		{
			mRegistry = MunitionRegistry.Load("MunitionRegistry");
			cRegistry = ComponentRegistry.Load("ComponentRegistry");
		}

		public static int RestockDCLockerRestores(SerializedHullSocket socket, int availableRestores)
		{
			if (socket.ComponentState != null && socket.ComponentState is DCLockerComponent.DCLockerState)
			{
				DCLockerComponent.DCLockerState locker = (DCLockerComponent.DCLockerState)socket.ComponentState;
				int restocked = Math.Min(availableRestores, (int)locker.RestoresConsumed);
				locker.RestoresConsumed -= (uint)restocked;
				availableRestores -= restocked;
			}

			return availableRestores;
		}

		public static int RestockMagazineAmmo(SerializedHullSocket socket, int availableMetals)
		{
			BulkMagazineComponent.BulkMagazineData magazineData = (BulkMagazineComponent.BulkMagazineData)socket.ComponentData;
			BulkMagazineComponent.BulkMagazineState magazineState = (BulkMagazineComponent.BulkMagazineState)socket.ComponentState;

			if (magazineData != null && magazineState != null)
			{
				foreach (MagSaveData munitionLoad in magazineData.Load)
				{
					if (munitionLoad.MunitionKey.StartsWith("$MODMIS$"))
					{
						continue;
					}

					Munition munitionEntry = mRegistry.Get(munitionLoad.MunitionKey);

					for (int index = 0; index < magazineState.Mags.Count; index++)
					{
						MagStateData munitionState = magazineState.Mags[index];

						if (munitionState.MagazineKey == munitionLoad.MagazineKey)
						{
							double expended = munitionState.Expended;

							if (expended > 0)
							{
								int neededBatches = (int)Math.Ceiling(expended / munitionEntry.PointDivision);
								int totalPrice = neededBatches * munitionEntry.PointCost;
								int paidPrice = Math.Min(availableMetals, totalPrice);
								int totalBatches = (int)Math.Floor((double)paidPrice / munitionEntry.PointCost);
								int totalRestock = totalBatches * munitionEntry.PointDivision;
								int finalRestock = (int)Math.Min(expended, totalRestock);

								munitionState.Expended -= (uint)finalRestock;
								magazineState.Mags[index] = munitionState;
								availableMetals -= paidPrice;
							}

							break;
						}
					}

					if (availableMetals == 0)
					{
						break;
					}
				}
			}

			return availableMetals;
		}

		public static int RestockMagazineMissiles(SerializedHullSocket socket, List<SerializedMissileTemplate> missileTypes, ref int availableResources, ref int availableFuel)
		{
			BulkMagazineComponent.BulkMagazineData magazineData = (BulkMagazineComponent.BulkMagazineData)socket.ComponentData;
			BulkMagazineComponent.BulkMagazineState magazineState = (BulkMagazineComponent.BulkMagazineState)socket.ComponentState;

			if (magazineData != null && magazineState != null)
			{
				foreach (MagSaveData munitionLoad in magazineData.Load)
				{
					if (!munitionLoad.MunitionKey.StartsWith("$MODMIS$"))
					{
						continue;
					}

					SerializedMissileTemplate missileType = missileTypes.Find(x => munitionLoad.MunitionKey.Equals("$MODMIS$/" + x.Designation + " " + x.Nickname));

					if (availableResources < missileType.Cost)
					{
						break;
					}

					for (int index = 0; index < magazineState.Mags.Count; index++)
					{
						MagStateData munitionState = magazineState.Mags[index];

						if (munitionState.MagazineKey == munitionLoad.MagazineKey)
						{
							int expended = (int)munitionState.Expended;

							if (expended > 0)
							{
								int costToRestock = expended * missileType.Cost;
								int restockPrice = Math.Min(availableResources, costToRestock);
								restockPrice -= restockPrice % missileType.Cost;
								int numRestocked = Math.Min(restockPrice / missileType.Cost, availableFuel);

								munitionState.Expended -= (uint)numRestocked;
								magazineState.Mags[index] = munitionState;
								availableResources -= numRestocked * missileType.Cost;
								availableFuel -= numRestocked;
							}

							break;
						}
					}
				}
			}

			return availableResources;
		}

		public static int RestockLauncherMissiles(SerializedHullSocket socket, List<SerializedMissileTemplate> missileTypes, ref int availableResources, ref int availableFuel)
		{
			BaseCellLauncherComponent.CellLauncherData launcherData = (BaseCellLauncherComponent.CellLauncherData)socket.ComponentData;
			BaseCellLauncherComponent.CellLauncherState launcherState = (BaseCellLauncherComponent.CellLauncherState)socket.ComponentState;

			if (launcherData != null && launcherState != null)
			{
				foreach (MagSaveData missileLoad in launcherData.MissileLoad)
				{
					if (missileLoad.MunitionKey.StartsWith("$MODMIS$"))
					{
						SerializedMissileTemplate missileType = missileTypes.Find(x => missileLoad.MunitionKey.Equals("$MODMIS$/" + x.Designation + " " + x.Nickname));

						if (availableResources < missileType.Cost)
						{
							continue;
						}

						for (int index = 0; index < launcherState.Missiles.Count; index++)
						{
							MagStateData missileState = launcherState.Missiles[index];

							if (missileState.MagazineKey == missileLoad.MagazineKey)
							{
								int expended = (int)missileState.Expended;

								if (expended > 0)
								{
									int costToRestock = expended * missileType.Cost;
									int restockPrice = Math.Min(availableResources, costToRestock);
									restockPrice -= restockPrice % missileType.Cost;
									int numRestocked = Math.Min(restockPrice / missileType.Cost, availableFuel);

									missileState.Expended -= (uint)numRestocked;
									launcherState.Missiles[index] = missileState;
									availableResources -= numRestocked * missileType.Cost;
									availableFuel -= numRestocked;
								}

								break;
							}
						}
					}
					else
					{
						Munition munitionEntry = mRegistry.Get(missileLoad.MunitionKey);

						if (availableResources < munitionEntry.PointCost)
						{
							continue;
						}

						for (int index = 0; index < launcherState.Missiles.Count; index++)
						{
							MagStateData missileState = launcherState.Missiles[index];

							if (missileState.MagazineKey == missileLoad.MagazineKey)
							{
								double expended = missileState.Expended;

								if (expended > 0)
								{
									int divisions = (int)Math.Ceiling(expended / munitionEntry.PointDivision);
									int costToRestock = divisions * munitionEntry.PointCost;
									int restocked = Math.Min(availableResources, costToRestock);

									missileState.Expended -= (uint)restocked;
									launcherState.Missiles[index] = missileState;
									availableResources -= restocked;
								}

								break;
							}
						}
					}
				}
			}

			return availableResources;
		}

		public static int UseDCLockerRestores(SerializedHullSocket socket, int restoresToUse)
		{
			int restoreCapacity = cRegistry.Get(socket.ComponentName).Restores;

			if (socket.ComponentState != null && socket.ComponentState is DCLockerComponent.DCLockerState)
			{
				DCLockerComponent.DCLockerState locker = (DCLockerComponent.DCLockerState)socket.ComponentState;
				int restoresTaken = Math.Min(restoresToUse, restoreCapacity - (int)locker.RestoresConsumed);
				locker.RestoresConsumed += (uint)restoresTaken;
				restoresToUse -= restoresTaken;
			}

			return restoresToUse;
		}

		public static int GetShipMass(SerializedConquestShip ship)
		{
			switch (ship.HullType)
			{
				case "Stock/Sprinter Corvette":
					return 3;

				case "Stock/Raines Frigate":
					return 5;

				case "Stock/Keystone Destroyer":
					return 8;

				case "Stock/Vauxhall Light Cruiser":
					return 10;

				case "Stock/Axford Heavy Cruiser":
					return 13;

				case "Stock/Solomon Battleship":
					return 21;

				case "Stock/Shuttle Clipper":
					return 1;

				case "Stock/Tug Clipper":
					return 3;

				case "Stock/Bulk Clipper":
					return 5;

				case "Stock/Ocello Cruiser":
					return 12;

				case "Stock/Bulker Line Ship":
					return 15;

				case "Stock/Container Line Ship":
					return 15;

				default:
					Console.WriteLine("ERROR! Unknown Hull Type: " + ship.HullType);
					return ship.Cost / 100;
			}
		}
	}
}
