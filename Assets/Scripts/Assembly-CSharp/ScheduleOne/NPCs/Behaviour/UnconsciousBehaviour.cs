using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class UnconsciousBehaviour : Behaviour
	{
		public const float SnoreInterval = 6f;

		public ParticleSystem Particles;

		public bool PlaySnoreSounds = true;

		private float timeOnLastSnore;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted;

		protected override void Begin()
		{
			base.Begin();
			base.Npc.behaviour.RagdollBehaviour.Disable();
			base.Npc.Movement.ActivateRagdoll(Vector3.zero, Vector3.zero, 0f);
			base.Npc.Movement.SetRagdollDraggable(draggable: true);
			base.Npc.dialogueHandler.HideWorldspaceDialogue();
			base.Npc.awareness.SetAwarenessActive(active: false);
			base.Npc.Avatar.EmotionManager.ClearOverrides();
			base.Npc.Avatar.EmotionManager.AddEmotionOverride("Sleeping", "Dead", 0f, 20);
			Particles.Play();
			base.Npc.PlayVO(EVOLineType.Die);
			timeOnLastSnore = Time.time;
		}

		protected override void End()
		{
			base.End();
			base.Npc.awareness.SetAwarenessActive(active: true);
			base.Npc.Avatar.EmotionManager.RemoveEmotionOverride("Dead");
			base.Npc.Movement.DeactivateRagdoll();
			base.Npc.Movement.SetRagdollDraggable(draggable: false);
			Particles.Stop();
		}

		public override void ActiveMinPass()
		{
			base.ActiveMinPass();
			if (PlaySnoreSounds && Time.time - timeOnLastSnore > 6f)
			{
				base.Npc.PlayVO(EVOLineType.Snore);
				timeOnLastSnore = Time.time;
			}
		}

		public override void Disable()
		{
			base.Disable();
			End();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EUnconsciousBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
