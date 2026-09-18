using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.WateringCan
{
	[Serializable]
	public class WateringCanInstance : StorableItemInstance
	{
		public float CurrentFillAmount;

		public WateringCanInstance()
		{
		}

		public WateringCanInstance(ItemDefinition definition, int quantity, float fillAmount)
			: base(definition, quantity)
		{
			CurrentFillAmount = fillAmount;
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new WateringCanInstance(base.Definition, quantity, CurrentFillAmount);
		}

		public void ChangeFillAmount(float change)
		{
			CurrentFillAmount = Mathf.Clamp(CurrentFillAmount + change, 0f, 15f);
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}

		public override ItemData GetItemData()
		{
			return new WateringCanData(ID, Quantity, CurrentFillAmount);
		}
	}
}
