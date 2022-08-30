using System;
using System.Drawing;
using Utility;

namespace NebulousConquestHelper
{
    class Program
	{
		static void Main(string[] args)
        {
			FilePath path = new FilePath(Helper.DATA_FOLDER_PATH + "TestGame.conquest");
			GameInfo game = (GameInfo)Helper.ReadXMLFile(typeof(GameInfo), path, GameInfo.init);

			// test code below, feel free to remove

			foreach (LocationInfo loc in game.System.OrbitingLocations)
            {
				Console.WriteLine(loc.Name);
				Console.WriteLine(loc.OrbitalStartDegrees + " -> " + loc.GetCurrentDegrees(13 * 7));
            }
        }
    }
}
