using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace ScheduleOne.ItemFramework
{
	[Serializable]
	public class ItemSlot
	{
		public Action onItemDataChanged;

		public Action onItemInstanceChanged;

		public Action onLocked;

		public Action onUnlocked;

		public ItemInstance ItemInstance { get; protected set; }

		public IItemSlotOwner SlotOwner { get; protected set; }

		private int SlotIndex => SlotOwner.ItemSlots.IndexOf(this);

		public int Quantity
		{
			get
			{
				if (ItemInstance == null)
				{
					return 0;
				}
				return ItemInstance.Quantity;
			}
		}

		public bool IsAtCapacity
		{
			get
			{
				if (ItemInstance != null)
				{
					return Quantity >= ItemInstance.StackLimit;
				}
				return false;
			}
		}

		public bool IsLocked => ActiveLock != null;

		public ItemSlotLock ActiveLock { get; protected set; }

		public bool IsRemovalLocked { get; protected set; }

		public bool IsAddLocked { get; protected set; }

		protected List<ItemFilter> Filters { get; set; } = new List<ItemFilter>();


		public void SetSlotOwner(IItemSlotOwner owner)
		{
			SlotOwner = owner;
			SlotOwner.ItemSlots.Add(this);
		}

		public void ReplicateStoredInstance()
		{
			if (SlotOwner != null)
			{
				SlotOwner.SetStoredInstance(null, SlotIndex, ItemInstance);
			}
		}

		public virtual void SetStoredItem(ItemInstance instance, bool _internal = false)
		{
			if (IsLocked)
			{
				Console.LogError("SetStoredInstance called on ItemSlot that is locked! Refusing.");
				return;
			}
			if (IsRemovalLocked)
			{
				Console.LogWarning("SetStoredItem called on ItemSlot that isRemovalLocked. You probably shouldn't do this.");
			}
			if (_internal || SlotOwner == null)
			{
				if (ItemInstance != null)
				{
					ClearStoredInstance(_internal: true);
				}
				ItemInstance = instance;
				if (ItemInstance != null)
				{
					ItemInstance itemInstance = ItemInstance;
					itemInstance.onDataChanged = (Action)Delegate.Combine(itemInstance.onDataChanged, new Action(ItemDataChanged));
					ItemInstance itemInstance2 = ItemInstance;
					itemInstance2.requestClearSlot = (Action)Delegate.Combine(itemInstance2.requestClearSlot, new Action(ClearItemInstanceRequested));
				}
				if (onItemDataChanged != null)
				{
					onItemDataChanged();
				}
				if (onItemInstanceChanged != null)
				{
					onItemInstanceChanged();
				}
				ItemDataChanged();
			}
			else
			{
				SlotOwner.SetStoredInstance(null, SlotIndex, instance);
			}
		}

		public virtual void InsertItem(ItemInstance item)
		{
			if (ItemInstance == null)
			{
				AddItem(item);
			}
			else if (ItemInstance.CanStackWith(item))
			{
				ChangeQuantity(item.Quantity);
			}
			else
			{
				Console.LogWarning("InsertItem called with item that cannot stack with current item. Refusing.");
			}
		}

		public virtual void AddItem(ItemInstance item, bool _internal = false)
		{
			if (ItemInstance == null)
			{
				SetStoredItem(item, _internal);
			}
			else if (!ItemInstance.CanStackWith(item))
			{
				Console.LogWarning("AddItem called with item that cannot stack with current item. Refusing.");
			}
			else
			{
				ChangeQuantity(item.Quantity, _internal);
			}
		}

		public virtual void ClearStoredInstance(bool _internal = false)
		{
			if (IsLocked)
			{
				Console.LogError("ClearStoredInstance called on ItemSlot that is locked! Refusing.");
			}
			else if (IsRemovalLocked)
			{
				Console.LogError("ClearStoredInstance called on ItemSlot that is removal locked! Refusing.");
			}
			else
			{
				if (ItemInstance == null)
				{
					return;
				}
				if (_internal || SlotOwner == null)
				{
					ItemInstance itemInstance = ItemInstance;
					itemInstance.onDataChanged = (Action)Delegate.Remove(itemInstance.onDataChanged, new Action(ItemDataChanged));
					ItemInstance itemInstance2 = ItemInstance;
					itemInstance2.requestClearSlot = (Action)Delegate.Remove(itemInstance2.requestClearSlot, new Action(ClearItemInstanceRequested));
					ItemInstance = null;
					if (onItemDataChanged != null)
					{
						onItemDataChanged();
					}
					if (onItemInstanceChanged != null)
					{
						onItemInstanceChanged();
					}
				}
				else
				{
					SlotOwner.SetStoredInstance(null, SlotIndex, null);
				}
			}
		}

		public void SetQuantity(int amount, bool _internal = false)
		{
			if (IsLocked)
			{
				Console.LogError("SetQuantity called on ItemSlot that is locked! Refusing.");
			}
			else if (ItemInstance == null)
			{
				Console.LogWarning("ChangeQuantity called but ItemInstance is null");
			}
			else if (amount < ItemInstance.Quantity && IsRemovalLocked)
			{
				Console.LogError("SetQuantity called on ItemSlot and passed lower quantity that current, and isRemovalLocked = true. Refusing.");
			}
			else if (_internal || SlotOwner == null)
			{
				ItemInstance.SetQuantity(amount);
			}
			else
			{
				SlotOwner.SetItemSlotQuantity(SlotIndex, amount);
			}
		}

		public void ChangeQuantity(int change, bool _internal = false)
		{
			if (IsLocked)
			{
				Console.LogWarning("isLocked = true!");
			}
			else if (ItemInstance == null)
			{
				Console.LogWarning("ChangeQuantity called but ItemInstance is null");
			}
			else if (IsRemovalLocked && change < 0)
			{
				Console.Log("Removal locked!");
			}
			else if (_internal || SlotOwner == null)
			{
				ItemInstance.ChangeQuantity(change);
			}
			else
			{
				SlotOwner.SetItemSlotQuantity(SlotIndex, Quantity + change);
			}
		}

		protected virtual void ItemDataChanged()
		{
			if (ItemInstance != null && ItemInstance.Quantity <= 0)
			{
				ClearStoredInstance();
			}
			else if (onItemDataChanged != null)
			{
				onItemDataChanged();
			}
		}

		protected virtual void ClearItemInstanceRequested()
		{
			ClearStoredInstance();
		}

		public void AddFilter(ItemFilter filter)
		{
			Filters.Add(filter);
		}

		public void ApplyLock(NetworkObject lockOwner, string lockReason, bool _internal = false)
		{
			if (_internal || SlotOwner == null)
			{
				ActiveLock = new ItemSlotLock(this, lockOwner, lockReason);
				if (onLocked != null)
				{
					onLocked();
				}
			}
			else
			{
				SlotOwner.SetSlotLocked(null, SlotIndex, locked: true, lockOwner, lockReason);
			}
		}

		public void RemoveLock(bool _internal = false)
		{
			if (_internal || SlotOwner == null)
			{
				ActiveLock = null;
				if (onUnlocked != null)
				{
					onUnlocked();
				}
			}
			else
			{
				SlotOwner.SetSlotLocked(null, SlotIndex, locked: false, null, string.Empty);
			}
		}

		public void SetIsRemovalLocked(bool locked)
		{
			IsRemovalLocked = locked;
		}

		public void SetIsAddLocked(bool locked)
		{
			IsAddLocked = locked;
		}

		public virtual bool DoesItemMatchFilters(ItemInstance item)
		{
			foreach (ItemFilter filter in Filters)
			{
				if (!filter.DoesItemMatchFilter(item))
				{
					return false;
				}
			}
			if (item is CashInstance)
			{
				return CanSlotAcceptCash();
			}
			return true;
		}

		public virtual int GetCapacityForItem(ItemInstance item)
		{
			if (ItemInstance == null || ItemInstance.CanStackWith(item, checkQuantities: false))
			{
				return item.StackLimit - Quantity;
			}
			return 0;
		}

		public virtual bool CanSlotAcceptCash()
		{
			return true;
		}

		public static bool TryInsertItemIntoSet(List<ItemSlot> ItemSlots, ItemInstance item)
		{
			int num = item.Quantity;
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				if (!ItemSlots[i].IsLocked && !ItemSlots[i].IsAddLocked && ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.CanStackWith(item))
				{
					int num2 = Mathf.Min(item.StackLimit - ItemSlots[i].ItemInstance.Quantity, num);
					num -= num2;
					ItemSlots[i].ChangeQuantity(num2);
				}
			}
			for (int j = 0; j < ItemSlots.Count; j++)
			{
				if (num <= 0)
				{
					break;
				}
				if (!ItemSlots[j].IsLocked && !ItemSlots[j].IsAddLocked && ItemSlots[j].ItemInstance == null)
				{
					num -= item.StackLimit;
					ItemSlots[j].SetStoredItem(item);
					break;
				}
			}
			return num <= 0;
		}
	}
}
