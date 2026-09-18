using ScheduleOne.Product;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class Meth_Stored : StoredItem
	{
		public MethVisuals Visuals;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			if (_item is MethInstance methInstance)
			{
				Visuals.Setup(methInstance.Definition as MethDefinition);
			}
		}
	}
}
