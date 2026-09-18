using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class PotLoader : GridItemLoader
	{
		public override string ItemType => typeof(PotData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			Pot pot = gridItem as Pot;
			if (pot == null)
			{
				Console.LogWarning("Failed to cast grid item to pot");
				return;
			}
			PotConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<PotConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			PotData data = GetData<PotData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load pot data");
				return;
			}
			if (!string.IsNullOrEmpty(data.SoilID))
			{
				pot.SetSoilID(data.SoilID);
				pot.AddSoil(data.SoilLevel);
				pot.SetSoilUses(data.RemainingSoilUses);
			}
			pot.ChangeWaterAmount(data.WaterLevel);
			for (int i = 0; i < data.AppliedAdditives.Length; i++)
			{
				pot.ApplyAdditive(null, data.AppliedAdditives[i], initial: false);
			}
			pot.LoadPlant(data.PlantData);
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				PotConfiguration obj = pot.Configuration as PotConfiguration;
				obj.Seed.Load(configData.Seed);
				obj.Additive1.Load(configData.Additive1);
				obj.Additive2.Load(configData.Additive2);
				obj.Additive3.Load(configData.Additive3);
				obj.Destination.Load(configData.Destination);
			}
		}
	}
}
