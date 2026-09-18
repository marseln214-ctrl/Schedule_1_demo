using System.Collections.Generic;
using System.IO;

namespace ScheduleOne.Persistence.Loaders
{
	public class NPCsLoader : Loader
	{
		public override void Load(string mainPath)
		{
			List<DirectoryInfo> directories = GetDirectories(mainPath);
			NPCLoader loader = new NPCLoader();
			for (int i = 0; i < directories.Count; i++)
			{
				new LoadRequest(directories[i].FullName, loader);
			}
		}
	}
}
