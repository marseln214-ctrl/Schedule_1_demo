using ScheduleOne.Packaging;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Product
{
	public class Product_Stored : StoredItem
	{
		public FilledPackagingVisuals Visuals;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			(_item as ProductItemInstance).SetupPackagingVisuals(Visuals);
		}
	}
}
