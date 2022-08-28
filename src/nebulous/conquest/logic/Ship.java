package nebulous.conquest.logic;

public class Ship implements Saveable {
    private String shipName; // filled by JSON
    private SerializedFleet.Ships.Ship serialObj;
    private String designFileName; // filled by JSON
    private Design design;
    private int numPartsDestroyed; // filled by JSON

    public void init() {
        for (Design thisDesign: Game.getInstance().allDesigns) {
            if (thisDesign.getName().equals(designFileName)) {
                design = thisDesign;
                return;
            }
        }
    }

    public String getName() {
        return shipName;
    }

    public Design getDesign() {
        return design;
    }

    public int getPointCost() {
        return serialObj.getCost();
    }

    public int getHullNumber() {
        return serialObj.getNumber();
    }

    public String getCallsign() {
        return serialObj.getCallsign();
    }

    @Override
    public String saveJSON() {
        return String.format("""
{
    "serializedFileName" : "%s",
    "designFileName" : "%s",
    "numPartsDestroyed" : %s
}
                """,
                shipName,
                design.getName(),
                numPartsDestroyed
        ).stripTrailing();
    }

    public void setSerialObj(SerializedFleet.Ships.Ship serializedShip) {
        serialObj = serializedShip;
    }

    public void setNumPartsDestroyed(int num) {
        numPartsDestroyed = num;
    }
}
