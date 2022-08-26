package nebulous.conquest.data;

import java.io.File;
import java.nio.file.Files;
import java.nio.file.Paths;

public class Helper {
    public static final String STATE_FOLDER_PATH = "src/nebulous/conquest/state/";
    public static final String SERIALIZED_FILE_TYPE = ".ship";

    public static File readFile(String path) throws Exception
    {
        return new File(path);
    }
    public static String readFileAsString(String path) throws Exception
    {
        return new String(Files.readAllBytes(Paths.get(path)));
    }
}
