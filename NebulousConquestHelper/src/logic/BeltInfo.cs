using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("BeltInfo")]
    [Serializable]
    public class BeltInfo : IComparable<BeltInfo>
    {
        public string Name;
        public float NearEdgeDistanceAU;
        public float FarEdgeDistanceAU;

        public int CompareTo(BeltInfo compareBelt)
        {
            if (compareBelt == null)
                return 1;
            else
                return compareBelt.NearEdgeDistanceAU.CompareTo(NearEdgeDistanceAU);
        }
    }
}