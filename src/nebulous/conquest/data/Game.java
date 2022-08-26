package nebulous.conquest.data;

import java.util.Collections;
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

    public void loadGame() {
        allLocations = List.of(locations);
        allDesigns = List.of(designs);
        allShips = List.of(ships);
        allFleets = List.of(fleets);

        for (Ship ship: ships) {
            ship.loadDesign(allDesigns);
        }
        for (Fleet fleet: fleets) {
            fleet.loadShips(allShips);
            fleet.loadLocation(allLocations);
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
                json += item.save() + ",\n";
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
