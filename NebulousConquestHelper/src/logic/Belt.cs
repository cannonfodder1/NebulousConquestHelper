using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("Belt")]
    [Serializable]
    public class Belt : IComparable<Belt>
    {
        public string Name;
        public float NearEdgeDistanceAU;
        public float FarEdgeDistanceAU;

        public int CompareTo(Belt compareBelt)
        {
            if (compareBelt == null)
                return 1;
            else
                return compareBelt.NearEdgeDistanceAU.CompareTo(NearEdgeDistanceAU);
        }
    }
}