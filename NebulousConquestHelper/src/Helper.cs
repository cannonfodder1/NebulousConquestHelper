using System;
using System.IO;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    class Helper
	{
		public const string DATA_FOLDER_PATH = "./src/data/";
        public const string FLEET_FILE_TYPE = ".fleet";

		public static ComponentRegistry Registry;

		// extracted from Nebulous.dll with Unity logging removed and heavily modified
		public static object ReadXMLFile(Type type, FilePath path, Func<object, bool> postLoadInit = null)
		{
			try
			{
				using (FileStream stream = new FileStream(path.RelativePath, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(type);
					object loaded = serializer.Deserialize(stream);
					stream.Close();
					bool resultOk = postLoadInit != null ? postLoadInit(loaded) : true;
					if (resultOk) return loaded;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return null;
		}

		// extracted from Nebulous.dll with Unity logging removed and heavily modified
		public static bool WriteXMLFile(Type type, FilePath path, object obj)
		{
			try
			{
				bool flag = !Directory.Exists(path.Directory);
				if (flag)
				{
					Directory.CreateDirectory(path.Directory);
				}
				using (FileStream stream = new FileStream(path.RelativePath, FileMode.Create))
				{
					XmlSerializer serializer = new XmlSerializer(type);
					serializer.Serialize(stream, obj);
					stream.Close();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
			return true;
		}
	}
}
