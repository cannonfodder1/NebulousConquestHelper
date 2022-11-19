using System;
using System.Xml.Serialization;

namespace NebulousConquestHelper
{
	public abstract class Backed<T>
	{
		public static BackingXmlFile<T> NewFile(string fileName)
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
				throw new Exception("Unknown File Type");
			}
		}

		[XmlIgnore]
		public string FileName { get; set; }
		[XmlIgnore]
		public T _XML { get; set; }

		[XmlIgnore]
		public virtual BackingXmlFile<T> BackingFile {
			get
            {			
				return new BackingXmlFile<T>(this.FileName, FileType);
            }
			set
            {
				this.FileName = value.Name;
            }
		}
		[XmlIgnore]
		public virtual T XML {
			get
            {
				if (this._XML == null)
                {
					this._XML = this.BackingFile.Object;
                }
				return this._XML;
            }
		}
	}
}
