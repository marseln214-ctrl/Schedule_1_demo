using System;
using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class GenericSaveablesLoader : Loader
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
				if (TryLoadFile(files[i], out var contents, autoAddExtension: false))
				{
					GenericSaveData genericSaveData = null;
					try
					{
						genericSaveData = JsonUtility.FromJson<GenericSaveData>(contents);
					}
					catch (Exception ex)
					{
						Debug.LogError("Error loading generic save data: " + ex.Message);
					}
					if (genericSaveData != null)
					{
						Singleton<GenericSaveablesManager>.Instance.LoadSaveable(genericSaveData);
					}
				}
			}
		}
	}
}
