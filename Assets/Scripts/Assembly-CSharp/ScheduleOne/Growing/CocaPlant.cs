using ScheduleOne.ItemFramework;

namespace ScheduleOne.Growing
{
	public class CocaPlant : Plant
	{
		public PlantHarvestable Harvestable;

		public override ItemInstance GetHarvestedProduct(int quantity = 1)
		{
			EQuality quality = ItemQuality.GetQuality(QualityLevel);
			QualityItemInstance obj = Harvestable.Product.GetDefaultInstance(quantity) as QualityItemInstance;
			obj.Quality = quality;
			return obj;
		}
	}
}
