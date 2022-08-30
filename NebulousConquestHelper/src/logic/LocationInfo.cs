using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("LocationInfo")]
    [Serializable]
    public class LocationInfo
    {
        public enum LocationType
        {
            PlanetHabitable,
            PlanetBarren,
            PlanetGaseous,
            StationMining,
            StationFoundry,
            StationCivilian,
            StationFleetHub,
            StationFleetDepot,
            StationFleetShipyard
        }

        public string Name;
        public float OrbitalDistanceAU;
        public int OrbitalStartDegrees;
        public LocationType Type;
        public List<LocationInfo> OrbitingLocations;
        public List<BeltInfo> SurroundingBelts;
        public List<FleetInfo> PresentFleets; 

        public int GetCurrentDegrees(int daysFromStart)
        {
            float perDay = 360 / GetOrbitalPeriodDays();
            float travelled = daysFromStart * perDay;
            return (int)((OrbitalStartDegrees + travelled) % 360);
        }

        public float GetOrbitalPeriodDays()
        {
            // P^2 = a^3
            double periodSquared = Math.Pow(OrbitalDistanceAU, 3);
            double years = Math.Sqrt(periodSquared);
            return (float)(years * 365);
        }
    }
}