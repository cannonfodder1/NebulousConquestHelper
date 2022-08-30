using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("System")]
    [Serializable]
    public class SystemInfo
    {
        public string Name;
        public List<LocationInfo> OrbitingLocations;
        public List<BeltInfo> SurroundingBelts;
    }
}