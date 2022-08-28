package nebulous.conquest.control;

import com.google.gson.Gson;
import nebulous.conquest.logic.Game;
import nebulous.conquest.logic.Helper;
import nebulous.conquest.logic.Location;

import javax.imageio.ImageIO;
import java.awt.*;
import java.awt.image.BufferedImage;
import java.io.*;
import java.lang.reflect.Type;

public class Main {
    private static Game game;

    public static void main(String[] args) throws Exception {
        loadGameState();
        game.allFleets.get(0).convertStateToSave();
        saveGameState();

//        String token = Helper.readFileAsString("../neb-bot-token.txt");
//        JDA jda = JDABuilder.createDefault(token).enableIntents(GatewayIntent.MESSAGE_CONTENT).build();
//        jda.addEventListener(new Listener());
    }

    public static void generateSystemMap() throws IOException {
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

        for (Location loc: game.allLocations) {
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

    private static void loadGameState() throws Exception {
        game = (Game) getStateFromJson();
        game.loadGame();
    }

    private static void saveGameState() throws Exception {
        String save = game.saveGame();
        Helper.writeStringToFile(Helper.DATA_FOLDER_PATH + "save.json", save);
    }

    private static Object getStateFromJson() throws Exception {
        String filePath = Helper.DATA_FOLDER_PATH + "gamestate.json";
        String jsonSource = Helper.readFileAsString(filePath);
        return new Gson().fromJson(jsonSource, Game.class);
    }
}
