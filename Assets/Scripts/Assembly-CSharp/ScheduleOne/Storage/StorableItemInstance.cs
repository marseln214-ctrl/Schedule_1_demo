using System;
using FishNet.Serializing.Helping;
using ScheduleOne.ItemFramework;

namespace ScheduleOne.Storage
{
	[Serializable]
	public class StorableItemInstance : ItemInstance
	{
		[CodegenExclude]
		public virtual StoredItem StoredItem
		{
			get
			{
				if (base.Definition != null && base.Definition is StorableItemDefinition)
				{
					return (base.Definition as StorableItemDefinition).StoredItem;
				}
				Console.LogError("StorableItemInstance has invalid definition: " + base.Definition);
				return null;
			}
		}

		public StorableItemInstance()
		{
		}

		public StorableItemInstance(ItemDefinition definition, int quantity)
			: base(definition, quantity)
		{
			if (definition as StorableItemDefinition == null)
			{
				Console.LogError("StoredItemInstance initialized with invalid definition!");
			}
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new StorableItemInstance(base.Definition, quantity);
		}

		public override float GetMonetaryValue()
		{
			return (base.Definition as StorableItemDefinition).BasePurchasePrice;
		}
	}
}
