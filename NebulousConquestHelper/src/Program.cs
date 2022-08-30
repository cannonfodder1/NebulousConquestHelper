using System;
using Utility;

namespace NebulousConquestHelper
{
    class Program
	{
		static void Main(string[] args)
        {
			FilePath path = new FilePath("../../../src/data/TestGame.conquest");
			GameInfo game = (GameInfo)Helper.ReadXMLFile(typeof(GameInfo), path, GameInfo.init);

			Console.WriteLine(game.Fleets[0].Fleet.Name);
			Console.WriteLine(game.Fleets[0].Location.Name);
        }
    }
}
