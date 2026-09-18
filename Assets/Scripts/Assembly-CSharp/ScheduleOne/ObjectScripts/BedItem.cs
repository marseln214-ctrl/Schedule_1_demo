using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Storage;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class BedItem : PlaceableStorageEntity
	{
		public Bed Bed;

		public StorageEntity Storage;

		public GameObject Briefcase;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			Bed.onAssignedEmployeeChanged.AddListener(UpdateBriefcase);
			UpdateBriefcase();
		}

		public static bool IsBedValid(BuildableItem obj, out string reason)
		{
			reason = string.Empty;
			if (obj is BedItem)
			{
				BedItem bedItem = obj as BedItem;
				if (bedItem.Bed.AssignedEmployee != null)
				{
					reason = "Already assigned to " + bedItem.Bed.AssignedEmployee.fullName;
					return false;
				}
				return true;
			}
			return false;
		}

		private void UpdateBriefcase()
		{
			Briefcase.gameObject.SetActive(Bed.AssignedEmployee != null || Storage.ItemCount > 0);
			if (Bed.AssignedEmployee != null)
			{
				Storage.StorageEntityName = Bed.AssignedEmployee.FirstName + "'s Briefcase";
				Storage.StorageEntitySubtitle = Bed.AssignedEmployee.fullName + " will draw " + (Bed.AssignedEmployee.IsMale ? "his" : "her") + " daily wage from this briefcase.";
			}
			else
			{
				Storage.StorageEntityName = "Briefcase";
				Storage.StorageEntitySubtitle = string.Empty;
			}
		}

		public float GetCashSum()
		{
			float num = 0f;
			foreach (ItemSlot itemSlot in Storage.ItemSlots)
			{
				if (itemSlot.ItemInstance != null && itemSlot.ItemInstance is CashInstance)
				{
					num += (itemSlot.ItemInstance as CashInstance).Balance;
				}
			}
			return num;
		}

		public void RemoveCash(float amount)
		{
			foreach (ItemSlot itemSlot in Storage.ItemSlots)
			{
				if (amount <= 0f)
				{
					break;
				}
				if (itemSlot.ItemInstance != null && itemSlot.ItemInstance is CashInstance)
				{
					CashInstance cashInstance = itemSlot.ItemInstance as CashInstance;
					float num = Mathf.Min(amount, cashInstance.Balance);
					cashInstance.ChangeBalance(0f - num);
					itemSlot.ReplicateStoredInstance();
					amount -= num;
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBedItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
