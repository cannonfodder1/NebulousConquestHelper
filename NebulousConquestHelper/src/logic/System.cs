using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("System")]
    [Serializable]
    public class System
    {
        public string Name;
        public List<Location> OrbitingLocations;
        public List<Belt> SurroundingBelts;
        [XmlIgnore] public List<Location> AllLocations;

        public Location FindLocationByName(string name)
        {
            return AllLocations.Find(x => x.Name == name);
        }

        public void InitSystem()
        {
            void InitSystemInternal(Location loc)
            {
                AllLocations.Add(loc);

                foreach (Location subloc in loc.OrbitingLocations)
                {
                    InitSystemInternal(subloc);
                }
                foreach (Location subloc in loc.LagrangeLocations)
                {
                    InitSystemInternal(subloc);
                }
            }

            AllLocations = new List<Location>();

            foreach (Location loc in OrbitingLocations)
            {
                InitSystemInternal(loc);
            }
        }
    }
}