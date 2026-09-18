using System.Collections;
using ScheduleOne.Economy;
using ScheduleOne.Quests;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCSignal_HandleDeal : NPCSignal
	{
		private Dealer dealer;

		private Contract contract;

		private Customer customer;

		private Coroutine handoverRoutine;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDealAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDealAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Handle deal";

		public void AssignContract(Contract c)
		{
			contract = c;
			if (contract != null)
			{
				customer = c.Customer.GetComponent<Customer>();
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDeal_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			priority = 10;
		}

		public override string GetName()
		{
			return ActionName;
		}

		public override void Started()
		{
			base.Started();
			SetDestination(GetStandPos());
		}

		public override void MinPassed()
		{
			base.MinPassed();
			if (!base.IsActive)
			{
				return;
			}
			if (contract == null || contract.QuestState != EQuestState.Active)
			{
				End();
				base.gameObject.SetActive(value: false);
				contract = null;
				base.StartedThisCycle = false;
			}
			else
			{
				if (handoverRoutine != null || npc.Movement.IsMoving)
				{
					return;
				}
				if (IsAtDestination())
				{
					if (IsCustomerReady())
					{
						BeginHandover();
					}
				}
				else
				{
					SetDestination(GetStandPos());
				}
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			SetDestination(GetStandPos());
		}

		public override void JumpTo()
		{
			base.JumpTo();
			SetDestination(GetStandPos());
		}

		public override void Interrupt()
		{
			base.Interrupt();
			npc.Movement.Stop();
			StopHandover();
		}

		public override void End()
		{
			base.End();
			StopHandover();
		}

		public override void Skipped()
		{
			base.Skipped();
		}

		private bool IsAtDestination()
		{
			return Vector3.Distance(npc.Movement.FootPosition, GetStandPos()) < 2f;
		}

		private bool IsCustomerReady()
		{
			return customer.IsAtDealLocation();
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && result != NPCMovement.WalkResult.Success)
			{
				Debug.LogWarning(npc.fullName + ": walk to location not successful");
			}
		}

		private void BeginHandover()
		{
			if (handoverRoutine == null)
			{
				handoverRoutine = StartCoroutine(Routine());
			}
			IEnumerator Routine()
			{
				npc.Movement.FaceDirection(GetStandDir());
				yield return new WaitForSeconds(2f);
				yield return new WaitUntil(() => customer.IsAtDealLocation());
				if (!dealer.RemoveContractItems(contract, customer.CustomerData.Standards.GetCorrespondingQuality(), out var items))
				{
					Console.LogWarning("Dealer does not have items for contract. Contract will still be marked as complete.");
				}
				customer.OfferDealItems(items, offeredByPlayer: false, out var _);
				npc.SetAnimationTrigger("GrabItem");
				End();
				base.gameObject.SetActive(value: false);
				contract = null;
				base.StartedThisCycle = false;
				handoverRoutine = null;
			}
		}

		private void StopHandover()
		{
			if (handoverRoutine != null)
			{
				StopCoroutine(handoverRoutine);
				handoverRoutine = null;
			}
		}

		private Vector3 GetStandPos()
		{
			if (contract == null)
			{
				return Vector3.zero;
			}
			return contract.DeliveryLocation.CustomerStandPoint.position + contract.DeliveryLocation.CustomerStandPoint.forward * 1.2f;
		}

		private Vector3 GetStandDir()
		{
			return -contract.DeliveryLocation.CustomerStandPoint.forward;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDealAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDealAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDealAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDealAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_HandleDeal_Assembly_002DCSharp_002Edll()
		{
			((NPCAction)this).Awake();
			priority = 100;
			MaxDuration = 720;
			dealer = npc as Dealer;
		}
	}
}
