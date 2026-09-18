using FishNet;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class IdleBehaviour : Behaviour
	{
		public Transform IdlePoint;

		private bool facingDir;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted;

		protected override void Begin()
		{
			base.Begin();
		}

		protected override void Resume()
		{
			base.Resume();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer || IdlePoint == null)
			{
				return;
			}
			if (!base.Npc.Movement.IsMoving)
			{
				if (base.Npc.Movement.IsAsCloseAsPossible(IdlePoint.position))
				{
					if (!facingDir)
					{
						facingDir = true;
						base.Npc.Movement.FaceDirection(IdlePoint.forward);
					}
				}
				else
				{
					facingDir = false;
					base.Npc.Movement.SetDestination(IdlePoint.position);
				}
			}
			else
			{
				facingDir = false;
			}
		}

		protected override void Pause()
		{
			base.Pause();
			facingDir = false;
			if (InstanceFinder.IsServer)
			{
				base.Npc.Movement.Stop();
			}
		}

		protected override void End()
		{
			base.End();
			facingDir = false;
			if (InstanceFinder.IsServer)
			{
				base.Npc.Movement.Stop();
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EIdleBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
