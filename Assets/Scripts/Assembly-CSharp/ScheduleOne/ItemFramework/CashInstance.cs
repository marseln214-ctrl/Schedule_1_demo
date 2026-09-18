using System;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ItemFramework
{
	[Serializable]
	public class CashInstance : StorableItemInstance
	{
		public const float MAX_BALANCE = 1E+09f;

		public float Balance { get; protected set; }

		public CashInstance()
		{
		}

		public CashInstance(ItemDefinition definition, int quantity)
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
			return new CashInstance(base.Definition, quantity);
		}

		public void ChangeBalance(float amount)
		{
			SetBalance(Balance + amount);
		}

		public void SetBalance(float newBalance, bool blockClear = false)
		{
			Balance = Mathf.Clamp(newBalance, 0f, 1E+09f);
			if (Balance <= 0f && !blockClear)
			{
				RequestClearSlot();
			}
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}

		public override ItemData GetItemData()
		{
			return new CashData(ID, Quantity, Balance);
		}

		public override float GetMonetaryValue()
		{
			return Balance;
		}
	}
}
