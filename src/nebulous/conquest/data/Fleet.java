package nebulous.conquest.data;

import java.util.ArrayList;
import java.util.List;

public class Fleet implements Saveable {
    private List<String> shipFileNames;
    private String currentLocationID;

    private List<Ship> ships;
    private Location currentLocation;

    public Fleet() {
        ships = new ArrayList<>();
    }

    public void init() {
        for (Ship ship: Game.getInstance().allShips) {
            if (shipFileNames.contains(ship.getFileName())) {
                ships.add(ship);
                if (ships.size() == shipFileNames.size()) {
                    break;
                }
            }
        }
        for (Location thisLocation: Game.getInstance().allLocations) {
            if (thisLocation.getLocationID().equals(currentLocationID)) {
                currentLocation = thisLocation;
                break;
            }
        }
    }

    public Location getCurrentLocation() {
        return currentLocation;
    }

    @Override
    public String saveJSON() {
        return String.format("""
{
    "shipFileNames" : [
%s
    ],
    "currentLocationID" : "%s"
}
                """,
                saveShipFileNames(),
                currentLocation.getLocationID()
        ).stripTrailing();
    }

    private String saveShipFileNames() {
        String json = "";
        for (Ship ship: ships) {
            json += "\"" + ship.getFileName() + "\",\n";
        }
        return json.substring(0, json.length()-2).indent(8).stripTrailing();
    }
}
