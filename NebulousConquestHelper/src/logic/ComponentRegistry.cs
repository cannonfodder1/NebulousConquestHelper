using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	[XmlType("ComponentRegistry")]
	[Serializable]
	public class ComponentRegistry : Backed<ComponentRegistry>
	{
		public List<Component> Components;

		public static Component defaultComponent
		{
			get
			{
				Component component = new Component();
				component.Name = "Unknown Inbuilt Part";
				component.Crew = 0;
				component.Restores = 0;
				component.MaxHP = 75;
				component.MinHP = 15;
				return component;
			}
		}

		public Component Get(string name)
        {
			Component searchResult = Components.Find(x => x.Name == name);

			if (searchResult != null)
            {
				return searchResult;
            }
            else
            {
				return defaultComponent;
            }
		}
	}
}
