using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Loaders;
using UnityEngine;

namespace ScheduleOne.Persistence
{
	public class LoadRequest
	{
		public string Path;

		public Loader Loader;

		public bool IsDone { get; private set; }

		public LoadRequest(string filePath, Loader loader)
		{
			if (loader == null)
			{
				Debug.LogError("Loader is null for file path: " + filePath);
				return;
			}
			Path = filePath;
			Loader = loader;
			Singleton<LoadManager>.Instance.QueueLoadRequest(this);
		}

		public void Complete()
		{
			Singleton<LoadManager>.Instance.DequeueLoadRequest(this);
			Loader.Load(Path);
			IsDone = true;
		}
	}
}
