using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("LocationInfo")]
    [Serializable]
    public class LocationInfo
    {
        public enum LocationType
        {
            Planet,
            Station
        }

        public enum LocationSubType
        {
            PlanetHabitable,
            PlanetBarren,
            PlanetGaseous,
            StationMining,
            StationFactory,
            StationCivilian,
            StationMinorRepair,
            StationMajorRepair
        }

        public string Name;
        public float OrbitalDistanceAU;
        public int OrbitalStartDegrees;
        public LocationType MainType;
        public LocationSubType SubType;
        public GameInfo.ConquestTeam ControllingTeam;
        public List<LocationInfo> OrbitingLocations;
        public List<BeltInfo> SurroundingBelts;
        [XmlIgnore] public List<FleetInfo> PresentFleets; 

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

        public PointF GetCoordinates(int daysFromStart)
        {
            double radians = GetCurrentDegrees(daysFromStart) * (Math.PI / 180);
            double planetX = Math.Sin(radians) * OrbitalDistanceAU;
            double planetY = Math.Cos(radians) * OrbitalDistanceAU * -1;
            return new PointF((float)planetX, (float)planetY);
        }

        public double GetDistanceTo(LocationInfo loc, int daysFromStart)
        {
            PointF loc1 = this.GetCoordinates(daysFromStart);
            PointF loc2 = loc.GetCoordinates(daysFromStart);

            double diffX = Math.Abs(loc1.X - loc2.X);
            double diffY = Math.Abs(loc1.Y - loc2.Y);

            return Math.Sqrt(Math.Pow(diffX, 2) + Math.Pow(diffY, 2));
        }
    }
}