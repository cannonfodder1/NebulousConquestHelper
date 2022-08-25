package nebulous.conquest.data;

public class Location {
    private String locationID;
    private int orbitalDistance;
    private int orbitalDegrees;
    private int orbitalSpeed;

    public String getLocationID() {
        return locationID;
    }

    public int getOrbitalDistance() {
        return orbitalDistance;
    }

    public int getOrbitalDegrees() {
        return orbitalDegrees;
    }

    public int getOrbitalSpeed() {
        return orbitalSpeed;
    }

    public void ProgressOrbit() {
        orbitalDegrees += orbitalSpeed;
        if (orbitalDegrees >= 360) {
            orbitalDegrees -= 360;
        }
    }

    public void advanceTurn() {
        ProgressOrbit();
    }
}
