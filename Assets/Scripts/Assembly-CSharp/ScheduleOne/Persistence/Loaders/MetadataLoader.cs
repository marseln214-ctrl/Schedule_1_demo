using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class MetadataLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents))
			{
				MetaData metaData = JsonUtility.FromJson<MetaData>(contents);
				if (metaData != null)
				{
					Singleton<MetadataManager>.Instance.Load(metaData);
				}
			}
		}
	}
}
