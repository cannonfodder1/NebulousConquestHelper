using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("ConquestInfo")]
    [Serializable]
    public class SerializedConquestInfo
    {
        public string CurrentLocation;
    }
}
