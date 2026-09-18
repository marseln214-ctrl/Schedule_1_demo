using ScheduleOne.Clothing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class ClothingLoader : ItemLoader
	{
		public override string ItemType => typeof(ClothingData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			ClothingData clothingData = LoadData<ClothingData>(itemString);
			if (clothingData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (clothingData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(clothingData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + clothingData.ID);
				return null;
			}
			return new ClothingInstance(item, clothingData.Quantity, clothingData.Color);
		}
	}
}
