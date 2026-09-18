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
	public class PackagingStationLoader : GridItemLoader
	{
		public override string ItemType => typeof(PackagingStationData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			PackagingStation station = gridItem as PackagingStation;
			if (station == null)
			{
				Console.LogWarning("Failed to cast grid item to pot");
				return;
			}
			PackagingStationData data = GetData<PackagingStationData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load packaging station data data");
				return;
			}
			for (int i = 0; i < data.Contents.Items.Length; i++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(data.Contents.Items[i]);
				if (station.ItemSlots.Count > i)
				{
					station.ItemSlots[i].SetStoredItem(instance);
				}
			}
			station.UpdatePackagingVisuals();
			station.UpdateProductVisuals();
			PackagingStationConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<PackagingStationConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				(station.Configuration as PackagingStationConfiguration).Destination.Load(configData.Destination);
			}
		}
	}
}
