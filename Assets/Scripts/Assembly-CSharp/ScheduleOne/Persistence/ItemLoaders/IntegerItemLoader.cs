using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class IntegerItemLoader : ItemLoader
	{
		public override string ItemType => typeof(IntegerItemData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			IntegerItemData integerItemData = LoadData<IntegerItemData>(itemString);
			if (integerItemData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (integerItemData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(integerItemData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + integerItemData.ID);
				return null;
			}
			return new IntegerItemInstance(item, integerItemData.Quantity, integerItemData.Value);
		}
	}
}
