using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    [XmlType("FleetInfo")]
    [Serializable]
    public class FleetInfo
	{
		private static List<string> MAIN_DRIVE_COMPONENTS = new List<string>();
		private static List<string> DC_LOCKER_COMPONENTS = new List<string>();

		public string FleetFileName;
        public string LocationName;
		public int Restores;
		[XmlIgnore] public SerializedConquestFleet Fleet;
		[XmlIgnore] public LocationInfo Location;

		public void SaveFleet()
        {
			Helper.WriteXMLFile(typeof(SerializedConquestFleet), new FilePath(Helper.DATA_FOLDER_PATH + FleetFileName), Fleet);
        }

		public void ProcessBattleResults(bool losingTeam = false)
		{
			List<SerializedConquestShip> removeShips = new List<SerializedConquestShip>();

			foreach (SerializedConquestShip ship in Fleet.Ships)
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
				Fleet.Ships.Remove(deadShip);
			}

			UpdateRestoreCount();
		}

		public void UpdateRestoreCount()
		{
			int totalRestores = 0;

			foreach (SerializedConquestShip ship in Fleet.Ships)
			{
				totalRestores += GetShipRestores(ship);
			}

			Restores = totalRestores;
		}

		public static bool IsShipMobile(SerializedConquestShip ship)
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

            foreach (SerializedHullSocket socket in ship.SocketMap)
            {
                if (MAIN_DRIVE_COMPONENTS.Contains(socket.ComponentName))
                {
                    foreach (SerializedPartDamage part in ship.SavedState.Damage.Parts)
                    {
                        if (part.Key == socket.Key)
                        {
                            ComponentInfo component = Helper.registry.Components.Find(x => x.Name == socket.ComponentName);
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
					initialRestores += Helper.registry.Components.Find(x => x.Name == socket.ComponentName).Restores;
					if (socket.ComponentState is DCLockerComponent.DCLockerState)
					{
						DCLockerComponent.DCLockerState locker = (DCLockerComponent.DCLockerState)socket.ComponentState;
						expendedRestores += (int)locker.RestoresConsumed;
					}
				}
			}

			return initialRestores - expendedRestores;
        }
	}
}
