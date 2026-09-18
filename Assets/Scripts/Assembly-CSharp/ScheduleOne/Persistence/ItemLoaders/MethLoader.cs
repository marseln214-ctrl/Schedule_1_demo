using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class MethLoader : ItemLoader
	{
		public override string ItemType => typeof(MethData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			MethData methData = LoadData<MethData>(itemString);
			if (methData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (methData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(methData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + methData.ID);
				return null;
			}
			EQuality result;
			EQuality quality = (Enum.TryParse<EQuality>(methData.Quality, out result) ? result : EQuality.Standard);
			PackagingDefinition packaging = null;
			if (methData.PackagingID != string.Empty)
			{
				ItemDefinition item2 = Registry.GetItem(methData.PackagingID);
				if (item != null)
				{
					packaging = item2 as PackagingDefinition;
				}
			}
			return new MethInstance(item, methData.Quantity, quality, packaging);
		}
	}
}
