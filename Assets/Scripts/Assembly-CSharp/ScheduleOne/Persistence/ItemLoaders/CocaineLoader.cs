using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class CocaineLoader : ItemLoader
	{
		public override string ItemType => typeof(CocaineData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			CocaineData cocaineData = LoadData<CocaineData>(itemString);
			if (cocaineData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (cocaineData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(cocaineData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + cocaineData.ID);
				return null;
			}
			EQuality result;
			EQuality quality = (Enum.TryParse<EQuality>(cocaineData.Quality, out result) ? result : EQuality.Standard);
			PackagingDefinition packaging = null;
			if (cocaineData.PackagingID != string.Empty)
			{
				ItemDefinition item2 = Registry.GetItem(cocaineData.PackagingID);
				if (item != null)
				{
					packaging = item2 as PackagingDefinition;
				}
			}
			return new CocaineInstance(item, cocaineData.Quantity, quality, packaging);
		}
	}
}
