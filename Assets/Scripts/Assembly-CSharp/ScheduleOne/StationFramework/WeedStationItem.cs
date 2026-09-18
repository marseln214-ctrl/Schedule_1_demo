using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.Product;

namespace ScheduleOne.StationFramework
{
	public class WeedStationItem : StationItem
	{
		public FilledPackagingVisuals[] Visuals;

		public override void Initialize(StorableItemDefinition itemDefinition)
		{
			base.Initialize(itemDefinition);
			WeedInstance weedInstance = ((WeedDefinition)itemDefinition).GetDefaultInstance() as WeedInstance;
			FilledPackagingVisuals[] visuals = Visuals;
			foreach (FilledPackagingVisuals visuals2 in visuals)
			{
				weedInstance.SetupPackagingVisuals(visuals2);
			}
		}
	}
}
