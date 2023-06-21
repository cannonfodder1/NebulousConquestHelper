using static Munitions.Magazine;
using Munitions.ModularMissiles;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
	class Helper
	{
		public const string DATA_FOLDER_PATH = "./src/data/";

		public static ComponentRegistry cRegistry;
		public static MunitionRegistry mRegistry;

		public static void Init()
		{
			mRegistry = MunitionRegistry.Load("MunitionRegistry");
			cRegistry = ComponentRegistry.Load("ComponentRegistry");
		}

		public const int METAL_PER_AMMO_POINT = 4;

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

		public static void RestockMagazineAmmo(SerializedHullSocket socket, ref int availableMetals)
		{
			BulkMagazineComponent.BulkMagazineData magazineData = (BulkMagazineComponent.BulkMagazineData)socket.ComponentData;
			BulkMagazineComponent.BulkMagazineState magazineState = (BulkMagazineComponent.BulkMagazineState)socket.ComponentState;

			if (magazineData != null && magazineState != null)
			{
				foreach (MagSaveData munitionLoad in magazineData.Load)
				{
					if (IsMissile(munitionLoad.MunitionKey))
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
								int totalPrice = neededBatches * munitionEntry.PointCost * METAL_PER_AMMO_POINT;
								int paidPrice = Math.Min(availableMetals, totalPrice);
								int totalBatches = (int)Math.Floor((double)paidPrice / (munitionEntry.PointCost * METAL_PER_AMMO_POINT));
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
		}

		public static void RestockMagazineMissiles(SerializedHullSocket socket, List<SerializedMissileTemplate> missileTypes, ref int availableMetals, ref int availableParts, ref int availableFuel)
		{
			BulkMagazineComponent.BulkMagazineData magazineData = (BulkMagazineComponent.BulkMagazineData)socket.ComponentData;
			BulkMagazineComponent.BulkMagazineState magazineState = (BulkMagazineComponent.BulkMagazineState)socket.ComponentState;

			if (magazineData != null && magazineState != null)
			{
				foreach (MagSaveData munitionLoad in magazineData.Load)
				{
					if (!IsMissile(munitionLoad.MunitionKey))
					{
						continue;
					}

					if (IsModularMissile(munitionLoad.MunitionKey))
					{
						SerializedMissileTemplate missileType = missileTypes.Find(x => munitionLoad.MunitionKey.Equals("$MODMIS$/" + x.Designation + " " + x.Nickname));

						if (Math.Min(availableMetals, availableParts) < missileType.Cost)
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
									int restockPrice = Math.Min(Math.Min(availableMetals, availableParts), costToRestock);
									restockPrice -= restockPrice % missileType.Cost;
									int numRestocked = Math.Min(restockPrice / missileType.Cost, availableFuel / GetMissileFuel(munitionLoad.MunitionKey));

									munitionState.Expended -= (uint)numRestocked;
									magazineState.Mags[index] = munitionState;
									availableMetals -= numRestocked * missileType.Cost;
									availableParts -= numRestocked * missileType.Cost;
									availableFuel -= numRestocked * GetMissileFuel(munitionLoad.MunitionKey);
								}

								break;
							}
						}
					}
                    else
					{
						// munition is a Mine or Mine Container or Rocket Container, refill using basic ammo cost

						Munition munitionEntry = mRegistry.Get(munitionLoad.MunitionKey);

						if (availableMetals < munitionEntry.PointCost)
						{
							continue;
						}

						for (int index = 0; index < magazineState.Mags.Count; index++)
						{
							MagStateData munitionState = magazineState.Mags[index];

							if (munitionState.MagazineKey == munitionLoad.MagazineKey)
							{
								double expended = munitionState.Expended;

								if (expended > 0)
								{
									int divisions = (int)Math.Ceiling(expended / munitionEntry.PointDivision);
									int costToRestock = divisions * munitionEntry.PointCost * METAL_PER_AMMO_POINT;
									int restocked = Math.Min(availableMetals, costToRestock);

									munitionState.Expended -= (uint)restocked;
									magazineState.Mags[index] = munitionState;
									availableMetals -= restocked;
								}

								break;
							}
						}
					}
				}
			}
		}

		public static void RestockLauncherMissiles(SerializedHullSocket socket, List<SerializedMissileTemplate> missileTypes, ref int availableMetals, ref int availableParts, ref int availableFuel)
		{
			BaseCellLauncherComponent.CellLauncherData launcherData = (BaseCellLauncherComponent.CellLauncherData)socket.ComponentData;
			BaseCellLauncherComponent.CellLauncherState launcherState = (BaseCellLauncherComponent.CellLauncherState)socket.ComponentState;

			if (launcherData != null && launcherState != null)
			{
				foreach (MagSaveData missileLoad in launcherData.MissileLoad)
				{
					if (IsModularMissile(missileLoad.MunitionKey))
					{
						SerializedMissileTemplate missileType = missileTypes.Find(x => missileLoad.MunitionKey.Equals("$MODMIS$/" + x.Designation + " " + x.Nickname));

						if (Math.Min(availableMetals, availableParts) < missileType.Cost)
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
									int restockPrice = Math.Min(Math.Min(availableMetals, availableParts), costToRestock);
									restockPrice -= restockPrice % missileType.Cost;
									int numRestocked = Math.Min(restockPrice / missileType.Cost, availableFuel / GetMissileFuel(missileLoad.MunitionKey));

									missileState.Expended -= (uint)numRestocked;
									launcherState.Missiles[index] = missileState;
									availableMetals -= numRestocked * missileType.Cost;
									availableParts -= numRestocked * missileType.Cost;
									availableFuel -= numRestocked * GetMissileFuel(missileLoad.MunitionKey);
								}

								break;
							}
						}
					}
					else
					{
						Munition munitionEntry = mRegistry.Get(missileLoad.MunitionKey);

						if (availableMetals < munitionEntry.PointCost)
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
									int costToRestock = divisions * munitionEntry.PointCost * METAL_PER_AMMO_POINT;
									int restocked = Math.Min(availableMetals, costToRestock);

									missileState.Expended -= (uint)restocked;
									launcherState.Missiles[index] = missileState;
									availableMetals -= restocked;
								}

								break;
							}
						}
					}
				}
			}
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

		public static bool IsModularMissile(string munitionKey)
		{
			return munitionKey.StartsWith("$MODMIS$");
		}
		
		public static bool IsMissile(string munitionKey)
		{
			if (munitionKey.StartsWith("$MODMIS$"))
            {
				return true;
            }
			if (munitionKey.Contains("Mine"))
			{
				return true;
			}
			if (munitionKey.Contains("Rocket Container"))
			{
				return true;
			}

			return false;
		}
		
		public static int GetMissileFuel(string munitionKey)
		{
			if (munitionKey.StartsWith("$MODMIS$/SGM-H-3"))
			{
				return 2;
			}
			if (munitionKey.StartsWith("$MODMIS$/SGM-1"))
			{
				return 0;
			}
			if (munitionKey.Contains("Mine") && !munitionKey.Contains("Container"))
			{
				return 0;
			}
			if (munitionKey.Contains("S1 Rocket"))
			{
				return 0;
			}

			return 1;
		}
	}
}
