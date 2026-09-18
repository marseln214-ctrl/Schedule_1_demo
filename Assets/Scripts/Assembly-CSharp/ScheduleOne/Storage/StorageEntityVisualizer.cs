using UnityEngine;

namespace ScheduleOne.Storage
{
	[RequireComponent(typeof(StorageEntity))]
	public class StorageEntityVisualizer : StorageVisualizer
	{
		private StorageEntity storageEntity;

		protected virtual void Start()
		{
			storageEntity = GetComponent<StorageEntity>();
			storageEntity.onContentsChanged.AddListener(base.QueueRefresh);
			for (int i = 0; i < storageEntity.ItemSlots.Count; i++)
			{
				AddSlot(storageEntity.ItemSlots[i]);
			}
			if (storageEntity.ItemCount > 0)
			{
				QueueRefresh();
			}
		}
	}
}
