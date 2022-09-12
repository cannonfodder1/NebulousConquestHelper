using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    [XmlType("ConquestGame")]
    [Serializable]
    public class GameInfo
    {
        private const double AU_PER_DAY = 0.2;

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
            FLEET
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
        public List<TeamInfo> Teams;
        public List<FleetInfo> Fleets;
        public SystemInfo System;
        public int DaysPassed = 0;
        public ConquestTurnData TurnData = new ConquestTurnData();

        public static bool Init(object loaded)
        {
            GameInfo game = (GameInfo)loaded;

            if (game == null) return false;

            game.System.InitSystem();
            foreach (LocationInfo loc in game.System.AllLocations)
            {
                loc.PresentFleets = new List<FleetInfo>();
            }

            foreach (FleetInfo fleet in game.Fleets)
            {
                fleet.Fleet = (SerializedConquestFleet)Helper.ReadXMLFile(
                    typeof(SerializedConquestFleet),
                    new FilePath(Helper.DATA_FOLDER_PATH + fleet.FleetFileName + Helper.FLEET_FILE_TYPE)
                );

                if (fleet.Fleet == null) return false;

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

        public void CreateNewFleet(string fleetFileName, string locationName)
        {
            FleetInfo newFleet = new FleetInfo(fleetFileName, locationName);

            newFleet.Location = System.FindLocationByName(locationName);
            newFleet.Location.PresentFleets.Add(newFleet);

            Fleets.Add(newFleet);
        }

        public void SaveGame()
        {
            foreach (FleetInfo fleet in Fleets)
            {
                fleet.SaveFleet();
            }

            Helper.WriteXMLFile(typeof(GameInfo), new FilePath(Helper.DATA_FOLDER_PATH + "TestGame.conquest"), this);
        }

        public TeamInfo GetTeam(ConquestTeam team)
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
            foreach (FleetInfo fleet in Fleets)
            {
                if (!fleet.ReadyToAdvanceTurn())
                {
                    error = ConquestTurnError.FLEET;
                    return false;
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

                foreach (FleetInfo fleet in Fleets)
                {
                    if (fleet.OrderType == FleetInfo.FleetOrderType.Idle && fleet.OrderData.DefendNearby)
                    {
                        TurnData.responseFleets.Add(fleet.Fleet.Name);
                    }
                    else if (fleet.OrderType == FleetInfo.FleetOrderType.Move)
                    {
                        LocationInfo depart = System.FindLocationByName(fleet.LocationName);
                        LocationInfo arrive = System.FindLocationByName(fleet.OrderData.MoveToLocation);
                        double distance = depart.GetDistanceTo(arrive, DaysPassed);
                        int travelTime = (int)(distance / AU_PER_DAY);
                        if (distance % AU_PER_DAY != 0) travelTime++;
                        if (travelTime <= 7)
                        {
                            TurnData.arrivingSoon[travelTime-1].Add(fleet.Fleet.Name);
                        }
                        else
                        {
                            Console.WriteLine("Fleet Ordered to Move - Arriving In " + travelTime + " Days");
                            TurnData.arrivingLater.Add(new ConquestMovingFleet(fleet.Fleet.Name, travelTime));
                        }
                        fleet.OrderType = FleetInfo.FleetOrderType.InTransit;
                    }
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
                FleetInfo fleet = Fleets.Find(x => x.Fleet.Name == fleetName);
                Console.WriteLine(" - Fleet Arriving: " + fleetName);
                fleet.LocationName = fleet.OrderData.MoveToLocation;
                fleet.Location.PresentFleets.Remove(fleet);
                fleet.Location = System.FindLocationByName(fleet.LocationName);
                fleet.Location.PresentFleets.Add(fleet);
                fleet.OrderType = FleetInfo.FleetOrderType.None;
                fleet.OrderData = null;
            }
            TurnData.arrivingSoon[daysSinceTasking - 1].Clear();
            return true;
        }

        public bool Advance(out ConquestTurnError error)
        {
            error = ConquestTurnError.NONE;
            return GetPhase() == ConquestPhase.TaskingPhase ? AdvanceTurn(out error) : AdvanceDay();
        }
    }
}
