using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.Product;

namespace ScheduleOne.StationFramework
{
	public class MethStationItem : StationItem
	{
		public FilledPackagingVisuals[] Visuals;

		public override void Initialize(StorableItemDefinition itemDefinition)
		{
			base.Initialize(itemDefinition);
			MethInstance methInstance = ((MethDefinition)itemDefinition).GetDefaultInstance() as MethInstance;
			FilledPackagingVisuals[] visuals = Visuals;
			foreach (FilledPackagingVisuals visuals2 in visuals)
			{
				methInstance.SetupPackagingVisuals(visuals2);
			}
		}
	}
}
