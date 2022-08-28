package nebulous.conquest.logic;

import nebulous.conquest.logic.SerializedFleet.Ships;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SavedState.Damage.Parts.SerializedPartDamage;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SocketMap.HullSocket;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SocketMap.HullSocket.ComponentData.Load;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SocketMap.HullSocket.ComponentData.MissileLoad.MagSaveData;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SocketMap.HullSocket.ComponentState;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SocketMap.HullSocket.ComponentState.Mags;
import nebulous.conquest.logic.SerializedFleet.Ships.Ship.SocketMap.HullSocket.ComponentState.Missiles;

import javax.xml.bind.*;
import java.io.File;
import java.io.FileReader;
import java.util.ArrayList;
import java.util.List;

import static nebulous.conquest.logic.SerializedFleet.Ships.Ship.*;

public class Fleet implements Saveable {
    private static final String SERIALIZED_FILE_TYPE = ".fleet";
    private static final String SERIALIZED_FOLDER_PATH = Helper.DATA_FOLDER_PATH + "fleets/";

    private String serializedFileName; // filled by JSON
    private SerializedFleet serialObj;

    private List<String> shipNames; // filled by JSON
    private List<Ship> ships;

    private String currentLocationID; // filled by JSON
    private Location currentLocation;

    public void init() {
        ships = new ArrayList<>();
        for (Ships.Ship serializedShip: serialObj.ships.ship) {
            for (Ship ship: Game.getInstance().allShips) {
                if (serializedShip.name.equals(ship.getName())) {
                    ships.add(ship);
                    ship.setSerialObj(serializedShip);
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

    public void loadXML() {
        try
        {
            JAXBContext jaxbContext = JAXBContext.newInstance(SerializedFleet.class, ObjectFactory.class);
            Unmarshaller jaxbParser = jaxbContext.createUnmarshaller();
            serialObj = (SerializedFleet) jaxbParser.unmarshal(
                    new FileReader(SERIALIZED_FOLDER_PATH + serializedFileName + SERIALIZED_FILE_TYPE)
            );
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void saveXML() {
        String path = SERIALIZED_FOLDER_PATH + serializedFileName + SERIALIZED_FILE_TYPE;
        try
        {
            JAXBContext jaxbContext = JAXBContext.newInstance(SerializedFleet.class, ObjectFactory.class);
            Marshaller jaxbMarshaller = jaxbContext.createMarshaller();
            jaxbMarshaller.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.TRUE);

            File file = new File(path);
            jaxbMarshaller.marshal(serialObj, file);

        } catch (JAXBException e) {
            e.printStackTrace();
        }

        // TODO: missile templates also have custom xsi types that need to be re-added
        try {
            String xml = Helper.readFileAsString(path);
            String[] chunks = xml.split("<ComponentData>");
            xml = chunks[0];
            for (int i = 1; i < chunks.length; i++) {
                String chunk = chunks[i];
                if (chunk.contains("<ConfiguredSize>")) {
                    xml = xml + "<ComponentData xsi:type=\"ResizableCellLauncherData\">" + chunk;
                } else if (chunk.contains("<MissileLoad>")) {
                    xml = xml + "<ComponentData xsi:type=\"CellLauncherData\">" + chunk;
                } else if (chunk.contains("<Load>")) {
                    xml = xml + "<ComponentData xsi:type=\"BulkMagazineData\">" + chunk;
                } else {
                    xml = xml + "<ComponentData>" + chunk;
                }
            }
            xml = xml.replace("<Fleet>", "<Fleet xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            Helper.writeStringToFile(path, xml);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public String getName() {
        return serializedFileName;
    }

    public Ship getShip(int index) {
        return ships.get(index);
    }

    public Location getCurrentLocation() {
        return currentLocation;
    }

    public int getPointCost() {
        return serialObj.getTotalPoints();
    }

    public void convertStateToSave() {
        boolean converted = true;
        for (Ships.Ship ship: serialObj.ships.ship) {
            if (ship.savedState != null) {
                converted = false;
                break;
            }
        }
        if (converted) return;

        // TODO: weapon groups are getting deleted during this process or during unmarshalling
        for (Ships.Ship ship: serialObj.ships.ship) {
            for (HullSocket socket: ship.socketMap.hullSocket) {
                for (JAXBElement<?> unknown: socket.componentState.nextDebuffIDOrCycleActiveOrCycleTimeElapsed) {
                    if (unknown.getDeclaredType() == Missiles.class) {
                        Missiles launcher = (Missiles) unknown.getValue();
                        if (launcher != null) {
                            for (Missiles.MagStateData magState : launcher.magStateData) {
                                if (magState.expended > 0) {
                                    for (MagSaveData magSave : socket.componentData.missileLoad.magSaveData) {
                                        if (magState.magazineKey.equals(magSave.magazineKey)) {
                                            magSave.quantity -= magState.expended;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (unknown.getDeclaredType() == Mags.class) {
                        Mags magazine = (Mags) unknown.getValue();
                        if (magazine != null) {
                            for (Mags.MagStateData magState : magazine.magStateData) {
                                if (magState.expended > 0) {
                                    for (Load.MagSaveData magSave : socket.componentData.load.magSaveData) {
                                        if (magState.magazineKey.equals(magSave.magazineKey)) {
                                            magSave.quantity -= magState.expended;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                socket.componentState = null;
            }
            int numPartsDestroyed = 0;
            for (SerializedPartDamage partDamage: ship.savedState.damage.parts.serializedPartDamage) {
                if (partDamage.destroyed.equals("true")) {
                    numPartsDestroyed++;
                }
            }
            for (Ship codeShip: ships) {
                if (codeShip.getName().equals(ship.name)) {
                    codeShip.setNumPartsDestroyed(numPartsDestroyed);
                    break;
                }
            }
            ship.savedState = null;
        }
    }

    @Override
    public String saveJSON() {
        return String.format("""
{
    "serializedFileName" : "%s",
    "currentLocationID" : "%s"
}
                """,
                serializedFileName,
                currentLocation.getLocationID()
        ).stripTrailing();
    }
}
