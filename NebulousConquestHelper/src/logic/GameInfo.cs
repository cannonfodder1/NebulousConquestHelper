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

            void initLocation(LocationInfo loc)
            {
                loc.PresentFleets = new List<FleetInfo>();
            }
            game.System.ForeachLocation(initLocation);
            
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

            return true;
        }
    }
}
