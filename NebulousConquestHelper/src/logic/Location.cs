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
			PlanetHabitable,        // 2x, Polymers+++ Fuel+ Metals+
			PlanetBarren,           // 2x, Rares+++ Polymers+ Metals+
			PlanetGaseous,          // 2x, Fuel+++ Rares+ Metals+
			StationMining,          // 4x, Metals+++
									// Total: Metals+18, Rares+8, Polymers+8, Fuel+8
			
			StationFactoryParts,    // 4x, Metals- Rares-- Polymers-- Parts++
			StationFactoryRestores, // 4x, Metals- Parts- Restores+
									// Total: Metals-8, Rares-8, Polymers-8, Parts+4, Restores+4
			
			StationSupplyDepot      // 6x,
									//	Repair Armour:		1 Metals per mass
									//	Repair Component:	1 Restores to min HP, 1 Parts to max HP
									//	Build Missile:		1 Metals and 1 Parts per point, 1 Fuel per missile
									//	Build Ammo:			1 Metals per point



			// Grand Total: Metals+10, Fuel+8, Parts+4, Restores+4
			// Great Guess: one + or - here means 100 units
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
		[XmlIgnore] public List<Fleet> PresentFleets = new List<Fleet>();

		[XmlIgnore]
		public List<Location> AllLocations
		{
			get
			{
				List<Location> topLocations = new List<Location>(this.OrbitingLocations);
				topLocations.AddRange(this.LagrangeLocations);

				List<Location> ret = new List<Location>(topLocations.Count);

				foreach (Location loc in topLocations)
				{
					ret.AddRange(loc.AllLocations);
				}

				ret.InsertRange(0, topLocations);
				return ret;
			}
		}

		public void AddLagrangeStation(int lagIndex, LocationSubType type)
		{
			Location loc = new Location();
			loc.SubType = type;

			loc.Name = Name.Substring(0, 3) + "-L" + lagIndex;
			loc.MainType = LocationType.Station;
			loc.OrbitalDistanceAU = OrbitalDistanceAU;
			loc.ControllingTeam = ControllingTeam;

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
			else if (lagIndex == 5)
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

		public string PrintResources()
		{
			string print = Name + " Resource Report" + "\n";

			foreach (Resource resource in Resources)
			{
				print += "  " + resource.Type.ToString() + ":\n";
				print += "    Pile = " + resource.Stockpile + "\n";
				if (resource.Production > 0) print += "    Prod = " + resource.Production + "\n";
				if (resource.Consumption > 0) print += "    Cons = " + resource.Consumption + "\n";
			}

			print = print.Substring(0, print.Length - 1);

			return print;
		}

		public void SpawnFleet(Fleet fleet)
		{
			fleet.SpawnAtLocation(this);
			this.PresentFleets.Add(fleet);
		}

		public void SpawnResource(Resource res)
		{
			this.Resources.Add(res);
		}

		public void SetupResourceProducer(ResourceType type, int amount)
		{
			this.SpawnResource(new Resource(type, amount, amount, 0, 0));
		}

		public void SetupResourceConsumer(ResourceType type, int amount)
		{
			this.SpawnResource(new Resource(type, 0, 0, amount, amount));
		}

		public void SetupResourceStockpiler(ResourceType type, int amount)
		{
			this.SpawnResource(new Resource(type, amount, 0, 0, amount));
		}
	}
}