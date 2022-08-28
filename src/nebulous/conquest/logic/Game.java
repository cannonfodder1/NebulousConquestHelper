package nebulous.conquest.logic;

import java.util.List;

public class Game {
    private int turn = 0; // filled by JSON
    private Location[] locations; // filled by JSON
    private Design[] designs; // filled by JSON
    private Ship[] ships; // filled by JSON
    private Fleet[] fleets; // filled by JSON

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
        for (Fleet fleet: fleets) {
            fleet.loadXML();
        }
        for (Ship ship: ships) {
            ship.init();
        }
        for (Fleet fleet: fleets) {
            fleet.init();
        }
    }

    public String saveGame() {
        for (Design design: designs) {
            design.saveXML();
        }
        for (Fleet fleet: fleets) {
            fleet.saveXML();
        }
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
