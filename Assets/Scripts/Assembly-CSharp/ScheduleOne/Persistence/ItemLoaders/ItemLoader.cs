using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class ItemLoader
	{
		public virtual string ItemType => typeof(ItemData).Name;

		public ItemLoader()
		{
			Singleton<LoadManager>.Instance.ItemLoaders.Add(this);
		}

		public virtual ItemInstance LoadItem(string itemString)
		{
			ItemData itemData = LoadData<ItemData>(itemString);
			if (itemData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (itemData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(itemData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + itemData.ID);
				return null;
			}
			return new StorableItemInstance(item, itemData.Quantity);
		}

		protected T LoadData<T>(string itemString) where T : ItemData
		{
			T val = null;
			try
			{
				return JsonUtility.FromJson<T>(itemString);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error parsing item data: " + itemString + "\n" + ex);
				return null;
			}
		}
	}
}
