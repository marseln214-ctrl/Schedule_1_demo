using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Persistence.ItemLoaders
{
	public class QualityItemLoader : ItemLoader
	{
		public override string ItemType => typeof(QualityItemData).Name;

		public override ItemInstance LoadItem(string itemString)
		{
			QualityItemData qualityItemData = LoadData<QualityItemData>(itemString);
			if (qualityItemData == null)
			{
				Console.LogWarning("Failed loading item data from " + itemString);
				return null;
			}
			if (qualityItemData.ID == string.Empty)
			{
				return null;
			}
			ItemDefinition item = Registry.GetItem(qualityItemData.ID);
			if (item == null)
			{
				Console.LogWarning("Failed to find item definition for " + qualityItemData.ID);
				return null;
			}
			EQuality result;
			EQuality quality = (Enum.TryParse<EQuality>(qualityItemData.Quality, out result) ? result : EQuality.Standard);
			return new QualityItemInstance(item, qualityItemData.Quantity, quality);
		}
	}
}
