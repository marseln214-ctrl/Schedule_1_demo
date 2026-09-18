using System.Collections.Generic;

namespace ScheduleOne.ItemFramework
{
	public class ItemFilter_ID : ItemFilter
	{
		public List<string> AcceptedIDs = new List<string>();

		public ItemFilter_ID(List<string> acceptedIDs)
		{
			AcceptedIDs = acceptedIDs;
		}

		public override bool DoesItemMatchFilter(ItemInstance instance)
		{
			if (!AcceptedIDs.Contains(instance.ID))
			{
				return false;
			}
			return base.DoesItemMatchFilter(instance);
		}
	}
}
