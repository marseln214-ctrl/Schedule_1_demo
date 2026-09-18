using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class ProductList
	{
		[Serializable]
		public class Entry
		{
			public string ProductID;

			public EQuality Quality;

			public int Quantity;
		}

		public List<Entry> entries = new List<Entry>();

		public string GetCommaSeperatedString()
		{
			string text = string.Empty;
			foreach (Entry entry in entries)
			{
				if (entry.Quantity == 0)
				{
					continue;
				}
				ItemDefinition item = Registry.GetItem(entry.ProductID);
				if (item == null)
				{
					Debug.LogError("Item not found: " + entry.ProductID);
					continue;
				}
				text = text + entry.Quantity + "x ";
				text += item.Name;
				if (entry != entries[entries.Count - 1])
				{
					text += ", ";
				}
			}
			return text;
		}

		public string GetLineSeperatedString()
		{
			string text = "\n";
			foreach (Entry entry in entries)
			{
				text = text + entry.Quantity + "x ";
				text += Registry.GetItem(entry.ProductID).Name;
				if (entry != entries[entries.Count - 1])
				{
					text += "\n";
				}
			}
			return text;
		}

		public string GetQualityString()
		{
			Entry entry = entries[0];
			return "<color=#" + ColorUtility.ToHtmlStringRGBA(ItemQuality.GetColor(entry.Quality)) + ">" + entry.Quality.ToString() + "</color> ";
		}

		public int GetTotalQuantity()
		{
			int num = 0;
			foreach (Entry entry in entries)
			{
				num += entry.Quantity;
			}
			return num;
		}
	}
}
