using System;
using Utility;

namespace NebulousConquestHelper
{
    class Program
	{
		private Bot DiscordBot { get; set; }

		static void Main(string[] args)
		{
			// test code below, feel free to remove

			BackingXmlFile<ComponentRegistry> registryFile =
				BackingXmlFile<ComponentRegistry>.ComponentRegistry("ComponentRegistry");
			Helper.Registry = registryFile.Object;
			BackingXmlFile<Game> gameFile = BackingXmlFile<Game>.Game("TestGame");
			Game game = gameFile.Object;
			bool success = Game.Init(game);
			if (!success)
            {
				Console.WriteLine("Failed to initialize Game");
				return;
            }

			SetupSystemResources(game.System);

			game.CreateNewFleet("Conquest - TF Oak", "Sph-L4", Game.ConquestTeam.GreenTeam);

			Console.WriteLine("Fuel: " + game.Fleets[0].Fuel);
			Console.WriteLine("Restores: " + game.Fleets[0].Restores);

			game.Fleets[0].RestockFromLocation();
			game.Fleets[0].IssueMoveOrder("Sat-L3");

			Game.ConquestTurnError error;
			for (int i = 0; i < 14; i++)
			{
				game.Advance(out error);
				if (error != Game.ConquestTurnError.NONE)
				{
					Console.WriteLine(error);
					break;
				}
			}

			Console.WriteLine("Fuel: " + game.Fleets[0].Fuel);
			Console.WriteLine("Restores: " + game.Fleets[0].Restores);
			Console.WriteLine(game.System.FindLocationByName("Sph-L4").PrintResources());

			Mapping.CreateSystemMap("SystemMap_Overview.png", game.System, game.DaysPassed, false, false);
			Mapping.CreateSystemMap("SystemMap_Situation.png", game.System, game.DaysPassed, true, false);
			Mapping.CreateSystemMap("SystemMap_Logistics.png", game.System, game.DaysPassed, true, true);

			game.SaveGame();

			// test code above, feel free to remove

			Program program = new Program();
			program.DiscordBot = new Bot(game);
			program.RunBot();
        }

        private void RunBot()
        {
			DiscordBot.RunBotAsync().GetAwaiter().GetResult();
        }

		private static void SetupSystemResources(System system)
		{
			foreach (Location loc in system.AllLocations)
			{
				switch (loc.SubType)
				{
					case Location.LocationSubType.PlanetHabitable:
						loc.Resources.Add(new Resource(ResourceType.Polymers, 0, 300, 0));
						loc.Resources.Add(new Resource(ResourceType.Fuel, 0, 100, 0));
						loc.Resources.Add(new Resource(ResourceType.Metals, 0, 100, 0));
						break;
					case Location.LocationSubType.PlanetGaseous:
						loc.Resources.Add(new Resource(ResourceType.Fuel, 0, 300, 0));
						loc.Resources.Add(new Resource(ResourceType.Rares, 0, 100, 0));
						loc.Resources.Add(new Resource(ResourceType.Metals, 0, 100, 0));
						break;
					case Location.LocationSubType.PlanetBarren:
						loc.Resources.Add(new Resource(ResourceType.Rares, 0, 300, 0));
						loc.Resources.Add(new Resource(ResourceType.Polymers, 0, 100, 0));
						loc.Resources.Add(new Resource(ResourceType.Metals, 0, 100, 0));
						break;
					case Location.LocationSubType.StationMining:
						loc.Resources.Add(new Resource(ResourceType.Metals, 0, 300, 0));
						break;
					case Location.LocationSubType.StationFactoryParts:
						loc.Resources.Add(new Resource(ResourceType.Parts, 0, 200, 0));
						loc.Resources.Add(new Resource(ResourceType.Metals, 0, 0, 300));
						loc.Resources.Add(new Resource(ResourceType.Polymers, 0, 0, 100));
						break;
					case Location.LocationSubType.StationFactoryRestores:
						loc.Resources.Add(new Resource(ResourceType.Restores, 0, 100, 0));
						loc.Resources.Add(new Resource(ResourceType.Parts, 0, 0, 100));
						loc.Resources.Add(new Resource(ResourceType.Rares, 0, 0, 200));
						loc.Resources.Add(new Resource(ResourceType.Polymers, 0, 0, 100));
						break;
					case Location.LocationSubType.StationSupplyDepot:
						loc.Resources.Add(new Resource(ResourceType.Fuel, 500, 0, 0));
						loc.Resources.Add(new Resource(ResourceType.Metals, 400, 0, 0));
						loc.Resources.Add(new Resource(ResourceType.Rares, 200, 0, 0));
						loc.Resources.Add(new Resource(ResourceType.Parts, 100, 0, 0));
						loc.Resources.Add(new Resource(ResourceType.Restores, 100, 0, 0));
						break;
				}
			}
		}
	}
}
