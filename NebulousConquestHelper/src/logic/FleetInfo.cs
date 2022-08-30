using System;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    [XmlType("FleetInfo")]
    [Serializable]
    public class FleetInfo
    {
        public string FleetFileName;
        public string LocationName;
        public SerializedConquestFleet Fleet;
        public LocationInfo Location;
    }
}
