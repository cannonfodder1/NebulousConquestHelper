using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	public abstract class Backed<T>
	{
		public static T Load(string fileName)
		{
			return NewFileReference(fileName).LoadObject();
		}

		public static BackingXmlFile<T> NewFileReference(string fileName)
		{
			return new BackingXmlFile<T>(fileName, FileType);
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
		private T XML { get; set; }

		public virtual BackingXmlFile<T> GenerateFileReference()
        {
			return new BackingXmlFile<T>(this.FileName, FileType);
		}
		
		public virtual void SetFileReference(BackingXmlFile<T> BackingFile)
        {
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
