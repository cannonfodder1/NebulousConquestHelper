using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
	[XmlType("ConquestGame")]
	[Serializable]
	public class Game : Backed<Game>
	{
		private const double AU_PER_DAY = 0.2;
		private const double AU_PER_DAY_THRU_BELT = 0.1;
		private const double AEROBRAKE_FUEL_MULT = 0.5;
		private const int WP_PLANET_TAKEN = 6;
		private const int WP_STATION_TAKEN = 3;
		private const int WP_PER_SHIP = 1;
		private const int WP_PERFECT_MULT = 2;

		public enum ConquestTeam
		{
			GreenTeam,
			OrangeTeam
		}

		public enum ConquestPhase
		{
			TaskingPhase,
			ResolutionPhase
		}

		public enum ConquestTurnError
		{
			NONE,
			PHASE,
			FLEET_NEEDS_ORDER,
			FLEET_NEEDS_FUEL,
			BATTLE
		}

		public struct ConquestMovingFleet
		{
			public string fleetName;
			public int arrivalTime;

			public ConquestMovingFleet(string fleet, int time) : this()
			{
				this.fleetName = fleet;
				this.arrivalTime = time;
			}
		}

		public class ConquestTurnData
		{
			public List<string> responseFleets = new List<string>();
			public List<List<string>> arrivingSoon = new List<List<string>>();
			public List<ConquestMovingFleet> arrivingLater = new List<ConquestMovingFleet>();

			public ConquestTurnData()
			{
				for (int i = 0; i < 7; i++)
				{
					this.arrivingSoon.Add(new List<string>());
				}
			}
		}

		public string ScenarioName;
		public List<Team> Teams;
		public List<Fleet> Fleets;
		public System System;
		public int DaysPassed = 0;
		public int WarProgress = 50;
		public ConquestTurnData TurnData = new ConquestTurnData();
		public List<string> BattleLocations = new List<string>();

		private Game() { }

		public Game(BackingXmlFile<Game> backingFile)
		{
			this.BackingFile = backingFile;
		}

		public void SpawnFleets()
		{
			foreach (Fleet fleet in this.Fleets)
			{
				Location loc = this.System.FindLocationByName(fleet.LocationName);
				loc.SpawnFleet(fleet);
			}
		}

		public void SpawnResources()
		{
			foreach (Location loc in this.System.AllLocations)
			{
				switch (loc.SubType)
				{
					case Location.LocationSubType.PlanetHabitable:
						loc.SpawnResource(new Resource(ResourceType.Polymers, 0, 300, 0));
						loc.SpawnResource(new Resource(ResourceType.Fuel, 0, 100, 0));
						loc.SpawnResource(new Resource(ResourceType.Metals, 0, 100, 0));
						break;
					case Location.LocationSubType.PlanetGaseous:
						loc.SpawnResource(new Resource(ResourceType.Fuel, 0, 300, 0));
						loc.SpawnResource(new Resource(ResourceType.Rares, 0, 100, 0));
						loc.SpawnResource(new Resource(ResourceType.Metals, 0, 100, 0));
						break;
					case Location.LocationSubType.PlanetBarren:
						loc.SpawnResource(new Resource(ResourceType.Rares, 0, 300, 0));
						loc.SpawnResource(new Resource(ResourceType.Polymers, 0, 100, 0));
						loc.SpawnResource(new Resource(ResourceType.Metals, 0, 100, 0));
						break;
					case Location.LocationSubType.StationMining:
						loc.SpawnResource(new Resource(ResourceType.Metals, 0, 300, 0));
						break;
					case Location.LocationSubType.StationFactoryParts:
						loc.SpawnResource(new Resource(ResourceType.Parts, 0, 200, 0));
						loc.SpawnResource(new Resource(ResourceType.Metals, 0, 0, 300));
						loc.SpawnResource(new Resource(ResourceType.Polymers, 0, 0, 100));
						break;
					case Location.LocationSubType.StationFactoryRestores:
						loc.SpawnResource(new Resource(ResourceType.Restores, 0, 100, 0));
						loc.SpawnResource(new Resource(ResourceType.Parts, 0, 0, 100));
						loc.SpawnResource(new Resource(ResourceType.Rares, 0, 0, 200));
						loc.SpawnResource(new Resource(ResourceType.Polymers, 0, 0, 100));
						break;
					case Location.LocationSubType.StationSupplyDepot:
						loc.SpawnResource(new Resource(ResourceType.Fuel, 500, 0, 0));
						loc.SpawnResource(new Resource(ResourceType.Metals, 400, 0, 0));
						loc.SpawnResource(new Resource(ResourceType.Rares, 200, 0, 0));
						loc.SpawnResource(new Resource(ResourceType.Parts, 100, 0, 0));
						loc.SpawnResource(new Resource(ResourceType.Restores, 100, 0, 0));
						break;
				}
			}
		}

		public Fleet CreateNewFleet(string fleetFileName, string locationName, ConquestTeam team)
		{
			BackingXmlFile<SerializedConquestFleet> backingFile = Fleet.NewFile(fleetFileName);

			Fleet newFleet = new Fleet(backingFile, locationName, team);

			Location loc = System.FindLocationByName(locationName);
			loc.SpawnFleet(newFleet);

			Fleets.Add(newFleet);

			return newFleet;
		}

		public void SaveGame(string fileName = "TestGame")
		{
			foreach (Fleet fleet in Fleets)
			{
				fleet.SaveFleet();
			}

			this.FileName = fileName;
			this.BackingFile.Object = this;
		}

		public Team GetTeam(ConquestTeam team)
		{
			return team == ConquestTeam.GreenTeam ? Teams[0] : Teams[1];
		}

		public int GetTurn()
		{
			return DaysPassed / 7;
		}

		public ConquestPhase GetPhase()
		{
			return DaysPassed % 7 == 0 ? ConquestPhase.TaskingPhase : ConquestPhase.ResolutionPhase;
		}

		private bool ReadyToAdvanceTurn(out ConquestTurnError error)
		{
			if (GetPhase() != ConquestPhase.TaskingPhase)
			{
				error = ConquestTurnError.PHASE;
				return false;
			}

			if (BattleLocations.Count > 0)
			{
				error = ConquestTurnError.BATTLE;
				return false;
			}

			foreach (Fleet fleet in Fleets)
			{
				if (fleet.OrderType == Fleet.FleetOrderType.None || fleet.OrderData == null)
				{
					error = ConquestTurnError.FLEET_NEEDS_ORDER;
					return false;
				}

				if (fleet.OrderType == Fleet.FleetOrderType.Move)
				{
					if (fleet.Fuel < fleet.GetFuelConsumption())
					{
						error = ConquestTurnError.FLEET_NEEDS_FUEL;
						return false;
					}
				}
			}

			error = ConquestTurnError.NONE;
			return true;
		}

		private bool AdvanceTurn(out ConquestTurnError error)
		{
			if (ReadyToAdvanceTurn(out error))
			{
				TurnData.responseFleets.Clear();
				Console.WriteLine("Advancing Turn: " + GetTurn());
				Console.WriteLine("Fleets In Transit: " + TurnData.arrivingLater.Count);
				for (int i = 0; i < TurnData.arrivingLater.Count; i++)
				{
					ConquestMovingFleet mover = TurnData.arrivingLater[i];
					mover.arrivalTime -= 7;
					Console.WriteLine(" - Fleet Arriving In: " + mover.arrivalTime);
					if (mover.arrivalTime <= 7)
					{
						TurnData.arrivingSoon[mover.arrivalTime - 1].Add(mover.fleetName);
						TurnData.arrivingLater.RemoveAt(i);
						i--;
					}
					else
					{
						TurnData.arrivingLater[i] = mover;
					}
				}

				foreach (Fleet fleet in Fleets)
				{
					if (fleet.OrderType == Fleet.FleetOrderType.Idle && fleet.OrderData.DefendNearby)
					{
						TurnData.responseFleets.Add(fleet.XML.Name);
					}
					else if (fleet.OrderType == Fleet.FleetOrderType.Move)
					{
						Location depart = System.FindLocationByName(fleet.LocationName);
						Location arrive = System.FindLocationByName(fleet.OrderData.MoveToLocation);

						double distance = depart.GetDistanceTo(arrive, DaysPassed);
						double speed = AU_PER_DAY;
						int fuel = fleet.GetFuelConsumption();

						foreach (Belt belt in System.SurroundingBelts)
						{
							if (belt.TraversingAsteroidBelt(depart, arrive, DaysPassed))
							{
								speed = AU_PER_DAY_THRU_BELT;
								fuel = (int)(fuel * (AU_PER_DAY_THRU_BELT / AU_PER_DAY));
								break;
							}
						}

						if (arrive.MainType == Location.LocationType.Planet && arrive.ControllingTeam == fleet.ControllingTeam)
                        {
							fuel = (int)(fuel * AEROBRAKE_FUEL_MULT);
                        }

						int travelTime = (int)(distance / speed);
						if (distance % AU_PER_DAY != 0) travelTime++;
						if (travelTime <= 7)
						{
							TurnData.arrivingSoon[travelTime - 1].Add(fleet.XML.Name);
						}
						else
						{
							Console.WriteLine("Fleet Ordered to Move - Arriving In " + travelTime + " Days");
							TurnData.arrivingLater.Add(new ConquestMovingFleet(fleet.XML.Name, travelTime));
						}

						fleet.OrderType = Fleet.FleetOrderType.InTransit;
						fleet.Fuel -= fuel;
					}
				}

				foreach (Location loc in System.AllLocations)
				{
					loc.AdvanceTurn();
				}

				AdvanceDay();

				return true;
			}
			else
			{
				return false;
			}
		}

		private bool AdvanceDay()
		{
			DaysPassed += 1;
			int daysSinceTasking = DaysPassed % 7;
			if (daysSinceTasking == 0) daysSinceTasking = 7;
			Console.WriteLine("Advancing Day: " + daysSinceTasking);
			foreach (string fleetName in TurnData.arrivingSoon[daysSinceTasking - 1])
			{
				Fleet fleet = Fleets.Find(x => x.XML.Name == fleetName);
				Console.WriteLine(" - Fleet Arriving: " + fleetName);
				fleet.LocationName = fleet.OrderData.MoveToLocation;
				fleet.Location.PresentFleets.Remove(fleet);
				fleet.Location = System.FindLocationByName(fleet.LocationName);
				fleet.Location.PresentFleets.Add(fleet);
				fleet.OrderType = Fleet.FleetOrderType.None;
				fleet.OrderData = null;
				if (fleet.Location.PresentFleets.FindIndex(x => x.ControllingTeam != fleet.ControllingTeam) != -1 && !BattleLocations.Contains(fleet.LocationName))
				{
					BattleLocations.Add(fleet.LocationName);
				}
			}
			TurnData.arrivingSoon[daysSinceTasking - 1].Clear();
			return true;
		}

		public bool Advance(out ConquestTurnError error)
		{
			error = ConquestTurnError.NONE;
			return GetPhase() == ConquestPhase.TaskingPhase ? AdvanceTurn(out error) : AdvanceDay();
		}

		public void ResolveBattle(string location, ConquestTeam winner)
		{
			// should only be called after .fleet files are updated with generated post-battle states

			int winnerShipsBefore = 0;
			int winnerShipsAfter = 0;
			int loserShipsBefore = 0;
			int loserShipsAfter = 0;

			foreach (Fleet fleet in System.FindLocationByName(location).PresentFleets)
			{
				if (fleet.ControllingTeam == winner)
				{
					winnerShipsBefore += fleet.XML.Ships.Count;
					fleet.ProcessBattleResults(false);
					winnerShipsAfter += fleet.XML.Ships.Count;
				}
				else
				{
					loserShipsBefore += fleet.XML.Ships.Count;
					fleet.ProcessBattleResults(true);
					loserShipsAfter += fleet.XML.Ships.Count;
				}
			}

			int winnerShipsLost = winnerShipsBefore - winnerShipsAfter;
			int loserShipsLost = loserShipsBefore - loserShipsAfter;

			int reward = System.FindLocationByName(location).MainType == Location.LocationType.Planet ? WP_PLANET_TAKEN : WP_STATION_TAKEN;
			if (winnerShipsLost < 1) reward = reward * WP_PERFECT_MULT;
			reward = reward + (loserShipsLost * WP_PER_SHIP);
			reward = reward - (winnerShipsLost * WP_PER_SHIP);

			WarProgress = winner == ConquestTeam.GreenTeam ? WarProgress - reward : WarProgress + reward;
			BattleLocations.Remove(location);
		}
	}
}
