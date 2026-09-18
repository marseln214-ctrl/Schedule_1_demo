using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class ItemSet
	{
		public string[] Items;

		public ItemSet(List<ItemData> items)
		{
			Items = new string[items.Count];
			for (int i = 0; i < items.Count; i++)
			{
				Items[i] = items[i].GetJson(prettyPrint: false);
			}
		}

		public string GetJSON()
		{
			return JsonUtility.ToJson(this, prettyPrint: true);
		}

		public ItemSet(List<ItemInstance> items)
		{
			Items = new string[items.Count];
			for (int i = 0; i < items.Count; i++)
			{
				Items[i] = items[i].GetItemData().GetJson(prettyPrint: false);
			}
		}

		public ItemSet(List<ItemSlot> itemSlots)
		{
			Items = new string[itemSlots.Count];
			for (int i = 0; i < itemSlots.Count; i++)
			{
				if (itemSlots[i].ItemInstance != null)
				{
					Items[i] = itemSlots[i].ItemInstance.GetItemData().GetJson(prettyPrint: false);
				}
				else
				{
					Items[i] = new ItemData(string.Empty, 0).GetJson(prettyPrint: false);
				}
			}
		}

		public ItemSet(ItemSlot[] itemSlots)
		{
			Items = new string[itemSlots.Length];
			for (int i = 0; i < itemSlots.Length; i++)
			{
				if (itemSlots[i].ItemInstance != null)
				{
					Items[i] = itemSlots[i].ItemInstance.GetItemData().GetJson(prettyPrint: false);
				}
				else
				{
					Items[i] = new ItemData(string.Empty, 0).GetJson(prettyPrint: false);
				}
			}
		}

		public static ItemInstance[] Deserialize(string json)
		{
			ItemSet itemSet = null;
			try
			{
				itemSet = JsonUtility.FromJson<ItemSet>(json);
			}
			catch (Exception ex)
			{
				Console.LogError("Failed to deserialize ItemSet from JSON: " + json + "\nException: " + ex);
				return new ItemInstance[0];
			}
			ItemInstance[] array = new ItemInstance[itemSet.Items.Length];
			for (int i = 0; i < itemSet.Items.Length; i++)
			{
				array[i] = ItemDeserializer.LoadItem(itemSet.Items[i]);
			}
			return array;
		}
	}
}
