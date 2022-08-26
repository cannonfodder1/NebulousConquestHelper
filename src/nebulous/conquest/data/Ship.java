package nebulous.conquest.data;

public class Ship extends SerializedWrapper implements Saveable {
    public Ship() {
        SERIALIZED_FOLDER_PATH = Helper.STATE_FOLDER_PATH + "ships/";
    }

    private String designFileName;
    private Design design;

    @Override
    public void loadXML() {
        super.loadXML();
        for (Design thisDesign: Game.getInstance().allDesigns) {
            if (thisDesign.getFileName().equals(designFileName)) {
                design = thisDesign;
                return;
            }
        }
    }

    public String getFileName() {
        return serializedFileName;
    }

    public Design getDesign() {
        return design;
    }

    @Override
    public String saveJSON() {
        return String.format("""
{
    "serializedFileName" : "%s",
    "designFileName" : "%s"
}
                """,
                serializedFileName,
                design.getFileName()
        ).stripTrailing();
    }
}
