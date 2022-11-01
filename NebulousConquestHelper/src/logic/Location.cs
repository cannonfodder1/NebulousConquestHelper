using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("Location")]
    [Serializable]
    public class Location
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
        public string Code;
        public float OrbitalDistanceAU;
        public int OrbitalStartDegrees;
        public LocationType MainType;
        public LocationSubType SubType;
        public Game.ConquestTeam ControllingTeam;
        public List<Resource> Resources;
        public List<Location> OrbitingLocations;
        public List<Location> LagrangeLocations;
        public List<Belt> SurroundingBelts;
        [XmlIgnore] public List<Fleet> PresentFleets;

        public void AddLagrangeStation(int lagIndex, LocationSubType type)
        {
            Location loc = new Location();
            loc.SubType = type;

            loc.Name = Code + "-L" + lagIndex;
            loc.MainType = LocationType.Station;
            loc.OrbitalDistanceAU = OrbitalDistanceAU;

            if (lagIndex == 3)
            {
                loc.OrbitalStartDegrees = OrbitalStartDegrees + 180;
                if (loc.OrbitalStartDegrees > 360) loc.OrbitalStartDegrees -= 360;
            }
            else if (lagIndex == 4)
            {
                loc.OrbitalStartDegrees = OrbitalStartDegrees - 60;
                if (loc.OrbitalStartDegrees < 0) loc.OrbitalStartDegrees += 360;
            }
            else if(lagIndex == 5)
            {
                loc.OrbitalStartDegrees = OrbitalStartDegrees + 60;
                if (loc.OrbitalStartDegrees > 360) loc.OrbitalStartDegrees -= 360;
            }
            else
            {
                throw new Exception("The index '" + lagIndex + "' is not a supported Lagrange point");
            }

            LagrangeLocations.Add(loc);
        }

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

        public double GetDistanceTo(Location loc, int daysFromStart)
        {
            PointF loc1 = this.GetCoordinates(daysFromStart);
            PointF loc2 = loc.GetCoordinates(daysFromStart);

            double diffX = Math.Abs(loc1.X - loc2.X);
            double diffY = Math.Abs(loc1.Y - loc2.Y);

            return Math.Sqrt(Math.Pow(diffX, 2) + Math.Pow(diffY, 2));
        }

        public void AdvanceTurn()
        {
            float satisfaction = 1.0f;
            
            foreach (Resource res in Resources)
            {
                if (res.GetSatisfaction() < satisfaction)
                {
                    satisfaction = res.GetSatisfaction();
                }
            }

            foreach (Resource res in Resources)
            {
                res.Consume(satisfaction);
                res.Produce(satisfaction);
            }
        }
    }
}