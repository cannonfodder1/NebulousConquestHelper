package nebulous.conquest.data;

public class Design implements Saveable {
    private String designID;
    private String hullType;
    private int pointCost;

    public String getDesignID() {
        return designID;
    }

    public String getHullType() {
        return hullType;
    }

    public int getPointCost() {
        return pointCost;
    }

    @Override
    public String save() {
        return String.format("""
{
    "designID" : "%s",
    "hullType" : "%s",
    "pointCost" : %d
}
                """,
                designID,
                hullType,
                pointCost
        ).stripTrailing();
    }
}
