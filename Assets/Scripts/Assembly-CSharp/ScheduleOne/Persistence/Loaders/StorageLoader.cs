using System;
using System.IO;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class StorageLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (!Directory.Exists(mainPath))
			{
				return;
			}
			string[] files = Directory.GetFiles(mainPath);
			for (int i = 0; i < files.Length; i++)
			{
				if (!TryLoadFile(files[i], out var contents, autoAddExtension: false))
				{
					continue;
				}
				WorldStorageEntityData worldStorageEntityData = null;
				try
				{
					worldStorageEntityData = JsonUtility.FromJson<WorldStorageEntityData>(contents);
				}
				catch (Exception ex)
				{
					Debug.LogError("Error loading data: " + ex.Message);
				}
				if (worldStorageEntityData != null)
				{
					WorldStorageEntity @object = GUIDManager.GetObject<WorldStorageEntity>(new Guid(worldStorageEntityData.GUID));
					if (@object != null)
					{
						@object.Load(worldStorageEntityData);
					}
				}
			}
		}
	}
}
