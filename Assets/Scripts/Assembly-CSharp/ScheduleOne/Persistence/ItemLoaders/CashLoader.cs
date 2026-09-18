using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class CashLoader : ItemLoader
	{
		public override string ItemType => typeof(CashData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			CashData cashData = LoadData<CashData>(itemString);
			if (cashData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (cashData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(cashData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + cashData.ID);
				return null;
			}
			CashInstance cashInstance = new CashInstance(item, cashData.Quantity);
			cashInstance.SetBalance(cashData.CashBalance);
			return cashInstance;
		}
	}
}
