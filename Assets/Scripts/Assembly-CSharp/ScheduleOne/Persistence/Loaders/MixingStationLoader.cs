using System.IO;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class MixingStationLoader : GridItemLoader
	{
		public override string ItemType => typeof(MixingStationData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			MixingStation station = gridItem as MixingStation;
			if (station == null)
			{
				Console.LogWarning("Failed to cast grid item to mixing station");
				return;
			}
			MixingStationData data = GetData<MixingStationData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load mixing station data");
				return;
			}
			ItemInstance instance = ItemDeserializer.LoadItem(data.ProductContents.Items[0]);
			station.ProductSlot.SetStoredItem(instance);
			ItemInstance instance2 = ItemDeserializer.LoadItem(data.MixerContents.Items[0]);
			station.MixerSlot.SetStoredItem(instance2);
			ItemInstance instance3 = ItemDeserializer.LoadItem(data.OutputContents.Items[0]);
			station.OutputSlot.SetStoredItem(instance3);
			if (data.CurrentMixOperation != null)
			{
				station.SetMixOperation(null, data.CurrentMixOperation, data.CurrentMixTime);
				if (data.CurrentMixTime >= station.GetMixTimeForCurrentOperation())
				{
					station.MixingDone_Networked();
				}
			}
			MixingStationConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<MixingStationConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				(station.Configuration as MixingStationConfiguration).Destination.Load(configData.Destination);
				(station.Configuration as MixingStationConfiguration).StartThrehold.Load(configData.Threshold);
			}
		}
	}
}
