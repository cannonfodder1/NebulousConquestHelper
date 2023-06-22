using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	[XmlType("MunitionRegistry")]
	[Serializable]
	public class MunitionRegistry
	{
		public List<Munition> Munitions;
	}
}