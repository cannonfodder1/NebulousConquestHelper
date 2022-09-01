using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("ComponentInfo")]
    [Serializable]
    public class ComponentInfo
    {
        public string Name;
        public float MaxHP;
        public float MinHP;
        public int Restores;
    }
}