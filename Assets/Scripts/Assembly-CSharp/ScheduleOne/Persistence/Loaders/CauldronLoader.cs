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
	public class CauldronLoader : GridItemLoader
	{
		public override string ItemType => typeof(CauldronData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			Cauldron station = gridItem as Cauldron;
			if (station == null)
			{
				Console.LogWarning("Failed to cast grid item to Cauldron");
				return;
			}
			CauldronData data = GetData<CauldronData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load cauldron data");
				return;
			}
			for (int i = 0; i < data.Ingredients.Items.Length; i++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(data.Ingredients.Items[i]);
				if (station.IngredientSlots.Length > i)
				{
					station.IngredientSlots[i].SetStoredItem(instance);
				}
			}
			ItemInstance instance2 = ItemDeserializer.LoadItem(data.Liquid.Items[0]);
			station.LiquidSlot.SetStoredItem(instance2);
			ItemInstance instance3 = ItemDeserializer.LoadItem(data.Output.Items[0]);
			station.OutputSlot.SetStoredItem(instance3);
			if (data.RemainingCookTime > 0)
			{
				station.StartCookOperation(null, data.RemainingCookTime, data.InputQuality);
			}
			CauldronConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<CauldronConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				(station.Configuration as CauldronConfiguration).Destination.Load(configData.Destination);
			}
		}
	}
}
