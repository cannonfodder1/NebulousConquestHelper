using Game.Orders.Tasks;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utility;
using UnityEngine;

// extracted from Nebulous.dll with Unity logging removed
namespace NebulousConquestHelper
{
	// Token: 0x02000220 RID: 544
	[XmlType("ShipState")]
	[Serializable]
	public class SerializedConquestShipState
	{
		// Token: 0x04000A0E RID: 2574
		public Vector3 Position;

		// Token: 0x04000A12 RID: 2578
		public EliminationReason Eliminated;

		// Token: 0x04000A13 RID: 2579
		public bool Vaporized;

		// Token: 0x04000A14 RID: 2580
		public bool LaunchedLifeboats;

		// Token: 0x04000A15 RID: 2581
		public SerializedShipDamage Damage;

		// Token: 0x04000A16 RID: 2582
		public SerializedCrewState CrewState;

		// Token: 0x04000A17 RID: 2583
		public MovementSpeed Throttle;

		// Token: 0x04000A18 RID: 2584
		public MovementStyle MoveStyle;

		// Token: 0x04000A19 RID: 2585
		public FormationStyle FormStyle;

		// Token: 0x04000A1A RID: 2586
		public EmissionStatus EMCON;

		// Token: 0x04000A1B RID: 2587
		public bool BattleShort;

		// Token: 0x04000A1C RID: 2588
		public WeaponsControlStatus WeaponsControl;

		// Token: 0x04000A1D RID: 2589
		public PointDefenseController.SavedPDState PDState;

		// Token: 0x04000A1E RID: 2590
		public DamageControlDispatcher.SavedDCState DCState;

		// Token: 0x04000A23 RID: 2595
		public List<BulkSavedObjectState.ComponentState> BulkComponents;
	}
}