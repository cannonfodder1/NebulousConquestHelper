package nebulous.conquest.logic;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.nio.file.Files;
import java.nio.file.Paths;

public class Helper {
    public static final String DATA_FOLDER_PATH = "src/nebulous/conquest/data/";

    public static String readFileAsString(String path) throws Exception
    {
        return new String(Files.readAllBytes(Paths.get(path)));
    }

    public static void writeStringToFile(String path, String content) throws Exception
    {
        File file = new File(path);
        BufferedWriter writer = new BufferedWriter(new FileWriter(file));
        writer.write(content);
        writer.close();
    }
}
