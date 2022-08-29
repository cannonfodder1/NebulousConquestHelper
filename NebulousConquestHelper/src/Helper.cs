using System;
using System.IO;
using System.Xml.Serialization;
using Utility;

namespace NebulousConquestHelper
{
    class Helper
	{
		// extracted from Nebulous.dll with Unity logging removed
		public static SerializedConquestFleet ReadFleetFile(FilePath path)
		{
			try
			{
				using (FileStream stream = new FileStream(path.RelativePath, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(SerializedConquestFleet));
					SerializedConquestFleet loaded = (SerializedConquestFleet)serializer.Deserialize(stream);
					stream.Close();
					return loaded;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return null;
		}

		// extracted from Nebulous.dll with Unity logging removed
		public static bool WriteFleetFile(FilePath path, SerializedConquestFleet fleet)
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
					XmlSerializer serializer = new XmlSerializer(typeof(SerializedConquestFleet));
					serializer.Serialize(stream, fleet);
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
