using System.Collections;
using System.Collections.Generic;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class PlayersLoader : Loader
	{
		public override void Load(string mainPath)
		{
			List<DirectoryInfo> directories = GetDirectories(mainPath);
			Console.Log("Loading players");
			LoadRequest lastLoadRequest = null;
			PlayerLoader loader = new PlayerLoader();
			for (int i = 0; i < directories.Count; i++)
			{
				lastLoadRequest = new LoadRequest(directories[i].FullName, loader);
			}
			Singleton<CoroutineService>.Instance.StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => lastLoadRequest == null || lastLoadRequest.IsDone);
				Singleton<PlayerManager>.Instance.AllPlayerFilesLoaded();
			}
		}
	}
}
