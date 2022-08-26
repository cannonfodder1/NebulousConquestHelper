package nebulous.conquest.data;

import java.util.List;

public class Game {
    private int turn = 0;
    private Location[] locations;
    private Design[] designs;
    private Ship[] ships;
    private Fleet[] fleets;

    public List<Location> allLocations;
    public List<Design> allDesigns;
    public List<Ship> allShips;
    public List<Fleet> allFleets;

    private static Game instance;
    private Game() {
        instance = this;
    }
    public static Game getInstance() {
        if (instance != null) return instance; else return new Game();
    }

    public void loadGame() {
        allLocations = List.of(locations);
        allDesigns = List.of(designs);
        allShips = List.of(ships);
        allFleets = List.of(fleets);

        for (Design design: designs) {
            design.loadXML();
        }
        for (Ship ship: ships) {
            ship.loadXML();
        }
        for (Fleet fleet: fleets) {
            fleet.init();
        }
    }

    public String saveGame() {
        return String.format("""
{
    "turn" : "%s",
    "designs" : [
%s    ],
    "ships" : [
%s    ],
    "fleets" : [
%s    ],
    "locations" : [
%s    ]
}
                """,
                turn,
                saveList(allDesigns),
                saveList(allShips),
                saveList(allFleets),
                saveList(allLocations)
        );
    }

    private String saveList(List list) {
        String json = "";
        for (Object obj: list) {
            Saveable item = (Saveable) obj;
            if (item != null) {
                json += item.saveJSON() + ",\n";
            }
        }
        return json.substring(0, json.length()-2).indent(8);
    }

    public int getTurn() {
        return turn;
    }

    public void advanceTurn() {
        for (Location loc: allLocations) {
            loc.advanceTurn();
        }
        turn++;
    }
}
