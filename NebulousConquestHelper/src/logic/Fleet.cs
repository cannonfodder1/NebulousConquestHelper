using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

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
			if (MAIN_DRIVE_COMPONENTS.Count < 20)
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
				MAIN_DRIVE_COMPONENTS.Add(prefix + "BW800 Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "BW800-R Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "BW1500 Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "BW1500-R Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "BW2000 Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "CHI-777 Yard Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "CHI-7700 Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "CHI-9100 Long Haul Drive");
				MAIN_DRIVE_COMPONENTS.Add(prefix + "Sundrive Racing Pro");
			}
		}

		private static void InitializeDamconComponents()
		{
			if (DC_LOCKER_COMPONENTS.Count < 5)
			{
				string prefix = "Stock/";
				DC_LOCKER_COMPONENTS.Clear();
				DC_LOCKER_COMPONENTS.Add(prefix + "Rapid DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Small DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Large DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Reinforced DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Large DC Storage");
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
				LAUNCHER_COMPONENTS.Add(prefix + "RL18 Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "RL36 Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "Container Stack Launcher");
				LAUNCHER_COMPONENTS.Add(prefix + "Container Bank Launcher");
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
					case "Stock/Shuttle Clipper":
						mass += 1;
						break;
					case "Stock/Tug Clipper":
						mass += 3;
						break;
					case "Stock/Bulk Clipper":
						mass += 5;
						break;
					case "Stock/Ocello Cruiser":
						mass += 12;
						break;
					case "Stock/Bulker Line Ship":
						mass += 15;
						break;
					case "Stock/Container Line Ship":
						mass += 15;
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
                            availableRestores = Helper.RestockDCLocker(socket, availableRestores);
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
							availableMetals = Helper.RestockMagazineAmmo(socket, availableMetals);
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
							futureResources = Helper.RestockMagazineMissiles(socket, XML.MissileTypes, futureResources);
						}

						if (Location.SubType == Location.LocationSubType.StationSupplyDepot)
						{
							if (LAUNCHER_COMPONENTS.Contains(socket.ComponentName))
							{
								futureResources = Helper.RestockLauncher(socket, XML.MissileTypes, futureResources);
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

		public void SpawnAtLocation(Location loc)
		{
			this.Location = loc;
		}
	}
}
