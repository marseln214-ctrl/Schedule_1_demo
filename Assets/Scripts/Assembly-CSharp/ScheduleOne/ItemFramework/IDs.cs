using System.Collections.Generic;

namespace ScheduleOne.ItemFramework
{
	public class IDs : ItemFilter
	{
		public List<string> AcceptedIDs = new List<string>();

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
