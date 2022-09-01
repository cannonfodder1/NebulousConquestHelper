using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

// extracted from Nebulous.dll with Unity logging removed
namespace NebulousConquestHelper
{
	// Token: 0x0200021D RID: 541
	[XmlType("Ship")]
	[Serializable]
	public class SerializedConquestShip
	{
		// Token: 0x06000F13 RID: 3859 RVA: 0x00039A6C File Offset: 0x00037C6C
		public string ComputeHash()
		{
			SerializedConquestShip copy = (SerializedConquestShip)MemberwiseClone();
			copy.SaveID = null;
			copy.Key = Guid.Empty;
			copy.Name = null;
			copy.Cost = 0;
			copy.Callsign = null;
			copy.Number = 0;
			copy.SymbolOption = 0;
			copy.InitialFormation = null;
			copy.SavedState = null;
			BinaryFormatter bf = new BinaryFormatter();
			string result;
			using (MemoryStream stream = new MemoryStream())
			{
				bf.Serialize(stream, copy);
				using (SHA256 hash = SHA256.Create())
				{
					byte[] hashBytes = hash.ComputeHash(stream.ToArray());
					StringBuilder builder = new StringBuilder();
					for (int i = 0; i < hashBytes.Length; i++)
					{
						builder.Append(hashBytes[i].ToString("x2"));
					}
					result = builder.ToString();
				}
			}
			return result;
		}

		// Token: 0x040009FE RID: 2558
		public uint? SaveID;

		// Token: 0x040009FF RID: 2559
		public Guid Key;

		// Token: 0x04000A00 RID: 2560
		public string Name;

		// Token: 0x04000A01 RID: 2561
		public int Cost;

		// Token: 0x04000A02 RID: 2562
		public string Callsign;

		// Token: 0x04000A03 RID: 2563
		public int Number;

		// Token: 0x04000A04 RID: 2564
		public int SymbolOption;

		// Token: 0x04000A05 RID: 2565
		public string HullType;

		// Token: 0x04000A06 RID: 2566
		public ulong[] ModDependencies;

		// Token: 0x04000A07 RID: 2567
		public List<SerializedHullSocket> SocketMap = new List<SerializedHullSocket>();

		// Token: 0x04000A08 RID: 2568
		public List<SerializedWeaponGroup> WeaponGroups = new List<SerializedWeaponGroup>();

		// Token: 0x04000A09 RID: 2569
		public SerializedFormation InitialFormation;

		// Token: 0x04000A0A RID: 2570
		public SerializedConquestShipState SavedState;
	}
}