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
	public class LabOvenLoader : GridItemLoader
	{
		public override string ItemType => typeof(LabOvenData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			LabOven station = gridItem as LabOven;
			if (station == null)
			{
				Console.LogWarning("Failed to cast grid item to lab oven");
				return;
			}
			LabOvenData data = GetData<LabOvenData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load lab oven data");
				return;
			}
			for (int i = 0; i < data.InputContents.Items.Length; i++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(data.InputContents.Items[i]);
				if (station.ItemSlots.Count > i)
				{
					station.ItemSlots[i].SetStoredItem(instance);
				}
			}
			ItemInstance instance2 = ItemDeserializer.LoadItem(data.OutputContents.Items[0]);
			station.OutputSlot.SetStoredItem(instance2);
			if (data.CurrentIngredientID != string.Empty)
			{
				OvenCookOperation operation = new OvenCookOperation(data.CurrentIngredientID, data.CurrentIngredientQuality, data.CurrentIngredientQuantity, data.CurrentProductID, data.CurrentCookProgress);
				station.SetCookOperation(null, operation, playButtonPress: false);
			}
			LabOvenConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<LabOvenConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				(station.Configuration as LabOvenConfiguration).Destination.Load(configData.Destination);
			}
		}
	}
}
