using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class WeedLoader : ItemLoader
	{
		public override string ItemType => typeof(WeedData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			WeedData weedData = LoadData<WeedData>(itemString);
			if (weedData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (weedData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(weedData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + weedData.ID);
				return null;
			}
			EQuality result;
			EQuality quality = (Enum.TryParse<EQuality>(weedData.Quality, out result) ? result : EQuality.Standard);
			PackagingDefinition packaging = null;
			if (weedData.PackagingID != string.Empty)
			{
				ItemDefinition item2 = Registry.GetItem(weedData.PackagingID);
				if (item != null)
				{
					packaging = item2 as PackagingDefinition;
				}
			}
			return new WeedInstance(item, weedData.Quantity, quality, packaging);
		}
	}
}
