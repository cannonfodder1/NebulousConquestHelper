package nebulous.conquest.logic;

import com.google.gson.Gson;
import nebulous.conquest.data.Design;
import nebulous.conquest.data.Fleet;
import nebulous.conquest.data.Location;
import nebulous.conquest.data.Ship;

import javax.imageio.ImageIO;
import java.awt.*;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.lang.reflect.Type;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.List;

public class Main {
    private static String STATE_FOLDER_PATH = "src/nebulous/conquest/state/";

    private static List<Location> allLocations;
    private static List<Design> allDesigns;
    private static List<Ship> allShips;
    private static List<Fleet> allFleets;

    public static void main(String[] args) throws Exception {
        loadSavedState();

        generateSystemMap();
    }

    private static void generateSystemMap() throws IOException {
        int width = 1024;
        int height = 1024;

        int pixelsPerAU = 128;
        int starSize = 64;

        BufferedImage bufferedImage = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
        Graphics2D g2d = bufferedImage.createGraphics();

        g2d.setColor(Color.black);
        g2d.fillRect(0, 0, width, height);

        g2d.setColor(Color.white);
        g2d.drawString("Community Conquest", 8, 16);

        g2d.setColor(Color.yellow);
        g2d.fillOval((width - starSize) / 2, (height - starSize) / 2, starSize, starSize);

        g2d.setColor(Color.white);
        g2d.drawString("Bethel", (width + starSize) / 2, (height - starSize) / 2);

        for (Location loc: allLocations) {
            int size = 32;
            int dist = loc.getOrbitalDistance() * pixelsPerAU;
            double radians = loc.getOrbitalDegrees() * (Math.PI / 180);

            g2d.setColor(Color.white);
            g2d.drawOval((width / 2 - dist), (height / 2 - dist), dist * 2, dist * 2);

            int xOffset = (int) (Math.sin(radians) * dist) + (width / 2);
            int yOffset = (int) (Math.cos(radians) * dist) * -1 + (height / 2);

            g2d.setColor(Color.green);
            g2d.fillOval(xOffset - size / 2, yOffset - size / 2, size, size);

            g2d.setColor(Color.white);
            g2d.drawString(loc.getLocationID(), xOffset + size / 2, yOffset - size / 2);
        }

        g2d.dispose();
        File file = new File("conquestmap.png");
        ImageIO.write(bufferedImage, "png", file);
    }

    private static void loadSavedState() throws Exception {
        allLocations = Arrays.asList((Location[]) getStateFromJson("locations", Location[].class));
        allDesigns = Arrays.asList((Design[]) getStateFromJson("designs", Design[].class));
        allShips = Arrays.asList((Ship[]) getStateFromJson("ships", Ship[].class));
        allFleets = Arrays.asList((Fleet[]) getStateFromJson("fleets", Fleet[].class));

        for (Ship ship: allShips) {
            ship.loadDesign(allDesigns);
        }
        for (Fleet fleet: allFleets) {
            fleet.loadShips(allShips);
            fleet.loadLocation(allLocations);
        }
    }

    private static Object[] getStateFromJson(String fileName, Type type) throws Exception {
        String filePath = STATE_FOLDER_PATH + fileName + ".json";
        String jsonSource = readFileAsString(filePath);
        return new Gson().fromJson(jsonSource, type);
    }

    private static String readFileAsString(String file) throws Exception
    {
        return new String(Files.readAllBytes(Paths.get(file)));
    }
}
