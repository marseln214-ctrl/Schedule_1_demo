using FishNet;
using ScheduleOne.Economy;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCSignal_WaitForDelivery : NPCSignal
	{
		public const float DESTINATION_THRESHOLD = 1.5f;

		public DeliveryLocation Location;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Wait for delivery";

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDelivery_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			priority = 100;
		}

		public override string GetName()
		{
			return ActionName;
		}

		public override void Started()
		{
			base.Started();
			SetDestination(Location.CustomerStandPoint.position);
		}

		public override void ActiveMinPassed()
		{
			base.ActiveMinPassed();
			if (!npc.Movement.IsMoving)
			{
				if (!IsAtDestination())
				{
					SetDestination(Location.CustomerStandPoint.position);
				}
				else
				{
					npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
				}
			}
			else if (Vector3.Distance(npc.Movement.CurrentDestination, Location.CustomerStandPoint.position) > 1.5f)
			{
				SetDestination(Location.CustomerStandPoint.position);
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
			if (InstanceFinder.IsServer)
			{
				SetDestination(Location.CustomerStandPoint.position);
			}
		}

		public override void JumpTo()
		{
			base.JumpTo();
			npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
			if (InstanceFinder.IsServer)
			{
				SetDestination(Location.CustomerStandPoint.position);
			}
		}

		public override void Interrupt()
		{
			base.Interrupt();
			npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: false);
			npc.Movement.Stop();
		}

		public override void Resume()
		{
			base.Resume();
			npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
		}

		public override void End()
		{
			npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: false);
			base.StartedThisCycle = false;
			base.End();
		}

		public override void Skipped()
		{
			base.Skipped();
			if (InstanceFinder.IsServer)
			{
				npc.Movement.Warp(Location.CustomerStandPoint.position);
			}
		}

		private bool IsAtDestination()
		{
			return Vector3.Distance(npc.Movement.FootPosition, Location.CustomerStandPoint.position) < 1.5f;
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && result == NPCMovement.WalkResult.Success)
			{
				npc.Movement.FaceDirection(Location.CustomerStandPoint.forward);
				npc.GetComponent<Customer>().SetIsAwaitingDelivery(awaiting: true);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDeliveryAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_WaitForDelivery_Assembly_002DCSharp_002Edll()
		{
			((NPCAction)this).Awake();
			priority = 1000;
			MaxDuration = 720;
		}
	}
}
