using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class CleanerLoader : EmployeeLoader
	{
		public override string NPCType => typeof(CleanerData).Name;

		public override void Load(string mainPath)
		{
			Employee employee = LoadAndCreateEmployee(mainPath);
			if (employee == null)
			{
				return;
			}
			Cleaner cleaner = employee as Cleaner;
			if (cleaner == null)
			{
				Console.LogWarning("Failed to cast employee to Cleaner");
				return;
			}
			CleanerConfigurationData configData = null;
            CleanerData data = null;
            if (TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<CleanerConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			
			if (TryLoadFile(mainPath, "NPC", out var contents2))
			{
				data = null;
				try
				{
					data = JsonUtility.FromJson<CleanerData>(contents2);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (data == null)
				{
					Console.LogWarning("Failed to load cleaner data");
				}
				else
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration1()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration1);
				CleanerConfiguration obj = cleaner.Configuration as CleanerConfiguration;
				obj.Bed.Load(configData.Bed);
				obj.Bins.Load(configData.Bins);
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				cleaner.MoveItemBehaviour.Load(data.MoveItemData);
			}
		}
	}
}
