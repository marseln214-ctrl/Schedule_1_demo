using System.Collections.Generic;
using System.IO;

namespace ScheduleOne.Persistence.Loaders
{
	public class PropertiesLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (Directory.Exists(mainPath))
			{
				List<DirectoryInfo> directories = GetDirectories(mainPath);
				PropertyLoader loader = new PropertyLoader();
				for (int i = 0; i < directories.Count; i++)
				{
					new LoadRequest(directories[i].FullName, loader);
				}
			}
		}
	}
}
