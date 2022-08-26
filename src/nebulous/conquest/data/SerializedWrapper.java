package nebulous.conquest.data;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.Unmarshaller;
import java.io.FileReader;

public abstract class SerializedWrapper {
    protected String SERIALIZED_FOLDER_PATH;
    protected String serializedFileName;
    protected SerializedShip serializedObj;

    public String getFileName() {
        return serializedFileName;
    }

    public void loadXML() {
        try
        {
            JAXBContext jaxbContext = JAXBContext.newInstance(SerializedShip.class);
            Unmarshaller jaxbParser = jaxbContext.createUnmarshaller();
            serializedObj = (SerializedShip) jaxbParser.unmarshal(
                    new FileReader(SERIALIZED_FOLDER_PATH + serializedFileName + Helper.SERIALIZED_FILE_TYPE)
            );
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
