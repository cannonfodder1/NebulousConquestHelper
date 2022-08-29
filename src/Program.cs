using Utility;
using static Ships.DCLockerComponent;

namespace NebulousConquestHelper
{
    class Program
	{
		static void Main(string[] args)
        {
			FilePath path = new FilePath("../../../src/data/Conquest - TF Oak.fleet");
			SerializedConquestFleet fleet = Helper.ReadFleetFile(path);
			fleet.Ships[0].SavedState.Damage.Parts[8].HP = 0;
			fleet.Ships[0].SavedState.Damage.Parts[8].Destroyed = true;
			DCLockerState locker = (DCLockerState)fleet.Ships[0].SocketMap[11].ComponentState;
			locker.RestoresConsumed = 2;
			fleet.ConquestInfo.CurrentLocation = "Goguen";
			Helper.WriteFleetFile(path, fleet);
        }
    }
}
