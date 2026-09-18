using System;
using FishNet.Serializing.Helping;
using ScheduleOne.Equipping;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.ItemFramework
{
	[Serializable]
	public abstract class ItemInstance
	{
		[CodegenExclude]
		protected ItemDefinition definition;

		public string ID = string.Empty;

		public int Quantity = 1;

		[CodegenExclude]
		public Action onDataChanged;

		[CodegenExclude]
		public Action requestClearSlot;

		[CodegenExclude]
		public ItemDefinition Definition
		{
			get
			{
				if (definition == null)
				{
					definition = Registry.GetItem(ID);
					if (definition == null)
					{
						Console.LogError("Failed to find definition with ID: " + ID);
					}
				}
				return definition;
			}
		}

		[CodegenExclude]
		public virtual string Name => Definition.Name;

		[CodegenExclude]
		public virtual string Description => Definition.Description;

		[CodegenExclude]
		public virtual Sprite Icon => Definition.Icon;

		[CodegenExclude]
		public virtual EItemCategory Category => Definition.Category;

		[CodegenExclude]
		public virtual int StackLimit => Definition.StackLimit;

		[CodegenExclude]
		public virtual Color LabelDisplayColor => Definition.LabelDisplayColor;

		[CodegenExclude]
		public virtual Equippable Equippable => Definition.Equippable;

		public ItemInstance()
		{
		}

		public ItemInstance(ItemDefinition definition, int quantity)
		{
			this.definition = definition;
			Quantity = quantity;
			ID = definition.ID;
		}

		public virtual bool CanStackWith(ItemInstance other, bool checkQuantities = true)
		{
			if (other == null)
			{
				return false;
			}
			if (other.ID != ID)
			{
				return false;
			}
			if (checkQuantities && Quantity + other.Quantity > StackLimit)
			{
				return false;
			}
			return true;
		}

		public virtual ItemInstance GetCopy(int overrideQuantity = -1)
		{
			Console.LogError("This should be overridden in the definition class!");
			return null;
		}

		public virtual bool IsValidInstance()
		{
			if (ID != string.Empty && Definition != null)
			{
				return Quantity > 0;
			}
			return false;
		}

		protected void InvokeDataChange()
		{
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}

		public void SetQuantity(int quantity)
		{
			if (quantity < 0)
			{
				Debug.LogError("SetQuantity called and passed quantity less than zero.");
				return;
			}
			if (quantity > StackLimit && quantity > Quantity)
			{
				Debug.LogError("SetQuantity called and passed quantity larger than stack limit.");
				return;
			}
			Quantity = quantity;
			InvokeDataChange();
		}

		public void ChangeQuantity(int change)
		{
			int num = Quantity + change;
			if (num < 0)
			{
				Debug.LogError("ChangeQuantity called and passed quantity less than zero.");
				return;
			}
			if (num > StackLimit)
			{
				Debug.LogError("ChangeQuantity called and passed quantity larger than stack limit.");
				return;
			}
			Quantity = num;
			InvokeDataChange();
		}

		public virtual ItemData GetItemData()
		{
			return new ItemData(ID, Quantity);
		}

		public virtual float GetMonetaryValue()
		{
			return 0f;
		}

		public void RequestClearSlot()
		{
			if (requestClearSlot != null)
			{
				requestClearSlot();
			}
		}
	}
}
