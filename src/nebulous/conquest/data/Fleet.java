package nebulous.conquest.data;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class Fleet implements Saveable {
    private List<String> shipIDs;
    private String currentLocationID;

    private List<Ship> ships;
    private Location currentLocation;

    public Fleet() {
        ships = new ArrayList<>();
    }

    public void loadShips(List<Ship> allShips) {
        for (Ship ship: allShips) {
            if (shipIDs.contains(ship.getShipID())) {
                ships.add(ship);
                if (ships.size() == shipIDs.size()) {
                    return;
                }
            }
        }
    }

    public void loadLocation(List<Location> allLocations) {
        for (Location thisLocation: allLocations) {
            if (thisLocation.getLocationID().equals(currentLocationID)) {
                currentLocation = thisLocation;
                return;
            }
        }
    }

    public Location getCurrentLocation() {
        return currentLocation;
    }

    @Override
    public String save() {
        return String.format("""
{
    "shipIDs" : [
%s
    ],
    "currentLocationID" : "%s"
}
                """,
                saveShipIDs(),
                currentLocation.getLocationID()
        ).stripTrailing();
    }

    private String saveShipIDs() {
        String json = "";
        for (Ship ship: ships) {
            json += "\"" + ship.getShipID() + "\",\n";
        }
        return json.substring(0, json.length()-2).indent(8).stripTrailing();
    }
}
