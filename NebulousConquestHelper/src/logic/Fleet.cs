using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    [XmlType("Fleet")]
    [Serializable]
    public class Fleet
	{
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

		public string FleetFileName;
        public string LocationName;
		public int Restores;
		public int Fuel;
		public Game.ConquestTeam ControllingTeam;
		public FleetOrderType OrderType = FleetOrderType.None;
		public FleetOrderData OrderData = null;
		[XmlIgnore] public SerializedConquestFleet FleetXML;
		[XmlIgnore] public Location Location;

		public Fleet() { }

		public Fleet(string fleetFileName, string locationName, Game.ConquestTeam team)
        {
			FleetFileName = fleetFileName;
			LocationName = locationName;
			ControllingTeam = team;

			UpdateFromFile();

			UpdateRestoreCount();
		}

		public void UpdateFromFile()
		{
			FleetXML = (SerializedConquestFleet)Helper.ReadXMLFile(
				typeof(SerializedConquestFleet),
				new FilePath(Helper.DATA_FOLDER_PATH + FleetFileName)
			);
		}

		public void SaveFleet()
        {
			Helper.WriteXMLFile(typeof(SerializedConquestFleet), new FilePath(Helper.DATA_FOLDER_PATH + FleetFileName), FleetXML);
        }

		public void ProcessBattleResults(bool losingTeam = false)
		{
			UpdateFromFile();

			List<SerializedConquestShip> removeShips = new List<SerializedConquestShip>();

			foreach (SerializedConquestShip ship in FleetXML.Ships)
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
				FleetXML.Ships.Remove(deadShip);
			}

			UpdateRestoreCount();
		}

		public void UpdateRestoreCount()
		{
			int totalRestores = 0;

			foreach (SerializedConquestShip ship in FleetXML.Ships)
			{
				totalRestores += GetShipRestores(ship);
			}

			Restores = totalRestores;
		}

		public int GetRestoreCapacity()
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

			int totalRestores = 0;

			foreach (SerializedConquestShip ship in FleetXML.Ships)
			{
				foreach (SerializedHullSocket socket in ship.SocketMap)
				{
					if (DC_LOCKER_COMPONENTS.Contains(socket.ComponentName))
					{
						totalRestores += Helper.Registry.Components.Find(x => x.Name == socket.ComponentName).Restores;
					}
				}
			}

			return totalRestores;
		}

        public static bool IsShipMobile(SerializedConquestShip ship)
		{
			if (ship.SavedState == null) return true;

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

            foreach (SerializedHullSocket socket in ship.SocketMap)
            {
                if (MAIN_DRIVE_COMPONENTS.Contains(socket.ComponentName))
                {
                    foreach (SerializedPartDamage part in ship.SavedState.Damage.Parts)
                    {
                        if (part.Key == socket.Key)
                        {
                            Component component = Helper.Registry.Components.Find(x => x.Name == socket.ComponentName);
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
			if (DC_LOCKER_COMPONENTS.Count < 4)
			{
				string prefix = "Stock/";
				DC_LOCKER_COMPONENTS.Clear();
				DC_LOCKER_COMPONENTS.Add(prefix + "Rapid DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Small DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Large DC Locker");
				DC_LOCKER_COMPONENTS.Add(prefix + "Reinforced DC Locker");
			}

			int initialRestores = 0;
			int expendedRestores = 0;

			foreach (SerializedHullSocket socket in ship.SocketMap)
			{
				if (DC_LOCKER_COMPONENTS.Contains(socket.ComponentName))
				{
					initialRestores += Helper.Registry.Components.Find(x => x.Name == socket.ComponentName).Restores;
					if (socket.ComponentState != null && socket.ComponentState is DCLockerComponent.DCLockerState)
					{
						DCLockerComponent.DCLockerState locker = (DCLockerComponent.DCLockerState)socket.ComponentState;
						expendedRestores += (int)locker.RestoresConsumed;
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
			int total = 0;

			foreach (SerializedConquestShip ship in FleetXML.Ships)
            {
				switch (ship.HullType)
				{
					case "Stock/Sprinter Corvette":
						total += 3;
						break;
					case "Stock/Raines Frigate":
						total += 5;
						break;
					case "Stock/Keystone Destroyer":
						total += 8;
						break;
					case "Stock/Vauxhall Light Cruiser":
						total += 10;
						break;
					case "Stock/Axford Heavy Cruiser":
						total += 13;
						break;
					case "Stock/Solomon Battleship":
						total += 21;
						break;
					default:
						Console.WriteLine("ERROR! Unknown Hull Type: " + ship.HullType);
						total += ship.Cost / 100;
						break;
				}
            }

			return total;
        }
		/*
		public int GetFoodConsumption()
		{
			int total = 0;

			foreach (SerializedConquestShip ship in FleetXML.Ships)
			{
				foreach (SerializedHullSocket socket in ship.SocketMap)
				{
					total += Helper.Registry.Components.Find(x => x.Name == socket.ComponentName).Crew;
				}
			}

			return total;
		}
		*/
	}
}
