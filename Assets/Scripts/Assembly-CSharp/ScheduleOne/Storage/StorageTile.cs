using System;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class StorageTile : MonoBehaviour
	{
		public int x;

		public int y;

		[SerializeField]
		public StorageGrid ownerGrid;

		public Action onOccupantChanged;

		public StorageGrid _ownerGrid => ownerGrid;

		public StoredItem occupant { get; protected set; }

		public void InitializeStorageTile(int _x, int _y, float _available_Offset, StorageGrid _ownerGrid)
		{
			x = _x;
			y = _y;
			ownerGrid = _ownerGrid;
		}

		public void SetOccupant(StoredItem occ)
		{
			if (occ != null && occupant != null)
			{
				Console.LogWarning("SetOccupant called by there is an existing occupant. Existing occupant should be dealt with before calling this.");
			}
			occupant = occ;
			if (onOccupantChanged != null)
			{
				onOccupantChanged();
			}
		}
	}
}
