package nebulous.conquest.data;

import java.util.ArrayList;
import java.util.List;

public class Fleet {
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
}
