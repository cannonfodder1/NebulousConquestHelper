using Munitions.ModularMissiles;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;
using static Munitions.Magazine;

namespace NebulousConquestHelper
{
	[XmlType("Fleet")]
	[Serializable]
	public class Fleet : Backed<SerializedConquestFleet>
	{
		private const int FUEL_BURNED_PER_MASS = 4;
		private const int MAXIMUM_BURNS_OF_FUEL = 20;

		public enum FleetOrderType
		{
			[XmlEnum] None,
			[XmlEnum] Move,
			[XmlEnum] Idle,
			[XmlEnum] InTransit
		}

		public class FleetOrderData
		{
			public string MoveToLocation;
			public bool DefendNearby;

			public FleetOrderData() { }

			public FleetOrderData(string destination)
			{
				MoveToLocation = destination;
			}

			public FleetOrderData(bool defend)
			{
				DefendNearby = defend;
			}
		}

		private static List<string> MAIN_DRIVE_COMPONENTS = new List<string>();
		private static List<string> DC_LOCKER_COMPONENTS = new List<string>();
		private static List<string> MAGAZINE_COMPONENTS = new List<string>();
		private static List<string> LAUNCHER_COMPONENTS = new List<string>();

		public string LocationName;
		public int Restores;
		public int Fuel;
		public Game.ConquestTeam ControllingTeam;
		public FleetOrderType OrderType = FleetOrderType.None;
		public FleetOrderData OrderData = null;

		[XmlIgnore] public Location Location;

		public Fleet() { }

		public Fleet(BackingXmlFile<SerializedConquestFleet> backingFile, string locationName, Game.ConquestTeam team)
		{
			LocationName = locationName;
			ControllingTeam = team;
			this.BackingFile = backingFile;

			UpdateRestoreCount();
		}

		public void SaveFleet()
		{
			BackingFile.Object = XML;
		}

		public void ProcessBattleResults(bool losingTeam = false)
		{
			List<SerializedConquestShip> removeShips = new List<SerializedConquestShip>();

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				if (losingTeam)
				{
					if (ship.SavedState.Eliminated == EliminationReason.NotEliminated)
					{
						bool canEscape = IsShipMobile(ship);
						if (!canEscape)
						{
							Console.WriteLine(ship.Name + " has no operational drive modules and cannot escape");
							ship.SavedState.Eliminated = EliminationReason.Destroyed;
							ship.SavedState.LaunchedLifeboats = true;
						}
					}
				}

				if (ship.SavedState.Eliminated == EliminationReason.Withdrew)
				{
					ship.SavedState.Eliminated = EliminationReason.NotEliminated;
					ship.SavedState.Vaporized = false;
					ship.SavedState.LaunchedLifeboats = false;
				}

				if (ship.SavedState.Eliminated != EliminationReason.NotEliminated)
				{
					removeShips.Add(ship);
				}
			}

			foreach (var deadShip in removeShips)
			{
				XML.Ships.Remove(deadShip);
			}

			UpdateRestoreCount();
		}

