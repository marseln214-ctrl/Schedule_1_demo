using ScheduleOne.ItemFramework;
using ScheduleOne.Packaging;
using ScheduleOne.Product;

namespace ScheduleOne.StationFramework
{
	public class CocaineStationItem : StationItem
	{
		public FilledPackagingVisuals[] Visuals;

		public override void Initialize(StorableItemDefinition itemDefinition)
		{
			base.Initialize(itemDefinition);
			CocaineInstance cocaineInstance = ((CocaineDefinition)itemDefinition).GetDefaultInstance() as CocaineInstance;
			FilledPackagingVisuals[] visuals = Visuals;
			foreach (FilledPackagingVisuals visuals2 in visuals)
			{
				cocaineInstance.SetupPackagingVisuals(visuals2);
			}
		}
	}
}
