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
		private List<Location> _allLocations = null;
		[XmlIgnore]
		public List<Location> AllLocations
		{
			get
			{
				if (this._allLocations != null)
				{
					return this._allLocations;
				}

				this._allLocations = new List<Location>(this.OrbitingLocations);

				foreach (Location loc in this.OrbitingLocations)
				{
					this._allLocations.AddRange(loc.AllLocations);
				}

				return this._allLocations;
			}
		}

		public Location FindLocationByName(string name)
		{
			return AllLocations.Find(x => x.Name == name);
		}
	}
}