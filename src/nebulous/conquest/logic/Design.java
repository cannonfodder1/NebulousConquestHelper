package nebulous.conquest.logic;

public class Design extends SerializedWrapper implements Saveable {
    public Design() {
        SERIALIZED_FOLDER_PATH = Helper.STATE_FOLDER_PATH + "designs/";
    }

    public String getHullType() {
        return serializedObj.getHullType();
    }

    public int getPointCost() {
        return serializedObj.getCost();
    }

    @Override
    public String saveJSON() {
        return String.format("""
{
    "serializedFileName" : "%s"
}
                """,
                serializedFileName
        ).stripTrailing();
    }
}
