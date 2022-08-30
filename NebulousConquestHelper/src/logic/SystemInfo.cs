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

        public LocationInfo FindLocationByName(string name)
        {
            LocationInfo FindLocationByNameInternal(string name, LocationInfo location)
            {
                if (location.Name == name)
                {
                    return location;
                }

                foreach (LocationInfo loc in location.OrbitingLocations)
                {
                    if (FindLocationByNameInternal(name, loc) != null)
                    {
                        return loc;
                    }
                }

                return null;
            }

            foreach (LocationInfo loc in OrbitingLocations)
            {
                if (FindLocationByNameInternal(name, loc) != null)
                {
                    return loc;
                }
            }

            return null;
        }

        public void ForeachLocation(Action<LocationInfo> action)
        {
            void ForeachLocationInternal(LocationInfo loc, Action<LocationInfo> action)
            {
                action(loc);
                foreach (LocationInfo subloc in loc.OrbitingLocations)
                {
                    ForeachLocationInternal(subloc, action);
                }
            }

            foreach (LocationInfo loc in OrbitingLocations)
            {
                ForeachLocationInternal(loc, action);
            }
        }
    }
}