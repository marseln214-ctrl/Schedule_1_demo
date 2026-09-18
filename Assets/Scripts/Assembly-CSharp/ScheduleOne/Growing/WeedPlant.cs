using ScheduleOne.ItemFramework;

namespace ScheduleOne.Growing
{
	public class WeedPlant : Plant
	{
		public PlantHarvestable BranchPrefab;

		public override ItemInstance GetHarvestedProduct(int quantity = 1)
		{
			EQuality quality = ItemQuality.GetQuality(QualityLevel);
			QualityItemInstance obj = BranchPrefab.Product.GetDefaultInstance(quantity) as QualityItemInstance;
			obj.Quality = quality;
			return obj;
		}
	}
}
