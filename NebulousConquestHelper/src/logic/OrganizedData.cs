using Ships.Serialization;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
    public class OrganizedComponentData
    {
        public SerializedHullSocket socket;
        public SerializedPartDamage state;
        public Component entry;

        public OrganizedComponentData(ref SerializedHullSocket socket, ref SerializedPartDamage state, Component entry)
        {
            this.socket = socket;
            this.state = state;
            this.entry = entry;
        }
    }

    public class OrganizedShipData
    {
        public SerializedConquestShip shipXML;
        public List<OrganizedComponentData> components;

        public OrganizedShipData(ref SerializedConquestShip ship)
        {
            shipXML = ship;

            components = new List<OrganizedComponentData>();

            for (int i = 0; i < shipXML.SavedState.Damage.Parts.Count; i++)
            {
                SerializedPartDamage state = shipXML.SavedState.Damage.Parts[i];
                SerializedHullSocket socket = null;

                for (int k = 0; k < shipXML.SocketMap.Count; k++)
                {
                    if (state.Key == shipXML.SocketMap[k].Key)
                    {
                        socket = shipXML.SocketMap[k];
                        break;
                    }
                }

                Component entry = Helper.cRegistry.Get(socket != null ? socket.ComponentName : "");

                components.Add(new OrganizedComponentData(ref socket, ref state, entry));
            }
        }

        public void SortComponents()
        {
            components.Sort(CompareComponents);
        }

        private int CompareComponents(OrganizedComponentData a, OrganizedComponentData b)
        {
            return b.entry.Priority - a.entry.Priority;
        }
    }
}
