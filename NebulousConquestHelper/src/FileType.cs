namespace NebulousConquestHelper
{
	public enum FileType
	{
		Fleet,
		Game,
		ComponentRegistry
	}

	public static class FileTypeExtensions
	{
		public static string ToExtension(this FileType fileType)
		{
			if(fileType == FileType.Fleet)
			{
				return ".fleet";
			}
			if (fileType == FileType.Game)
			{
				return ".conquest";
			}
			if (fileType == FileType.ComponentRegistry)
			{
				return ".xml";
			}
			return null;
		}
	}
}
