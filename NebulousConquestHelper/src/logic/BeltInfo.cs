using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("BeltInfo")]
    [Serializable]
    public class BeltInfo
    {
        public string Name;
        public float NearEdgeDistanceAU;
        public float FarEdgeDistanceAU;
    }
}