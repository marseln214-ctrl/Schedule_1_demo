using System;
using System.Collections.Generic;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using ScheduleOne.Trash;

namespace ScheduleOne.ObjectScripts.WateringCan
{
	[Serializable]
	public class TrashGrabberInstance : StorableItemInstance
	{
		public const int TRASH_CAPACITY = 20;

		private TrashContent Content = new TrashContent();

		public TrashGrabberInstance()
		{
		}

		public TrashGrabberInstance(ItemDefinition definition, int quantity)
			: base(definition, quantity)
		{
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			TrashGrabberInstance trashGrabberInstance = new TrashGrabberInstance(base.Definition, quantity);
			trashGrabberInstance.Content.LoadFromData(Content.GetData());
			return trashGrabberInstance;
		}

		public void LoadContentData(TrashContentData content)
		{
			Content.LoadFromData(content);
		}

		public override ItemData GetItemData()
		{
			return new TrashGrabberData(ID, Quantity, Content.GetData());
		}

		public void AddTrash(string id, int quantity)
		{
			Content.AddTrash(id, quantity);
			InvokeDataChange();
		}

		public void RemoveTrash(string id, int quantity)
		{
			Content.RemoveTrash(id, quantity);
			InvokeDataChange();
		}

		public void ClearTrash()
		{
			Content.Clear();
			InvokeDataChange();
		}

		public int GetTotalSize()
		{
			return Content.GetTotalSize();
		}

		public List<string> GetTrashIDs()
		{
			List<string> list = new List<string>();
			foreach (TrashContent.Entry entry in Content.Entries)
			{
				list.Add(entry.TrashID);
			}
			return list;
		}

		public List<int> GetTrashQuantities()
		{
			List<int> list = new List<int>();
			foreach (TrashContent.Entry entry in Content.Entries)
			{
				list.Add(entry.Quantity);
			}
			return list;
		}

		public List<ushort> GetTrashUshortQuantities()
		{
			List<ushort> list = new List<ushort>();
			foreach (TrashContent.Entry entry in Content.Entries)
			{
				list.Add((ushort)entry.Quantity);
			}
			return list;
		}
	}
}
