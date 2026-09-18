using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ObjectScripts.Cash
{
	public class StoredItem_Cash : StoredItem
	{
		protected CashInstance cashInstance;

		[Header("References")]
		public CashStackVisuals Visuals;

		public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
		{
			base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
			cashInstance = base.item as CashInstance;
			RefreshShownBills();
			CashInstance obj = cashInstance;
			obj.onDataChanged = (Action)Delegate.Combine(obj.onDataChanged, new Action(RefreshShownBills));
		}

		private void RefreshShownBills()
		{
			Visuals.ShowAmount(cashInstance.Balance);
		}
	}
}
