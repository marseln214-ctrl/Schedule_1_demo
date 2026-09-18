using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;

namespace ScheduleOne.ItemFramework
{
	public interface IItemSlotOwner
	{
		List<ItemSlot> ItemSlots { get; set; }

		void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance);

		void SetItemSlotQuantity(int itemSlotIndex, int quantity);

		void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason);

		void SendItemsToClient(NetworkConnection conn)
		{
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (ItemSlots[i].IsLocked)
				{
					SetSlotLocked(conn, i, locked: true, ItemSlots[i].ActiveLock.LockOwner, ItemSlots[i].ActiveLock.LockReason);
				}
				if (ItemSlots[i].ItemInstance != null)
				{
					SetStoredInstance(conn, i, ItemSlots[i].ItemInstance);
				}
			}
		}

		int GetTotalItemCount()
		{
			return ItemSlots.Sum((ItemSlot x) => x.Quantity);
		}

		int GetItemCount(string id)
		{
			int num = 0;
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.ID == id)
				{
					num += ItemSlots[i].Quantity;
				}
			}
			return num;
		}
	}
}
