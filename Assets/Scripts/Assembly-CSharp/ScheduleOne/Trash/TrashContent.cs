using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;

namespace ScheduleOne.Trash
{
	[Serializable]
	public class TrashContent
	{
		[Serializable]
		public class Entry
		{
			public string TrashID;

			public int Quantity;

			public int UnitSize { get; private set; }

			public int UnitValue { get; private set; }

			public Entry(string id, int quantity)
			{
				TrashID = id;
				Quantity = quantity;
				TrashItem trashPrefab = NetworkSingleton<TrashManager>.Instance.GetTrashPrefab(id);
				if (trashPrefab != null)
				{
					UnitSize = trashPrefab.Size;
					UnitValue = trashPrefab.SellValue;
				}
			}
		}

		public List<Entry> Entries = new List<Entry>();

		public void AddTrash(string trashID, int quantity)
		{
			Entry entry = Entries.Find((Entry e) => e.TrashID == trashID);
			if (entry == null)
			{
				entry = new Entry(trashID, 0);
				Entries.Add(entry);
			}
			Entries.Remove(entry);
			Entries.Add(entry);
			entry.Quantity += quantity;
		}

		public void RemoveTrash(string trashID, int quantity)
		{
			Entry entry = Entries.Find((Entry e) => e.TrashID == trashID);
			if (entry != null)
			{
				entry.Quantity -= quantity;
				if (entry.Quantity <= 0)
				{
					Entries.Remove(entry);
				}
			}
		}

		public int GetTrashQuantity(string trashID)
		{
			return Entries.Find((Entry e) => e.TrashID == trashID)?.Quantity ?? 0;
		}

		public void Clear()
		{
			Entries.Clear();
		}

		public int GetTotalSize()
		{
			int num = 0;
			foreach (Entry entry in Entries)
			{
				num += entry.Quantity * entry.UnitSize;
			}
			return num;
		}

		public TrashContentData GetData()
		{
			TrashContentData trashContentData = new TrashContentData();
			trashContentData.TrashIDs = new string[Entries.Count];
			trashContentData.TrashQuantities = new int[Entries.Count];
			for (int i = 0; i < Entries.Count; i++)
			{
				trashContentData.TrashIDs[i] = Entries[i].TrashID;
				trashContentData.TrashQuantities[i] = Entries[i].Quantity;
			}
			return trashContentData;
		}

		public void LoadFromData(TrashContentData data)
		{
			for (int i = 0; i < data.TrashIDs.Length; i++)
			{
				AddTrash(data.TrashIDs[i], data.TrashQuantities[i]);
			}
		}
	}
}
