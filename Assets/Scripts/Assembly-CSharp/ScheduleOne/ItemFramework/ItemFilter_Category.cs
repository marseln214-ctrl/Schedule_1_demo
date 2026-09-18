using System.Collections.Generic;

namespace ScheduleOne.ItemFramework
{
	public class ItemFilter_Category : ItemFilter
	{
		public List<EItemCategory> AcceptedCategories = new List<EItemCategory>();

		public ItemFilter_Category(List<EItemCategory> acceptedCategories)
		{
			AcceptedCategories = acceptedCategories;
		}

		public override bool DoesItemMatchFilter(ItemInstance instance)
		{
			if (!AcceptedCategories.Contains(instance.Category))
			{
				return false;
			}
			return base.DoesItemMatchFilter(instance);
		}
	}
}
