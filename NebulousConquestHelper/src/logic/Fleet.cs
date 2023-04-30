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
			[XmlEnum] Idle,
			[XmlEnum] Repair,
			[XmlEnum] Repairing,
			[XmlEnum] Move,
			[XmlEnum] Moving
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
						totalRestores += Helper.cRegistry.Get(socket.ComponentName).Restores;
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
							Component component = Helper.cRegistry.Get(socket.ComponentName);
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
					initialRestores += Helper.cRegistry.Get(socket.ComponentName).Restores;
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
			if (OrderType == FleetOrderType.Repairing)
			{
				foreach (SerializedConquestShip ship in XML.Ships)
				{
					if (CanShipBeRepaired(ship) && Location.RepairsUnderway.Exists(x => x.ShipName == ship.Name))
					{
						Console.WriteLine("    " + ship.Name + " will be removed from " + Location.Name + " repair berth on turn execution");
					}
				}
			}

			OrderType = FleetOrderType.Move;
			OrderData = new FleetOrderData(destination);
		}

		public void IssueIdleOrder(bool defend = false)
		{
			if (OrderType == FleetOrderType.Repairing)
			{
				foreach (SerializedConquestShip ship in XML.Ships)
				{
					if (CanShipBeRepaired(ship) && Location.RepairsUnderway.Exists(x => x.ShipName == ship.Name))
					{
						Console.WriteLine("    " + ship.Name + " will be removed from " + Location.Name + " repair berth on turn execution");
					}
				}
			}

			OrderType = FleetOrderType.Idle;
			OrderData = new FleetOrderData(defend);
		}

		public void IssueRepairOrder(bool autosplit = false)
		{
			if (Location.ControllingTeam != ControllingTeam)
			{
				Console.WriteLine("ERROR! Fleet attempting to repair at location of opposite team: " + Location.Name);
				return;
			}

			if (Location.SubType != Location.LocationSubType.StationSupplyDepot)
			{
				Console.WriteLine("ERROR! Fleet attempting to repair at location that is not supply depot: " + Location.Name);
				return;
			}

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				if (!CanShipBeRepaired(ship) && CanShipBeRestored(ship))
				{
					Console.WriteLine("    " + ship.Name + " has no partially damaged components and will not be repaired");
				}
				else if (CanShipBeRestored(ship))
				{
					Console.WriteLine("    " + ship.Name + " has destroyed components that will not be repaired");
				}
			}

			OrderType = FleetOrderType.Repair;
			OrderData = new FleetOrderData();
		}

		public int GetFuelConsumption()
		{
			int mass = 0;

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				mass += Helper.GetShipMass(ship);
			}

			return mass * FUEL_BURNED_PER_MASS;
		}

		public int GetFuelCapacity()
		{
			return GetFuelConsumption() * MAXIMUM_BURNS_OF_FUEL;
		}

		public void UseOnboardRestores(int restoresToUse)
        {
			InitializeDamconComponents();

			foreach (SerializedConquestShip ship in XML.Ships)
			{
				if (restoresToUse == 0) break;
				foreach (SerializedHullSocket socket in ship.SocketMap)
				{
					if (restoresToUse == 0) break;
					if (DC_LOCKER_COMPONENTS.Contains(socket.ComponentName))
					{
						restoresToUse = Helper.UseDCLockerRestores(socket, restoresToUse);
					}
				}
			}

			UpdateRestoreCount();
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
                            availableRestores = Helper.RestockDCLockerRestores(socket, availableRestores);
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

			// TODO figure out if non-modular missiles like chaff and container decoys should be ammo or missile prices

			if (restockMissiles
				&& Location.Resources.Exists(x => (x.Type == ResourceType.Metals) && (x.Stockpile > 0))
				&& Location.Resources.Exists(x => (x.Type == ResourceType.Parts) && (x.Stockpile > 0))
				&& Location.Resources.Exists(x => (x.Type == ResourceType.Fuel) && (x.Stockpile > 0))
				)
			{
				Resource resourceMetals = Location.Resources.Find(x => (x.Type == ResourceType.Metals) && (x.Stockpile > 0));
				Resource resourceParts = Location.Resources.Find(x => (x.Type == ResourceType.Parts) && (x.Stockpile > 0));
				Resource resourceFuel = Location.Resources.Find(x => (x.Type == ResourceType.Fuel) && (x.Stockpile > 0));

				int availableResources = Math.Min(resourceParts.Stockpile, resourceMetals.Stockpile);
				int futureResources = availableResources;

				int availableFuel = resourceFuel.Stockpile;

				InitializeMagazineComponents();
				InitializeLauncherComponents();

				foreach (SerializedConquestShip ship in XML.Ships)
				{
					if (futureResources == 0 || availableFuel == 0) break;

					foreach (SerializedHullSocket socket in ship.SocketMap)
					{
						if (futureResources == 0 || availableFuel == 0) break;

						if (MAGAZINE_COMPONENTS.Contains(socket.ComponentName))
						{
							Helper.RestockMagazineMissiles(socket, XML.MissileTypes, ref futureResources, ref availableFuel);
						}

						if (Location.SubType == Location.LocationSubType.StationSupplyDepot)
						{
							if (LAUNCHER_COMPONENTS.Contains(socket.ComponentName))
							{
								Helper.RestockLauncherMissiles(socket, XML.MissileTypes, ref futureResources, ref availableFuel);
							}
						}
					}
				}

				int spentResources = availableResources - futureResources;

				resourceMetals.Stockpile -= spentResources;
				resourceParts.Stockpile -= spentResources;

				resourceFuel.Stockpile = availableFuel;
			}
		}

		public SerializedConquestShip GetShip(string name)
        {
			return XML.Ships.Find(x => x.Name == name);
        }

		public int GetShipRepairCost(SerializedConquestShip ship, int maxRepairs = -1)
		{
			int parts = 0;

			OrganizedShipData shipState = new OrganizedShipData(ref ship);

			foreach (OrganizedComponentData component in shipState.components)
			{
				if (!component.state.Destroyed && component.state.HP < component.entry.MaxHP)
				{
					parts++;

					if (maxRepairs > 0 && parts >= maxRepairs) return maxRepairs;
				}
			}

			return parts;
		}

		public List<SerializedConquestShip> GetAllRepairableShips()
        {
			return XML.Ships.FindAll(x => CanShipBeRepaired(x));
		}

		public bool CanShipBeRepaired(SerializedConquestShip ship)
		{
			OrganizedShipData shipState = new OrganizedShipData(ref ship);

			foreach (OrganizedComponentData component in shipState.components)
			{
				if (!component.state.Destroyed && component.state.HP < component.entry.MaxHP)
				{
					return true;
				}
			}

			return false;
		}

		public bool CanShipBeRestored(SerializedConquestShip ship)
		{
			OrganizedShipData shipState = new OrganizedShipData(ref ship);

			foreach (OrganizedComponentData component in shipState.components)
			{
				if (component.state.Destroyed)
				{
					return true;
				}
			}

			return false;
		}

		public void PrintDamageReport()
		{
			Console.WriteLine(XML.Name + " Damage Report");

			for (int i = 0; i < XML.Ships.Count; i++)
			{
				int intact = 0;
				int damaged = 0;
				int destroyed = 0;

				SerializedConquestShip ship = XML.Ships[i];
				OrganizedShipData shipState = new OrganizedShipData(ref ship);

				foreach (OrganizedComponentData component in shipState.components)
				{
					if (component.state.Destroyed)
					{
						destroyed++;
					}
					else
					{
						if (component.state.HP < component.entry.MaxHP)
						{
							damaged++;
						}
						else
						{
							intact++;
						}
					}
				}

				string formatted = string.Format("{0:00}/{1:00}/{2:00}", intact, damaged, destroyed);
				Console.WriteLine("    " + formatted + " - " + ship.Name);
			}

			UpdateRestoreCount();
			Console.WriteLine("    " + "Restores Onboard Fleet: " + Restores);

			if (Location.Resources.Exists(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0)))
			{
				Resource resource = Location.Resources.Find(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0));
				Console.WriteLine("    " + "Restores At Location: " + resource.Stockpile);
			}
		}

		public void PatchAllShips()
		{
			foreach (SerializedConquestShip ship in XML.Ships)
			{
				PatchShip(ship.Name);
			}
		}

		public void PatchShip(string name)
		{
			SerializedConquestShip ship = GetShip(name);

			OrganizedShipData shipState = new OrganizedShipData(ref ship);

			int patched = 0;

			for (int i = 0; i < shipState.components.Count; i++)
			{
				OrganizedComponentData component = shipState.components[i];

				if (component.state.Destroyed) continue;

				float patchworkRepairedHP = component.entry.MinHP + (component.entry.MaxHP * 0.1f);

				if (component.state.HP < patchworkRepairedHP)
				{
					component.state.HP = patchworkRepairedHP;

					patched++;
				}
			}

			if (patched > 0)
            {
				// players don't need to know this, since it happens automatically and is free
				//Console.WriteLine(ship.Name + ": patched up " + patched + " components");
            }
		}

		public void RestoreAllShips(bool bLog = true)
		{
			foreach (SerializedConquestShip ship in XML.Ships)
			{
				RestoreShip(ship.Name, bLog);
			}
		}

		public void RestoreShip(string name, bool bLog = true)
		{
			// TODO parameter for priority, below which components won't be restored

			UpdateRestoreCount();
			int onboardRestores = Restores;
			int onstationRestores = 0;

			if (Location.Resources.Exists(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0)))
			{
				Resource resource = Location.Resources.Find(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0));
				onstationRestores += resource.Stockpile;
			}

			SerializedConquestShip ship = GetShip(name);

			OrganizedShipData shipState = new OrganizedShipData(ref ship);
			shipState.SortComponents();

			int locRestored = 0;
			int selfRestored = 0;

			if (bLog) Console.WriteLine(ship.Name + ":");

			for (int i = 0; i < shipState.components.Count; i++)
			{
				if (onboardRestores + onstationRestores == 0) break;

				OrganizedComponentData component = shipState.components[i];

				if (!component.state.Destroyed) continue;

				float patchworkRepairedHP = component.entry.MinHP + (component.entry.MaxHP * 0.1f);
				component.state.HP = patchworkRepairedHP;
				component.state.Destroyed = false;

				if (bLog) Console.WriteLine("    restored " + component.entry.Name);

				if (onstationRestores > 0)
                {
					locRestored++;
					onstationRestores--;
                }
				else
				{
					selfRestored++;
					onboardRestores--;
                }
			}

			if (locRestored + selfRestored > 0)
			{
				if (bLog) Console.WriteLine("    used " + locRestored + " on-station restores, leaving " + onstationRestores + " remaining");
				if (bLog) Console.WriteLine("    used " + selfRestored + " onboard restores, leaving " + onboardRestores + " remaining");
			}

			if (selfRestored > 0)
            {
				UseOnboardRestores(Restores - onboardRestores);
            }

			if (locRestored > 0)
			{
				Location.Resources.Find(x => (x.Type == ResourceType.Restores) && (x.Stockpile > 0)).Stockpile = onstationRestores;
			}
		}

		public void RepairShip(string name, int maxRepairs, bool bLog = true)
		{
			SerializedConquestShip ship = GetShip(name);

			OrganizedShipData shipState = new OrganizedShipData(ref ship);
			shipState.SortComponents();

			if (bLog) Console.WriteLine(ship.Name + ": ");

			for (int i = 0; i < shipState.components.Count; i++)
			{
				if (maxRepairs == 0) break;

				OrganizedComponentData component = shipState.components[i];

				if (component.state.Destroyed || component.state.HP >= component.entry.MaxHP) continue;

				component.state.HP = component.entry.MaxHP;

				if (bLog) Console.WriteLine("    repaired " + component.entry.Name);

				maxRepairs--;
			}
		}

		public void UnshredShip(string name, bool bLog = true)
		{
			SerializedConquestShip ship = GetShip(name);

			ship.SavedState.Damage.Armor = null;
		}

		public void SpawnAtLocation(Location loc)
		{
			this.Location = loc;
		}
    }
}
