using System.Collections.Generic;
using ScheduleOne.Product;

namespace ScheduleOne.ItemFramework
{
	public class ItemFilter_PackagedProduct : ItemFilter_Category
	{
		public ItemFilter_PackagedProduct()
			: base(new List<EItemCategory> { EItemCategory.Product })
		{
		}

		public override bool DoesItemMatchFilter(ItemInstance instance)
		{
			if (!(instance is ProductItemInstance productItemInstance))
			{
				return false;
			}
			if (productItemInstance.AppliedPackaging == null)
			{
				return false;
			}
			return base.DoesItemMatchFilter(instance);
		}
	}
}
