using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	[XmlType("ComponentRegistry")]
	[Serializable]
	public class ComponentRegistry
	{
		public List<Component> Components;
	}
}