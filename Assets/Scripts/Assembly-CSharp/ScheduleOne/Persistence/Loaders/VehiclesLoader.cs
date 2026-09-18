using System.Collections.Generic;
using System.IO;

namespace ScheduleOne.Persistence.Loaders
{
	public class VehiclesLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (Directory.Exists(mainPath))
			{
				List<DirectoryInfo> directories = GetDirectories(mainPath);
				VehicleLoader loader = new VehicleLoader();
				for (int i = 0; i < directories.Count; i++)
				{
					new LoadRequest(directories[i].FullName, loader);
				}
			}
		}
	}
}
