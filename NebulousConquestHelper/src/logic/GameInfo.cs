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
        public string ScenarioName;
        public List<FleetInfo> Fleets;
        public SystemInfo System;

        public static bool init(object loaded)
        {
            GameInfo game = (GameInfo)loaded;

            if (game == null) return false;

            foreach (FleetInfo fleet in game.Fleets)
            {
                fleet.Fleet = (SerializedConquestFleet)Helper.ReadXMLFile(
                    typeof(SerializedConquestFleet),
                    new FilePath(Helper.DATA_FOLDER_PATH + fleet.FleetFileName + Helper.FLEET_FILE_TYPE)
                );

                if (fleet.Fleet == null) return false;

                fleet.Location = game.FindLocationByName(fleet.LocationName);

                if (fleet.Location == null) return false;
            }

            return true;
        }

        public LocationInfo FindLocationByName(string name)
        {
            LocationInfo FindLocationByNameInternal(string name, LocationInfo location)
            {
                if (location.Name == name)
                {
                    return location;
                }

                foreach (LocationInfo loc in location.OrbitingLocations)
                {
                    if (FindLocationByNameInternal(name, loc) != null)
                    {
                        return loc;
                    }
                }

                return null;
            }

            foreach (LocationInfo loc in System.OrbitingLocations)
            {
                if (FindLocationByNameInternal(name, loc) != null)
                {
                    return loc;
                }
            }

            return null;
        }
    }
}