		private static void InitializeDriveComponents()
		{
			if (MAIN_DRIVE_COMPONENTS.Count < 11)
			{
				string prefix = "Stock/";
				MAIN_DRIVE_COMPONENTS.Clear();
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM200 Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM200R Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM280 'Raider' Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM230 'Whiplash' Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM240 'Dragonfly' Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM30X 'Prowler' Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM500 Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM500R Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM580 'Raider' Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM530 'Whiplash' Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "FM540 'Dragonfly' Drive");
			}
		}

		private static void InitializeDamconComponents()
		{
			if (DC_LOCKER_COMPONENTS.Count < 4)
			{
				string prefix = "Stock/";
				DC_LOCKER_COMPONENTS.Clear();
				DC_LOCKER_COMPONENTS.Add(prefix + "Rapid DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Small DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Large DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Reinforced DC Locker");
			}
		}

		private static void InitializeMagazineComponents()
		{
			if (MAGAZINE_COMPONENTS.Count < 2)
			{
				string prefix = "Stock/";
				MAGAZINE_COMPONENTS.Clear();
				MAGAZINE_COMPONENTS.Add(prefix + "Bulk Magazine");
				MAGAZINE_COMPONENTS.Add(prefix + "Reinforced Magazine");
			}
		}

		private static void InitializeLauncherComponents()
		{
			if (LAUNCHER_COMPONENTS.Count < 9)
			{
				string prefix = "Stock/";
				LAUNCHER_COMPONENTS.Clear();
				LAUNCHER_COMPONENTS.Add(prefix + "VLS-1-23 Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "VLS-1-46 Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "VLS-2 Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "VLS-3 Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "CLS-3 Launcher");
			}
		}

		public void UpdateRestoreCount()
		{
			int totalRestores = 0;

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				totalRestores += GetShipRestores(ship);
			}

			Restores = totalRestores;
		}

		public int GetRestoreCapacity()
		{
			InitializeDamconComponents();

			int totalRestores = 0;

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				foreach (SerializedHullSocket socket in ship.SocketMap)
				{
					if (DC_LOCKER_COMPONENTS.Contains(socket.ComponentName))
					{
						totalRestores += Helper.cRegistry.Components.Find(x => x.Name == socket.ComponentName).Restores;
					}
				}
			}

			return totalRestores;
		}

		public static bool IsShipMobile(SerializedConquestShip ship)
		{
			if (ship.SavedState == null) return true;

			InitializeDriveComponents();

			foreach (SerializedHullSocket socket in ship.SocketMap)
			{
				if (MAIN_DRIVE_COMPONENTS.Contains(socket.ComponentName))
				{
					foreach (SerializedPartDamage part in ship.SavedState.Damage.Parts)
					{
						if (part.Key == socket.Key)
						{
							Component component = Helper.cRegistry.Components.Find(x => x.Name == socket.ComponentName);
							if (!part.Destroyed && part.HP >= component.MinHP)
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public static int GetShipRestores(SerializedConquestShip ship)
		{
			InitializeDamconComponents();

			int initialRestores = 0;
			int expendedRestores = 0;

			foreach (SerializedHullSocket socket in ship.SocketMap)
			{
				if (DC_LOCKER_COMPONENTS.Contains(socket.ComponentName))
				{
					initialRestores += Helper.cRegistry.Components.Find(x => x.Name == socket.ComponentName).Restores;
					//Console.WriteLine("ADDING RESTORES: " + initialRestores);
					if (socket.ComponentState != null && socket.ComponentState is DCLockerComponent.DCLockerState)
					{
						DCLockerComponent.DCLockerState locker = (DCLockerComponent.DCLockerState)socket.ComponentState;
						expendedRestores += (int)locker.RestoresConsumed;
						//Console.WriteLine("NIXING RESTORES: " + expendedRestores);
					}
				}
			}

			return initialRestores - expendedRestores;
		}

		public void IssueMoveOrder(string destination)
		{
			OrderType = FleetOrderType.Move;
			OrderData = new FleetOrderData(destination);
		}

		public void IssueIdleOrder(bool defend)
		{
			OrderType = FleetOrderType.Idle;
			OrderData = new FleetOrderData(defend);
		}

		public int GetFuelConsumption()
		{
			int mass = 0;

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				switch (ship.HullType)
				{
					case "Stock/Sprinter Corvette":
						mass += 3;
						break;
					case "Stock/Raines Frigate":
						mass += 5;
						break;
					case "Stock/Keystone Destroyer":
						mass += 8;
						break;
					case "Stock/Vauxhall Light Cruiser":
						mass += 10;
						break;
					case "Stock/Axford Heavy Cruiser":
						mass += 13;
						break;
					case "Stock/Solomon Battleship":
						mass += 21;
						break;
					default:
						Console.WriteLine("ERROR! Unknown Hull Type: " + ship.HullType);
						mass += ship.Cost / 100;
						break;
				}
			}

			return mass * FUEL_BURNED_PER_MASS;
		}

		public int GetFuelCapacity()
		{
			return GetFuelConsumption() * MAXIMUM_BURNS_OF_FUEL;
		}

		public void RestockFromLocation(bool restockFuel = true, bool restockRestores = true, bool restockAmmo = true, bool restockMissiles = true)
		{
			if (Location.ControllingTeam != ControllingTeam)
			{
				Console.WriteLine("ERROR! Fleet attempting to restock from location of opposite team: " + Location.Name);
				return;
			}

			if (restockFuel && Location.Resources.Exists(x => (x.Type == ResourceType.Fuel) && (x.Stockpile > 0)))
			{
				Resource resource = Location.Resources.Find(x => (x.Type == ResourceType.Fuel) && (x.Stockpile > 0));
				int amount = Math.Min(resource.Stockpile, GetFuelCapacity() - Fuel);
				resource.Stockpile -= amount;
				Fuel += amount;
			}

			if (restockRestores && Location.Resources.Exists(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0)))
			{
				Resource resource = Location.Resources.Find(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0));
				int availableRestores = resource.Stockpile;
				InitializeDamconComponents();

				foreach (SerializedConquestShip ship in XML.Ships)
				{
					if (availableRestores == 0) break;
					foreach (SerializedHullSocket socket in ship.SocketMap)
					{
						if (availableRestores == 0) break;
						if (DC_LOCKER_COMPONENTS.Contains(socket.ComponentName))
                        {
                            availableRestores = RestockDCLocker(socket, availableRestores);
                        }
                    }
				}

				resource.Stockpile = availableRestores;
				UpdateRestoreCount();
			}

			if (restockAmmo && Location.Resources.Exists(x => (x.Type == ResourceType.Metals) && (x.Stockpile > 0)))
			{
				Resource resource = Location.Resources.Find(x => (x.Type == ResourceType.Metals) && (x.Stockpile > 0));
				int availableMetals = resource.Stockpile;
				InitializeMagazineComponents();

				foreach (SerializedConquestShip ship in XML.Ships)
				{
					if (availableMetals == 0) break;
					foreach (SerializedHullSocket socket in ship.SocketMap)
					{
						if (availableMetals == 0) break;
						if (MAGAZINE_COMPONENTS.Contains(socket.ComponentName))
						{
							availableMetals = RestockMagazineAmmo(socket, availableMetals);
						}
					}
				}

				resource.Stockpile = availableMetals;
			}

			if (restockMissiles
				&& Location.Resources.Exists(x => (x.Type == ResourceType.Metals) && (x.Stockpile > 0))
				&& Location.Resources.Exists(x => (x.Type == ResourceType.Parts) && (x.Stockpile > 0))
				&& Location.Resources.Exists(x => (x.Type == ResourceType.Fuel) && (x.Stockpile > 0))
				)
			{
				Resource resourceMetals = Location.Resources.Find(x => (x.Type == ResourceType.Metals) && (x.Stockpile > 0));
				Resource resourceParts = Location.Resources.Find(x => (x.Type == ResourceType.Parts) && (x.Stockpile > 0));
				Resource resourceFuel = Location.Resources.Find(x => (x.Type == ResourceType.Fuel) && (x.Stockpile > 0));

				int availableResources = Math.Min(resourceFuel.Stockpile, Math.Min(resourceParts.Stockpile, resourceMetals.Stockpile));
				int futureResources = availableResources;

				InitializeMagazineComponents();
				InitializeLauncherComponents();

				foreach (SerializedConquestShip ship in XML.Ships)
				{
					if (futureResources == 0) break;
					foreach (SerializedHullSocket socket in ship.SocketMap)
					{
						if (futureResources == 0) break;

						if (MAGAZINE_COMPONENTS.Contains(socket.ComponentName))
						{
							futureResources = RestockMagazineMissiles(socket, XML.MissileTypes, futureResources);
						}

						if (Location.SubType == Location.LocationSubType.StationSupplyDepot)
						{
							if (LAUNCHER_COMPONENTS.Contains(socket.ComponentName))
							{
								futureResources = RestockLauncher(socket, XML.MissileTypes, futureResources);
							}
						}
					}
				}

				int spentResources = availableResources - futureResources;

				resourceMetals.Stockpile -= spentResources;
				resourceParts.Stockpile -= spentResources;
				resourceFuel.Stockpile -= spentResources;
			}
		}

		private static int RestockDCLocker(SerializedHullSocket socket, int availableRestores)
        {
            int totalRestores = Helper.cRegistry.Components.Find(x => x.Name == socket.ComponentName).Restores;
            if (socket.ComponentState != null && socket.ComponentState is DCLockerComponent.DCLockerState)
            {
                DCLockerComponent.DCLockerState locker = (DCLockerComponent.DCLockerState)socket.ComponentState;
                int restocked = Math.Min(availableRestores, (int)locker.RestoresConsumed);
                locker.RestoresConsumed -= (uint)restocked;
				availableRestores -= restocked;
            }

            return availableRestores;
        }

		private static int RestockMagazineAmmo(SerializedHullSocket socket, int availableMetals)
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

					Munition munitionEntry = Helper.mRegistry.Munitions.Find(x => x.Name == munitionLoad.MunitionKey);

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
								int totalBatches = (int)Math.Floor((double) paidPrice / munitionEntry.PointCost);
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

		private static int RestockMagazineMissiles(SerializedHullSocket socket, List<SerializedMissileTemplate> missileTypes, int availableResources)
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
								int numRestocked = restockPrice / missileType.Cost;

								munitionState.Expended -= (uint)numRestocked;
								magazineState.Mags[index] = munitionState;
								availableResources -= restockPrice;
							}

							break;
						}
					}
				}
			}

			return availableResources;
		}

		private static int RestockLauncher(SerializedHullSocket socket, List<SerializedMissileTemplate> missileTypes, int availableResources)
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
									int numRestocked = restockPrice / missileType.Cost;

									missileState.Expended -= (uint)numRestocked;
									launcherState.Missiles[index] = missileState;
									availableResources -= restockPrice;
								}

								break;
							}
						}
					}
                    else
					{
						Munition munitionEntry = Helper.mRegistry.Munitions.Find(x => x.Name == missileLoad.MunitionKey);

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

		public void SpawnAtLocation(Location loc)
		{
			this.Location = loc;
		}
	}
}
