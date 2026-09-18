using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Product;
using ScheduleOne.Variables;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.CharacterClasses
{
	public class Benji : Dealer
	{
		public UnityEvent onRecruitmentRequested;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EBenjiAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EBenjiAssembly_002DCSharp_002Edll_Excuted;

		protected override void MinPass()
		{
			base.MinPass();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Benji_FriendUnlocked", RelationData.IsMutuallyKnown().ToString());
			int num = 0;
			for (int i = 0; i < base.Inventory.ItemSlots.Count; i++)
			{
				if (base.Inventory.ItemSlots[i].Quantity != 0 && base.Inventory.ItemSlots[i].ItemInstance is WeedInstance)
				{
					num += (base.Inventory.ItemSlots[i].ItemInstance as WeedInstance).Amount * base.Inventory.ItemSlots[i].Quantity;
				}
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Benji_WeedCount", num.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Benji_CashAmount", base.Cash.ToString());
		}

		protected override void AddCustomer(Customer customer)
		{
			base.AddCustomer(customer);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Benji_CustomerCount", AssignedCustomers.Count.ToString());
		}

		public override void RemoveCustomer(Customer customer)
		{
			base.RemoveCustomer(customer);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Benji_CustomerCount", AssignedCustomers.Count.ToString());
		}

		protected override void RecruitmentRequested()
		{
			base.RecruitmentRequested();
			if (onRecruitmentRequested != null)
			{
				onRecruitmentRequested.Invoke();
			}
		}

		protected override void UpdatePotentialDealerPoI()
		{
			base.UpdatePotentialDealerPoI();
			base.potentialDealerPoI.enabled = false;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EBenjiAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ECharacterClasses_002EBenjiAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EBenjiAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ECharacterClasses_002EBenjiAssembly_002DCSharp_002Edll_Excuted = true;
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
