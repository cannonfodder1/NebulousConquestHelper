package nebulous.conquest.logic;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;
import javax.xml.bind.Unmarshaller;
import java.io.File;
import java.io.FileReader;

public class Design implements Saveable {
    private static final String SERIALIZED_FILE_TYPE = ".ship";
    private static final String SERIALIZED_FOLDER_PATH = Helper.DATA_FOLDER_PATH + "designs/";
    private String serializedFileName; // filled by JSON
    private SerializedDesign serialObj;

    public void loadXML() {
        try
        {
            JAXBContext jaxbContext = JAXBContext.newInstance(SerializedDesign.class);
            Unmarshaller jaxbParser = jaxbContext.createUnmarshaller();
            serialObj = (SerializedDesign) jaxbParser.unmarshal(
                    new FileReader(SERIALIZED_FOLDER_PATH + serializedFileName + SERIALIZED_FILE_TYPE)
            );
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void saveXML() {
        try
        {
            JAXBContext jaxbContext = JAXBContext.newInstance(SerializedDesign.class);
            Marshaller jaxbMarshaller = jaxbContext.createMarshaller();
            jaxbMarshaller.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.TRUE);

            File file = new File(SERIALIZED_FOLDER_PATH + serializedFileName + SERIALIZED_FILE_TYPE);
            jaxbMarshaller.marshal(serialObj, file);

        } catch (JAXBException e) {
            e.printStackTrace();
        }
    }

    public String getName() {
        return serializedFileName;
    }

    public String getHullType() {
        return serialObj.getHullType();
    }

    public int getPointCost() {
        return serialObj.getCost();
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
