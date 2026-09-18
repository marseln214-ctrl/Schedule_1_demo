using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class BuildableItemLoader : Loader
	{
		public virtual string ItemType => typeof(BuildableItemData).Name;

		public BuildableItemLoader()
		{
			Singleton<LoadManager>.Instance.ObjectLoaders.Add(this);
		}

		public override void Load(string mainPath)
		{
			BuildableItemData buildableItemData = GetBuildableItemData(mainPath);
			if (buildableItemData != null)
			{
				BuildableItemLoader objectLoader = Singleton<LoadManager>.Instance.GetObjectLoader(buildableItemData.DataType);
				if (objectLoader != null)
				{
					new LoadRequest(mainPath, objectLoader);
				}
			}
		}

		public BuildableItemData GetBuildableItemData(string mainPath)
		{
			return GetData<BuildableItemData>(mainPath);
		}

		protected T GetData<T>(string mainPath) where T : BuildableItemData
		{
			if (TryLoadFile(mainPath, "Data", out var contents))
			{
				T result = null;
				try
				{
					result = JsonUtility.FromJson<T>(contents);
					return result;
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				return result;
			}
			return null;
		}
	}
}
