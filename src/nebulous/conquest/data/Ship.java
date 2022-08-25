package nebulous.conquest.data;

import java.util.List;

public class Ship {
    private String shipID;
    private String designID;

    private Design design;

    public void loadDesign(List<Design> allDesigns) {
        for (Design thisDesign: allDesigns) {
            if (thisDesign.getDesignID().equals(designID)) {
                design = thisDesign;
                return;
            }
        }
    }

    public String getShipID() {
        return shipID;
    }

    public Design getDesign() {
        return design;
    }
}
