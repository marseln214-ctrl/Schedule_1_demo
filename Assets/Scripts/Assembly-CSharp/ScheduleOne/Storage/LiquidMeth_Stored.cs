using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class LiquidMeth_Stored : StoredItem
	{
		public LiquidMethVisuals Visuals;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			LiquidMethDefinition def = _item.Definition as LiquidMethDefinition;
			if (Visuals != null)
			{
				Visuals.Setup(def);
			}
		}
	}
}
