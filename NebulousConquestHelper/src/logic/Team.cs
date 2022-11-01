using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("TeamInfo")]
    [Serializable]
    public class Team
    {
        public string ShortName;
        public string LongName;
        public int MaximumTransportCapacity = 500;
        public int OccupiedTransportCapacity = 0;
    }
}
