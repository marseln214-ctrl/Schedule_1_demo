using System;
using FishNet.Object;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.Tools;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.NPCs
{
	public class NPCAnimation : NetworkBehaviour
	{
		[Header("References")]
		public ScheduleOne.AvatarFramework.Avatar Avatar;

		[SerializeField]
		protected AvatarAnimation anim;

		[SerializeField]
		protected NPCMovement movement;

		protected NPC npc;

		[SerializeField]
		protected SmoothedVelocityCalculator velocityCalculator;

		[Header("Settings")]
		public AnimationCurve WalkMapCurve;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted;

		private void Start()
		{
			npc = GetComponent<NPC>();
			NPC nPC = npc;
			nPC.onExitVehicle = (Action<LandVehicle>)Delegate.Combine(nPC.onExitVehicle, (Action<LandVehicle>)delegate
			{
				ResetVelocityCalculations();
			});
		}

		protected virtual void LateUpdate()
		{
			if (npc.isVisible)
			{
				UpdateMovementAnimation();
			}
		}

		protected virtual void UpdateMovementAnimation()
		{
			Vector3 vector = Avatar.transform.InverseTransformVector(velocityCalculator.Velocity) / 8f;
			anim.SetDirection(WalkMapCurve.Evaluate(Mathf.Abs(vector.z)) * Mathf.Sign(vector.z));
			anim.SetStrafe(WalkMapCurve.Evaluate(Mathf.Abs(vector.x)) * Mathf.Sign(vector.x));
		}

		public virtual void SetRagdollActive(bool active)
		{
			Avatar.SetRagdollPhysicsEnabled(active);
		}

		public void ResetVelocityCalculations()
		{
			velocityCalculator.FlushBuffer();
		}

		public void StandupStart()
		{
			movement.SpeedController.AddSpeedControl(new NPCSpeedController.SpeedControl("ragdollstandup", 100, 0f));
		}

		public void StandupDone()
		{
			movement.SpeedController.RemoveSpeedControl("ragdollstandup");
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCAnimationAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
