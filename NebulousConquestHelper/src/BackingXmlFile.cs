using System;
using System.IO;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
	public partial class BackingXmlFile<T>
    {
        private readonly String fileName;
        private readonly FileType fileType;

        public static BackingXmlFile<SerializedConquestFleet> Fleet(string fleetName)
        {
            return new BackingXmlFile<SerializedConquestFleet>(fleetName, FileType.Fleet);
        }

        public static BackingXmlFile<Game> Game(string gameName)
        {
            return new BackingXmlFile<Game>(gameName, FileType.Game);
        }
        public static BackingXmlFile<ComponentRegistry> ComponentRegistry(string registryName)
        {
            return new BackingXmlFile<ComponentRegistry>(registryName, FileType.ComponentRegistry);
        }

        private BackingXmlFile(String fileName, FileType fileType)
        {
            this.fileName = fileName;
            this.fileType = fileType;
        }

        public string Name
        {
            get
            {
                return this.fileName;
            }
        }

        public FilePath Path {
            get
			{
                return new FilePath(Helper.DATA_FOLDER_PATH + this.fileName + this.fileType.ToExtension());
			}
		}

        public T Object
        {
            get {
                using (FileStream stream = new FileStream(this.Path.RelativePath, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    T loaded = (T)serializer.Deserialize(stream);
                    stream.Close();
                    return loaded;
                }
            }
            set
            {
                if (!Directory.Exists(this.Path.Directory))
                {
                    Directory.CreateDirectory(this.Path.Directory);
                }
                using (FileStream stream = new FileStream(this.Path.RelativePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, value);
                    stream.Close();
                }
            }
        }
    }
}
