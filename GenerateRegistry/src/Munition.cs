using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	[XmlType("Munition")]
	[Serializable]
	public class Munition
	{
		public string Key;
		public string Name;
		public int PointCost;
		public int PointDivision;
	}
}