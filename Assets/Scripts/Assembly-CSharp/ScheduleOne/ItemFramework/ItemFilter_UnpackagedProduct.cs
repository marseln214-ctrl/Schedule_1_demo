using System.Collections.Generic;
using ScheduleOne.Product;

namespace ScheduleOne.ItemFramework
{
	public class ItemFilter_UnpackagedProduct : ItemFilter_Category
	{
		public ItemFilter_UnpackagedProduct()
			: base(new List<EItemCategory> { EItemCategory.Product })
		{
		}

		public override bool DoesItemMatchFilter(ItemInstance instance)
		{
			if (!(instance is ProductItemInstance productItemInstance))
			{
				return false;
			}
			if (productItemInstance.AppliedPackaging != null)
			{
				return false;
			}
			return base.DoesItemMatchFilter(instance);
		}
	}
}
