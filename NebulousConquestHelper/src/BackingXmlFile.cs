using System;
using System.IO;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
	public partial class BackingXmlFile<T>
	{
		private readonly String filePath;
		private readonly FileType fileType;

		public BackingXmlFile(String filePath, FileType fileType)
		{
			this.filePath = filePath;
			this.fileType = fileType;
		}

		public string Folder
		{
			get
			{
				return this.filePath.Substring(0, this.filePath.Length - Name.Length);
			}
		}

		public string Name
		{
			get
			{
				return Path.NameWithoutExtension;
			}
		}

		public FilePath Path
		{
			get
			{
				return new FilePath(Helper.DATA_FOLDER_PATH + this.filePath + this.fileType.ToExtension());
			}
		}

		public T LoadObject()
		{
			using (FileStream stream = new FileStream(this.Path.RelativePath, FileMode.Open, FileAccess.Read))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				T loaded = (T)serializer.Deserialize(stream);
				stream.Close();

				if (loaded is Backed<T> backed)
				{
					backed.SetFileReference(this);
				}

				return loaded;
			}
		}

		public void SaveObject(T obj)
		{
			if (!Directory.Exists(this.Path.Directory))
			{
				Directory.CreateDirectory(this.Path.Directory);
			}

			using (FileStream stream = new FileStream(this.Path.RelativePath, FileMode.Create))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(stream, obj);
				stream.Close();
			}
		}
	}
}
