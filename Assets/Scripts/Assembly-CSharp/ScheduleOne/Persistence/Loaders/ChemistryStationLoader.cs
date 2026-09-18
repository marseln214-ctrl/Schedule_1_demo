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
	public class ChemistryStationLoader : GridItemLoader
	{
		public override string ItemType => typeof(ChemistryStationData).Name;

		public override void Load(string mainPath)
		{
			GridItem gridItem = LoadAndCreate(mainPath);
			if (gridItem == null)
			{
				Console.LogWarning("Failed to load grid item");
				return;
			}
			ChemistryStation station = gridItem as ChemistryStation;
			if (station == null)
			{
				Console.LogWarning("Failed to cast grid item to chemistry station");
				return;
			}
			ChemistryStationData data = GetData<ChemistryStationData>(mainPath);
			if (data == null)
			{
				Console.LogWarning("Failed to load chemistry station data");
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
			if (data.CurrentRecipeID != string.Empty)
			{
				ChemistryCookOperation operation = new ChemistryCookOperation(data.CurrentRecipeID, data.ProductQuality, data.StartLiquidColor, data.LiquidLevel, data.CurrentTime);
				station.SetCookOperation(null, operation);
			}
			ChemistryStationConfigurationData configData;
			if (File.Exists(Path.Combine(mainPath, "Configuration.json")) && TryLoadFile(mainPath, "Configuration", out var contents))
			{
				configData = JsonUtility.FromJson<ChemistryStationConfigurationData>(contents);
				if (configData != null)
				{
					Singleton<LoadManager>.Instance.onLoadComplete.AddListener(LoadConfiguration);
				}
			}
			void LoadConfiguration()
			{
				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(LoadConfiguration);
				(station.Configuration as ChemistryStationConfiguration).Recipe.Load(configData.Recipe);
				(station.Configuration as ChemistryStationConfiguration).Destination.Load(configData.Destination);
			}
		}
	}
}
