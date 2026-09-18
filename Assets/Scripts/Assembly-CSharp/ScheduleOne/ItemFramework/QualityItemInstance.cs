using System;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;

namespace ScheduleOne.ItemFramework
{
	[Serializable]
	public class QualityItemInstance : StorableItemInstance
	{
		public EQuality Quality = EQuality.Standard;

		public QualityItemInstance()
		{
		}

		public QualityItemInstance(ItemDefinition definition, int quantity, EQuality quality)
			: base(definition, quantity)
		{
			base.definition = definition;
			Quantity = quantity;
			ID = definition.ID;
			Quality = quality;
		}

		public override bool CanStackWith(ItemInstance other, bool checkQuantities = true)
		{
			if (!(other is QualityItemInstance qualityItemInstance) || qualityItemInstance.Quality != Quality)
			{
				return false;
			}
			return base.CanStackWith(other, checkQuantities);
		}

		public override ItemInstance GetCopy(int overrideQuantity = -1)
		{
			int quantity = Quantity;
			if (overrideQuantity != -1)
			{
				quantity = overrideQuantity;
			}
			return new QualityItemInstance(base.Definition, quantity, Quality);
		}

		public override ItemData GetItemData()
		{
			return new QualityItemData(ID, Quantity, Quality.ToString());
		}

		public void SetQuality(EQuality quality)
		{
			Quality = quality;
			if (onDataChanged != null)
			{
				onDataChanged();
			}
		}
	}
}
