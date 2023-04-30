using Ships;
using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	[XmlType("Component")]
	[Serializable]
	public class Component
	{
		public string Name;
		public float MaxHP;
		public float MinHP;
		public int Crew;
		public int Restores;
		public Priority Priority;
	}
}