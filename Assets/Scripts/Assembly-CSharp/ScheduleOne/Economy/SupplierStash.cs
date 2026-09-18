using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.Economy
{
	public class SupplierStash : MonoBehaviour
	{
		public string locationDescription = "behind the X";

		[Header("References")]
		public Supplier Supplier;

		public StorageEntity Storage;

		public InteractableObject IntObj;

		public OptimizedLight Light;

		public POI StashPoI;

		public float CashAmount { get; private set; }

		protected virtual void Awake()
		{
			IntObj.SetMessage("View " + Supplier.fullName + "'s stash");
			IntObj.enabled = Supplier.RelationData.Unlocked;
			NPCRelationData relationData = Supplier.RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, (Action<NPCRelationData.EUnlockType, bool>)delegate
			{
				SupplierUnlocked();
			});
			Storage.StorageEntityName = Supplier.fullName + "'s Stash";
			Interacted();
			RecalculateCash();
			Storage.onContentsChanged.AddListener(RecalculateCash);
			StashPoI.enabled = false;
			StashPoI.SetMainText(Supplier.fullName + "'s Stash");
		}

		protected virtual void Start()
		{
			UpdateDeadDrop();
			IntObj.onInteractStart.AddListener(Interacted);
			Storage.onContentsChanged.AddListener(UpdateDeadDrop);
		}

		private void SupplierUnlocked()
		{
			StashPoI.enabled = true;
			IntObj.enabled = true;
		}

		private void RecalculateCash()
		{
			float num = 0f;
			for (int i = 0; i < Storage.ItemSlots.Count; i++)
			{
				if (Storage.ItemSlots[i] != null && Storage.ItemSlots[i].ItemInstance != null && Storage.ItemSlots[i].ItemInstance is CashInstance)
				{
					num += (Storage.ItemSlots[i].ItemInstance as CashInstance).Balance;
				}
			}
			CashAmount = num;
		}

		private void Interacted()
		{
			Storage.StorageEntitySubtitle = "You owe " + Supplier.fullName + " <color=#54E717>" + MoneyManager.FormatAmount(Supplier.Debt) + "</color>. Insert cash and exit stash to pay off your debt";
		}

		public void RemoveCash(float amount)
		{
			float num = amount;
			for (int i = 0; i < Storage.SlotCount; i++)
			{
				if (num <= 0f)
				{
					break;
				}
				if (Storage.ItemSlots[i].ItemInstance != null && Storage.ItemSlots[i].ItemInstance is CashInstance)
				{
					CashInstance cashInstance = Storage.ItemSlots[i].ItemInstance as CashInstance;
					float num2 = Mathf.Min(num, cashInstance.Balance);
					cashInstance.ChangeBalance(0f - num2);
					if (cashInstance.Balance > 0f)
					{
						Storage.ItemSlots[i].SetStoredItem(cashInstance);
					}
					num -= num2;
				}
			}
		}

		private void UpdateDeadDrop()
		{
			Light.Enabled = Storage.ItemCount > 0;
		}
	}
}
