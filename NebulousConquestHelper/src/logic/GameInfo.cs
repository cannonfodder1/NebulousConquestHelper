using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    [XmlType("ConquestGame")]
    [Serializable]
    public class GameInfo
    {
        public List<FleetInfo> Fleets;
        public List<LocationInfo> Locations;

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

                fleet.Location = game.Locations.Find(loc => loc.Name == fleet.LocationName);

                if (fleet.Location == null) return false;
            }

            return true;
        }
    }
}
