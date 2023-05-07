using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	public abstract class Backed<T>
	{
		public static T Load(string filePath)
		{
			return NewFileReference(filePath).LoadObject();
		}

		public static BackingXmlFile<T> NewFileReference(string filePath)
		{
			return new BackingXmlFile<T>(filePath, FileType);
		}

		[XmlIgnore]
		public static FileType FileType
		{
			get
			{
				if (typeof(T) == typeof(SerializedConquestFleet))
				{
					return FileType.Fleet;
				}
				if (typeof(T) == typeof(Game))
				{
					return FileType.Game;
				}
				if (typeof(T) == typeof(ComponentRegistry))
				{
					return FileType.ComponentRegistry;
				}
				if (typeof(T) == typeof(MunitionRegistry))
				{
					return FileType.MunitionRegistry;
				}
				throw new Exception("Unknown File Type");
			}
		}

		[XmlIgnore]
		public string FileName { get; set; }
		
		[XmlIgnore]
		public string FilePath { get; set; }

		[XmlIgnore]
		private T XML { get; set; }

		public virtual BackingXmlFile<T> GenerateFileReference()
        {
			return new BackingXmlFile<T>(this.FilePath, FileType);
		}
		
		public virtual void SetFileReference(BackingXmlFile<T> BackingFile)
        {
			this.FilePath = BackingFile.Folder + BackingFile.Name;
			this.FileName = BackingFile.Name;
		}

		public virtual T GetXML()
		{
			if (this.XML == null)
			{
				this.XML = this.GenerateFileReference().LoadObject();
			}

			return this.XML;
		}
	}
}
