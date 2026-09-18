using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;

namespace ScheduleOne.ItemFramework
{
	public class IntegerItemInstance : StorableItemInstance
	{
		public int Value;

		public IntegerItemInstance()
		{
		}

		public IntegerItemInstance(ItemDefinition definition, int quantity, int value)
			: base(definition, quantity)
		{
			Value = value;
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new IntegerItemInstance(base.Definition, quantity, Value);
		}

		public void ChangeValue(int change)
		{
			Value += change;
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}

		public void SetValue(int value)
		{
			Value = value;
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}

		public override ItemData GetItemData()
		{
			return new IntegerItemData(ID, Quantity, Value);
		}
	}
}
