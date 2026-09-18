using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class WateringCanLoader : ItemLoader
	{
		public override string ItemType => typeof(WateringCanData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			WateringCanData wateringCanData = LoadData<WateringCanData>(itemString);
			if (wateringCanData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (wateringCanData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(wateringCanData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + wateringCanData.ID);
				return null;
			}
			return new WateringCanInstance(item, wateringCanData.Quantity, wateringCanData.CurrentFillAmount);
		}
	}
}
