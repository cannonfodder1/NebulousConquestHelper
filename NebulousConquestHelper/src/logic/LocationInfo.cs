using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("LocationInfo")]
    [Serializable]
    public class LocationInfo
    {
        public string Name;
    }
}