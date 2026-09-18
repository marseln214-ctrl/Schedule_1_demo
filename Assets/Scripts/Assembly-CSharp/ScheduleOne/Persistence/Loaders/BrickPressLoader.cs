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
	public class BrickPressLoader : GridItemLoader
	{
		public override string ItemType => typeof(BrickPressData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			BrickPress brickPress = gridItem as BrickPress;
			if (brickPress == null)
			{
				Console.LogWarning("Failed to cast grid item to brick press");
				return;
			}
			BrickPressData data = GetData<BrickPressData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load brick press data");
				return;
			}
			for (int i = 0; i < data.Contents.Items.Length; i++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(data.Contents.Items[i]);
				if (brickPress.ItemSlots.Count > i)
				{
					brickPress.ItemSlots[i].SetStoredItem(instance);
				}
			}
			BrickPressConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<BrickPressConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				(brickPress.Configuration as BrickPressConfiguration).Destination.Load(configData.Destination);
			}
		}
	}
}
