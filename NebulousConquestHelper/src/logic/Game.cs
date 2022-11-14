using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    [XmlType("ConquestGame")]
    [Serializable]
    public class Game
    {
        private const double AU_PER_DAY = 0.2;
        private const double AU_PER_DAY_THRU_BELT = 0.1;
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

        public struct ConquestTurnData
        {
            public List<string> responseFleets;
            public List<List<string>> arrivingSoon;
            public List<ConquestMovingFleet> arrivingLater;
        }

        public string ScenarioName;
        public List<Team> Teams;
        public List<Fleet> Fleets;
        public System System;
        public int DaysPassed = 0;
        public int WarProgress = 50;
        public ConquestTurnData TurnData = new ConquestTurnData();
        public List<string> BattleLocations = new List<string>();

        public static bool Init(object loaded)
        {
            Game game = (Game)loaded;

            if (game == null) return false;

            game.System.InitSystem();
            foreach (Location loc in game.System.AllLocations)
            {
                loc.PresentFleets = new List<Fleet>();
            }

            foreach (Fleet fleet in game.Fleets)
            {
                fleet.FleetXML = (SerializedConquestFleet)Helper.ReadXMLFile(
                    typeof(SerializedConquestFleet),
                    new FilePath(Helper.DATA_FOLDER_PATH + fleet.FleetFileName + Helper.FLEET_FILE_TYPE)
                );

                if (fleet.FleetXML == null) return false;

                fleet.Location = game.System.FindLocationByName(fleet.LocationName);

                if (fleet.Location == null) return false;

                fleet.Location.PresentFleets.Add(fleet);
            }

            game.TurnData.responseFleets = new List<string>();
            game.TurnData.arrivingLater = new List<ConquestMovingFleet>();
            game.TurnData.arrivingSoon = new List<List<string>>();
            for (int i = 0; i < 7; i++)
            {
                game.TurnData.arrivingSoon.Add(new List<string>());
            }
            if (game.TurnData.arrivingSoon.Count != 7) return false;

            return true;
        }

        public void CreateNewFleet(string fleetFileName, string locationName, ConquestTeam team)
        {
            Fleet newFleet = new Fleet(fleetFileName, locationName, team);

            newFleet.Location = System.FindLocationByName(locationName);
            newFleet.Location.PresentFleets.Add(newFleet);

            Fleets.Add(newFleet);
        }

        public void SaveGame(string fileName = "TestGame")
        {
            foreach (Fleet fleet in Fleets)
            {
                fleet.SaveFleet();
            }

            Helper.WriteXMLFile(typeof(Game), new FilePath(Helper.DATA_FOLDER_PATH + fileName + ".conquest"), this);
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
                        TurnData.responseFleets.Add(fleet.FleetXML.Name);
                    }
                    else if (fleet.OrderType == Fleet.FleetOrderType.Move)
                    {
                        Location depart = System.FindLocationByName(fleet.LocationName);
                        Location arrive = System.FindLocationByName(fleet.OrderData.MoveToLocation);
                        
                        double distance = depart.GetDistanceTo(arrive, DaysPassed);
                        double speed = AU_PER_DAY;

                        foreach (Belt belt in System.SurroundingBelts)
                        {
                            if (belt.TraversingAsteroidBelt(depart, arrive, DaysPassed))
                            {
                                speed = AU_PER_DAY_THRU_BELT;
                                break;
                            }
                        }

                        int travelTime = (int)(distance / speed);
                        if (distance % AU_PER_DAY != 0) travelTime++;
                        if (travelTime <= 7)
                        {
                            TurnData.arrivingSoon[travelTime-1].Add(fleet.FleetXML.Name);
                        }
                        else
                        {
                            Console.WriteLine("Fleet Ordered to Move - Arriving In " + travelTime + " Days");
                            TurnData.arrivingLater.Add(new ConquestMovingFleet(fleet.FleetXML.Name, travelTime));
                        }

                        fleet.OrderType = Fleet.FleetOrderType.InTransit;
                        fleet.Fuel -= fleet.GetFuelConsumption();
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
                Fleet fleet = Fleets.Find(x => x.FleetXML.Name == fleetName);
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
                    winnerShipsBefore += fleet.FleetXML.Ships.Count;
                    fleet.ProcessBattleResults(false);
                    winnerShipsAfter += fleet.FleetXML.Ships.Count;
                }
                else
                {
                    loserShipsBefore += fleet.FleetXML.Ships.Count;
                    fleet.ProcessBattleResults(true);
                    loserShipsAfter += fleet.FleetXML.Ships.Count;
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
