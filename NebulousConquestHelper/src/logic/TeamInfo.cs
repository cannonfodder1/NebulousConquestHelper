using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("TeamInfo")]
    [Serializable]
    public class TeamInfo
    {
        public string ShortName;
        public string LongName;
    }
}
