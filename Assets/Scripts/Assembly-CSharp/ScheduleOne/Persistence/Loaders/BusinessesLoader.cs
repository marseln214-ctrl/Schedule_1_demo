using System.Collections.Generic;
using System.IO;

namespace ScheduleOne.Persistence.Loaders
{
	public class BusinessesLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (Directory.Exists(mainPath))
			{
				List<DirectoryInfo> directories = GetDirectories(mainPath);
				BusinessLoader loader = new BusinessLoader();
				for (int i = 0; i < directories.Count; i++)
				{
					new LoadRequest(directories[i].FullName, loader);
				}
			}
		}
	}
}
