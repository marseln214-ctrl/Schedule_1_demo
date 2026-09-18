using ScheduleOne.Clothing;

namespace ScheduleOne.ItemFramework
{
	public class ItemFilter_ClothingSlot : ItemFilter
	{
		public EClothingSlot SlotType { get; private set; }

		public ItemFilter_ClothingSlot(EClothingSlot slot)
		{
			SlotType = slot;
		}

		public override bool DoesItemMatchFilter(ItemInstance instance)
		{
			if (!(instance is ClothingInstance clothingInstance))
			{
				return false;
			}
			ClothingDefinition clothingDefinition = clothingInstance.Definition as ClothingDefinition;
			if (clothingDefinition == null || clothingDefinition.Slot != SlotType)
			{
				return false;
			}
			return base.DoesItemMatchFilter(instance);
		}
	}
}
