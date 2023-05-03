﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Munitions.ModularMissiles;

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
			BATTLE,
			LOGISTICS_SATISFACTION,
			LOGISTICS_TRANSPORT,
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

		public void LoadAllFleets()
		{
			foreach (Fleet fleet in this.Fleets)
			{
				Location loc = this.System.FindLocationByName(fleet.LocationName);
				loc.SpawnFleet(fleet);
			}
		}

		public void SetupResources()
		{
			foreach (Location loc in this.System.AllLocations)
			{
				switch (loc.SubType)
				{
					case Location.LocationSubType.PlanetHabitable:
						loc.SetupResourceProducer(ResourceType.Polymers, 300);
						loc.SetupResourceProducer(ResourceType.Fuel, 100);
						loc.SetupResourceProducer(ResourceType.Metals, 100);
						break;
					case Location.LocationSubType.PlanetGaseous:
						loc.SetupResourceProducer(ResourceType.Fuel, 300);
						loc.SetupResourceProducer(ResourceType.Rares, 100);
						loc.SetupResourceProducer(ResourceType.Metals, 100);
						break;
					case Location.LocationSubType.PlanetBarren:
						loc.SetupResourceProducer(ResourceType.Rares, 300);
						loc.SetupResourceProducer(ResourceType.Polymers, 100);
						loc.SetupResourceProducer(ResourceType.Metals, 100);
						break;
					case Location.LocationSubType.StationMining:
						loc.SetupResourceProducer(ResourceType.Metals, 300);
						break;
					case Location.LocationSubType.StationFactoryParts:
						loc.SetupResourceProducer(ResourceType.Parts, 200);
						loc.SetupResourceConsumer(ResourceType.Polymers, 200);
						loc.SetupResourceConsumer(ResourceType.Rares, 200);
						loc.SetupResourceConsumer(ResourceType.Metals, 100);
						break;
					case Location.LocationSubType.StationFactoryRestores:
						loc.SetupResourceProducer(ResourceType.Restores, 100);
						loc.SetupResourceConsumer(ResourceType.Parts, 100);
						loc.SetupResourceConsumer(ResourceType.Metals, 100);
						break;
					case Location.LocationSubType.StationSupplyDepot:
						loc.SetupResourceStockpiler(ResourceType.Fuel, 400);
						loc.SetupResourceStockpiler(ResourceType.Metals, 400);
						loc.SetupResourceStockpiler(ResourceType.Parts, 100);
						loc.SetupResourceStockpiler(ResourceType.Restores, 100);
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

		public void SaveGame(string fileName)
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

			foreach (Team team in Teams)
			{
				if (!CanTeamSatisfyRequests(team))
				{
					error = ConquestTurnError.LOGISTICS_SATISFACTION;
					return false;
				}

				if (!CanTeamTransportRequests(team))
				{
					error = ConquestTurnError.LOGISTICS_TRANSPORT;
					return false;
				}
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
					else if (fleet.OrderType == Fleet.FleetOrderType.Repair)
					{
						Console.WriteLine(fleet.FileName + " Ordered to Repair at " + fleet.Location.Name);

						foreach (SerializedConquestShip ship in fleet.GetAllRepairableShips())
						{
							fleet.Location.ScheduleRepair(fleet.FileName, ship.Name);
							fleet.OrderType = Fleet.FleetOrderType.Repairing;

							int queuePosition = fleet.Location.RepairQueue.Count + fleet.Location.RepairsUnderway.Count;
							Console.WriteLine("    Queue Position " + queuePosition + " - " + ship.Name);
						}
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
							Console.WriteLine(fleet.FileName + " Ordered to Move - Arriving In " + travelTime + " Days");
							TurnData.arrivingLater.Add(new ConquestMovingFleet(fleet.XML.Name, travelTime));
						}

						fleet.OrderType = Fleet.FleetOrderType.Moving;
						// TODO prevent issuing new orders while fleet is moving
						// TODO prevent fleets accessing their current location while moving
						fleet.Fuel -= fuel;
					}
				}

				foreach (Team team in Teams)
				{
					GetResourceLocations(team, out Dictionary<ResourceType, List<Location>> available, out Dictionary<ResourceType, List<Location>> requested);

					foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
					{
						int index1 = 0;
						int index2 = 0;

						while (index1 < available[type].Count && index2 < requested[type].Count)
						{
							Resource provide_resource = available[type][index1].Resources.Find(x => x.Type == type);
							Resource request_resource = requested[type][index2].Resources.Find(x => x.Type == type);

							int provide_amount = provide_resource.Stockpile - provide_resource.Requested;
							int request_amount = request_resource.Requested - request_resource.Stockpile;

							int transfer_size = Math.Min(provide_amount, request_amount);

							provide_resource.Stockpile -= transfer_size;
							request_resource.Stockpile += transfer_size;

							if (provide_resource.Stockpile == 0)
							{
								index1++;
							}
							if (request_resource.Stockpile == request_resource.Requested)
							{
								index2++;
							}
						}
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

		private bool CanTeamSatisfyRequests(Team team)
		{
			GetResourceSituation(team, out Dictionary<ResourceType, int> available, out Dictionary<ResourceType, int> requested);

			foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
			{
				if (available[type] < requested[type])
				{
					Console.WriteLine(requested[type] + " " + type.ToString() + " requested but only " + available[type] + " available");
					return false;
				}
			}

			return true;
		}

		private bool CanTeamTransportRequests(Team team)
		{
			GetResourceSituation(team, out Dictionary<ResourceType, int> available, out Dictionary<ResourceType, int> requested);

			int numToTransport = 0;

			foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
			{
				numToTransport += requested[type];
			}

			Console.WriteLine("Team " + team.ShortName + " Transport Need/Ability: " + numToTransport + " < " + team.TransportCapacity);
			return numToTransport <= team.TransportCapacity;
		}

		private void GetResourceSituation(Team team, out Dictionary<ResourceType, int> available, out Dictionary<ResourceType, int> requested)
		{
			available = new Dictionary<ResourceType, int>();
			requested = new Dictionary<ResourceType, int>();

			foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
			{
				available[type] = 0;
				requested[type] = 0;
			}

			foreach (Location loc in this.System.AllLocations)
			{
				if (GetTeam(loc.ControllingTeam) == team)
				{
					foreach (Resource res in loc.Resources)
					{
						int Required = res.Requested;

						if (res.Type == ResourceType.Metals)
						{
							Required += loc.GetRepairMetalCost();
						}
						if (res.Type == ResourceType.Parts)
						{
							Required += loc.GetRepairPartsCost();
						}

						if (res.Stockpile > Required)
						{
							available[res.Type] += res.Stockpile - Required;
						}
						if (res.Stockpile < Required)
						{
							requested[res.Type] += Required - res.Stockpile;
						}
					}
				}
			}
		}

		private void GetResourceLocations(Team team, out Dictionary<ResourceType, List<Location>> available, out Dictionary<ResourceType, List<Location>> requested)
		{
			available = new Dictionary<ResourceType, List<Location>>();
			requested = new Dictionary<ResourceType, List<Location>>();

			foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
			{
				available[type] = new List<Location>();
				requested[type] = new List<Location>();
			}

			foreach (Location loc in this.System.AllLocations)
			{
				if (GetTeam(loc.ControllingTeam) == team)
				{
					foreach (Resource res in loc.Resources)
					{
						int Required = res.Requested;

						if (res.Type == ResourceType.Metals)
						{
							Required += loc.GetRepairMetalCost();
						}
						if (res.Type == ResourceType.Parts)
						{
							Required += loc.GetRepairPartsCost();
						}

						if (res.Stockpile > Required)
						{
							available[res.Type].Add(loc);
						}
						if (res.Stockpile < Required)
						{
							requested[res.Type].Add(loc);
						}
					}
				}
			}
		}

		public Fleet MergeIntoFleet(Fleet mergingFleet, Fleet acceptingFleet, string newFleetName = null)
		{
			if (mergingFleet.ControllingTeam != acceptingFleet.ControllingTeam)
			{
				Console.WriteLine("ERROR! Fleets cannot be merged because they are not on the same team");
				return null;
			}

			if (mergingFleet.Location != acceptingFleet.Location)
			{
				Console.WriteLine("ERROR! Fleets cannot be merged because they are not in the same location");
				return null;
			}

			if (mergingFleet.OrderType != acceptingFleet.OrderType)
			{
				Console.WriteLine("ERROR! Fleets cannot be merged because they do not have the same orders");
				return null;
			}

			if (mergingFleet.OrderType == Fleet.FleetOrderType.Moving || acceptingFleet.OrderType == Fleet.FleetOrderType.Moving)
			{
				Console.WriteLine("ERROR! Fleets cannot be merged because they are in transit between locations");
				return null;
			}

			acceptingFleet.Restores = acceptingFleet.Restores + mergingFleet.Restores;
			acceptingFleet.Fuel = acceptingFleet.Fuel + mergingFleet.Fuel;

			acceptingFleet.XML.Ships.AddRange(mergingFleet.XML.Ships);

			HashSet<SerializedMissileTemplate> uniqueMissileTypes = new HashSet<SerializedMissileTemplate>();
			uniqueMissileTypes.UnionWith(acceptingFleet.XML.MissileTypes);
			uniqueMissileTypes.UnionWith(mergingFleet.XML.MissileTypes);

			acceptingFleet.XML.MissileTypes.Clear();
			acceptingFleet.XML.MissileTypes.AddRange(uniqueMissileTypes);

			Fleets.Remove(mergingFleet);

			File.Delete(mergingFleet.BackingFile.Path.RelativePath);

			if (newFleetName != null)
			{
				File.Delete(acceptingFleet.BackingFile.Path.RelativePath);
				acceptingFleet.FileName = newFleetName;
				acceptingFleet.XML.Name = newFleetName;
				acceptingFleet.SaveFleet();
			}

			return acceptingFleet;
		}

		public Fleet SplitFromFleet(Fleet originalFleet, List<string> shipsToSplit, string newFleetName)
		{
			if (originalFleet.OrderType == Fleet.FleetOrderType.Moving || originalFleet.OrderType == Fleet.FleetOrderType.Repairing)
			{
				Console.WriteLine("ERROR! Fleet cannot be split because it is moving or repairing");
				return null;
			}

			string copyFilePath = originalFleet.BackingFile.Path.Directory + "\\" + newFleetName + ".fleet";

			if (File.Exists(copyFilePath))
			{
				Console.WriteLine("ERROR! Fleet cannot be split because a fleet already exists with the split-off name");
				return null;
			}

			File.Copy(originalFleet.BackingFile.Path.RelativePath, copyFilePath);
			Fleet splitFleet = CreateNewFleet(newFleetName, originalFleet.Location.Name, originalFleet.ControllingTeam);
			splitFleet.XML.Name = newFleetName;

			originalFleet.XML.Ships.RemoveAll(x => shipsToSplit.Contains(x.Name));
			splitFleet.XML.Ships.RemoveAll(x => !shipsToSplit.Contains(x.Name));

			// TODO maybe need to handle missiles, maybe not

			originalFleet.UpdateRestoreCount();
			splitFleet.UpdateRestoreCount();

			int fuelImbalance = originalFleet.GetFuelCapacity() - originalFleet.Fuel;
			if (fuelImbalance < 0)
			{
				splitFleet.Fuel = fuelImbalance * -1;
			}

			return splitFleet;
		}
	}
}
