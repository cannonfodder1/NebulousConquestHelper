using Munitions.ModularMissiles;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

// extracted from Nebulous.dll with Unity logging removed
namespace NebulousConquestHelper
{
    // Token: 0x02000219 RID: 537
    [XmlType("Fleet")]
	[Serializable]
	public class SerializedConquestFleet
	{
		private const int MAIN_DRIVE_REQUIRED_HP = 25;
		private static List<string> MAIN_DRIVE_COMPONENTS = new List<string>();

		// Token: 0x040009ED RID: 2541
		public string Name;

		// Token: 0x040009EE RID: 2542
		public int Version;

		// Token: 0x040009EF RID: 2543
		public int TotalPoints;

		// Token: 0x040009F0 RID: 2544
		public string FactionKey;

		// Token: 0x040009F1 RID: 2545
		public string Description;

		// Token: 0x040009F2 RID: 2546
		public ulong[] ModDependencies;

		// Token: 0x040009F3 RID: 2547
		public List<SerializedConquestShip> Ships = new List<SerializedConquestShip>();

		// Token: 0x040009F4 RID: 2548
		public List<SerializedMissileTemplate> MissileTypes = new List<SerializedMissileTemplate>();

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

			foreach (SerializedConquestShip ship in Ships)
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
										if (!part.Destroyed && part.HP >= MAIN_DRIVE_REQUIRED_HP)
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
				Ships.Remove(deadShip);
			}
		}
	}
}
