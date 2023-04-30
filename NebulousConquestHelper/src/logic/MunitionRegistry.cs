using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	[XmlType("MunitionRegistry")]
	[Serializable]
	public class MunitionRegistry : Backed<MunitionRegistry>
	{
		public List<Munition> Munitions;

		public Munition Get(string name)
		{
			return Munitions.Find(x => x.Name == name);
		}
	}
}