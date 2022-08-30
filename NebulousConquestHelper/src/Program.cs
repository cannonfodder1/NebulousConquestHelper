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

			// stuff happens here

			Helper.WriteFleetFile(path, fleet);
        }
    }
}
