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

		public string FleetFileName;
        public string LocationName;
        public SerializedConquestFleet Fleet;
        public LocationInfo Location;

		public void ProcessBattleResults(bool losingTeam = false)
		{
			List<SerializedConquestShip> removeShips = new List<SerializedConquestShip>();

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

			foreach (SerializedConquestShip ship in Fleet.Ships)
			{
				if (losingTeam)
				{
					if (ship.SavedState.Eliminated == EliminationReason.NotEliminated)
					{
						bool canEscape = false;
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
											canEscape = true;
											break;
										}
									}
								}
							}
							if (canEscape)
							{
								break;
							}
						}
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
		}
	}
}
