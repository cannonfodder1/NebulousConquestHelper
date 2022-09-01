using Munitions.ModularMissiles;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

// extracted from Nebulous.dll with Unity logging removed
namespace NebulousConquestHelper
{
    // Token: 0x02000219 RID: 537
    [XmlType("Fleet")]
	[Serializable]
	public class SerializedConquestFleet
	{
		// Token: 0x040009ED RID: 2541
		public string Name;

		// Token: 0x040009EE RID: 2542
		public int Version;

		// Token: 0x040009EF RID: 2543
		public int TotalPoints;

		// Token: 0x040009F0 RID: 2544
		public string FactionKey;

		// Token: 0x040009F1 RID: 2545
		public string Description;

		// Token: 0x040009F2 RID: 2546
		public ulong[] ModDependencies;

		// Token: 0x040009F3 RID: 2547
		public List<SerializedConquestShip> Ships = new List<SerializedConquestShip>();

		// Token: 0x040009F4 RID: 2548
		public List<SerializedMissileTemplate> MissileTypes = new List<SerializedMissileTemplate>();
	}
}
