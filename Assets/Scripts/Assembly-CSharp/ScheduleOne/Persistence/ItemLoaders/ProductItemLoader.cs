using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class ProductItemLoader : ItemLoader
	{
		public override string ItemType => typeof(ProductItemData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			ProductItemData productItemData = LoadData<ProductItemData>(itemString);
			if (productItemData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (productItemData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(productItemData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + productItemData.ID);
				return null;
			}
			EQuality result;
			EQuality quality = (Enum.TryParse<EQuality>(productItemData.Quality, out result) ? result : EQuality.Standard);
			PackagingDefinition packaging = null;
			if (productItemData.PackagingID != string.Empty)
			{
				ItemDefinition item2 = Registry.GetItem(productItemData.PackagingID);
				if (item != null)
				{
					packaging = item2 as PackagingDefinition;
				}
			}
			return new ProductItemInstance(item, productItemData.Quantity, quality, packaging);
		}
	}
}
