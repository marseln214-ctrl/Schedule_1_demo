using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class GameDataLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents))
			{
				GameData gameData = JsonUtility.FromJson<GameData>(contents);
				if (gameData != null)
				{
					NetworkSingleton<GameManager>.Instance.Load(gameData, mainPath);
				}
			}
		}
	}
}
