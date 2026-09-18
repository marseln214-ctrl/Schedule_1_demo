using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;

namespace ScheduleOne.Clothing
{
	[Serializable]
	public class ClothingInstance : StorableItemInstance
	{
		public EClothingColor Color;

		public override string Name => base.Name + ((Color != 0) ? (" (" + Color.GetLabel() + ")") : string.Empty);

		public ClothingInstance()
		{
		}

		public ClothingInstance(ItemDefinition definition, int quantity, EClothingColor color)
			: base(definition, quantity)
		{
			Color = color;
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new ClothingInstance(base.Definition, quantity, Color);
		}

		public override ItemData GetItemData()
		{
			return new ClothingData(ID, Quantity, Color);
		}
	}
}
