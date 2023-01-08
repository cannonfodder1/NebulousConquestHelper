using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
    [XmlType("Team")]
	[Serializable]
	public class Team
	{
        private const float ITEMS_PER_FREIGHTER = 100;

		public string ShortName;
		public string LongName;
        // 1400 per team for essential production chain
        // 1300 per team for resupply and repairs
        // start with 2500 / 2700 needed
        public int Freighters;

        public int TransportCapacity { get => (int)(Freighters * ITEMS_PER_FREIGHTER); }
    }
}
