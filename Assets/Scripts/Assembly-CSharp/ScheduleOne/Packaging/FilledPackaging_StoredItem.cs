using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Packaging
{
	public class FilledPackaging_StoredItem : StoredItem
	{
		public FilledPackagingVisuals Visuals;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			(base.item as ProductItemInstance).SetupPackagingVisuals(Visuals);
		}

		public override GameObject CreateGhostModel(ItemInstance _item, Transform parent)
		{
			GameObject gameObject = base.CreateGhostModel(_item, parent);
			(_item as ProductItemInstance).SetupPackagingVisuals(gameObject.GetComponent<FilledPackaging_StoredItem>().Visuals);
			return gameObject;
		}
	}
}
