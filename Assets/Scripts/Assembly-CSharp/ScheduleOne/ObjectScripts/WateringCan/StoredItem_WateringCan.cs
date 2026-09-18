using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.WateringCan
{
	public class StoredItem_WateringCan : StoredItem
	{
		public WateringCanVisuals Visuals;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			if (_item is WateringCanInstance wateringCanInstance)
			{
				Visuals.SetFillLevel(wateringCanInstance.CurrentFillAmount / 15f);
			}
		}
	}
}
