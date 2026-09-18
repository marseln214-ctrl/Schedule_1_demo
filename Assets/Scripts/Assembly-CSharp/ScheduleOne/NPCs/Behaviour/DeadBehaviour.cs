using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class DeadBehaviour : Behaviour
	{
		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public bool IsInMedicalCenter => base.Npc.CurrentBuilding == Singleton<ScheduleOne.Map.Map>.Instance.MedicalCentre;

		private void Start()
		{
			ScheduleOne.GameTime.TimeManager.onSleepStart = (Action)Delegate.Combine(ScheduleOne.GameTime.TimeManager.onSleepStart, new Action(SleepStart));
		}

		private void OnDestroy()
		{
			ScheduleOne.GameTime.TimeManager.onSleepStart = (Action)Delegate.Remove(ScheduleOne.GameTime.TimeManager.onSleepStart, new Action(SleepStart));
		}

		protected override void Begin()
		{
			base.Begin();
			base.Npc.behaviour.RagdollBehaviour.Disable();
			if (Singleton<LoadManager>.Instance.IsLoading)
			{
				EnterMedicalCentre();
			}
			else
			{
				base.Npc.Movement.ActivateRagdoll(Vector3.zero, Vector3.zero, 0f);
				base.Npc.Movement.SetRagdollDraggable(draggable: true);
			}
			base.Npc.dialogueHandler.HideWorldspaceDialogue();
			base.Npc.awareness.SetAwarenessActive(active: false);
			base.Npc.Avatar.EmotionManager.ClearOverrides();
			base.Npc.Avatar.EmotionManager.AddEmotionOverride("Sleeping", "Dead", 0f, 20);
			base.Npc.PlayVO(EVOLineType.Die);
		}

		public override void ActiveMinPass()
		{
			base.ActiveMinPass();
			if (!IsInMedicalCenter && !base.Npc.Avatar.Ragdolled)
			{
				if (base.Npc.Movement.IsMoving)
				{
					base.Npc.Movement.Stop();
				}
				EnterMedicalCentre();
			}
		}

		private void SleepStart()
		{
			if (base.Active && !IsInMedicalCenter)
			{
				EnterMedicalCentre();
			}
		}

		private void EnterMedicalCentre()
		{
			Console.Log(base.Npc.fullName + " entering medical center");
			base.Npc.Movement.DeactivateRagdoll();
			base.Npc.Movement.SetRagdollDraggable(draggable: false);
			base.Npc.EnterBuilding(null, Singleton<ScheduleOne.Map.Map>.Instance.MedicalCentre.GUID.ToString(), 0);
		}

		protected override void End()
		{
			base.End();
			base.Npc.awareness.SetAwarenessActive(active: true);
			base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("Dead");
			base.Npc.Movement.DeactivateRagdoll();
			base.Npc.Movement.SetRagdollDraggable(draggable: false);
			if (IsInMedicalCenter)
			{
				base.Npc.ExitBuilding();
			}
		}

		public override void Disable()
		{
			base.Disable();
			End();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EDeadBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
