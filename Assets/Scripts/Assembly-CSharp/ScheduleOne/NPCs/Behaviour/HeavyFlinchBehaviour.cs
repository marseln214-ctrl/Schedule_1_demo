using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class HeavyFlinchBehaviour : Behaviour
	{
		public const float FLINCH_DURATION = 1.25f;

		private float remainingFlinchTime;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHeavyFlinchBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHeavyFlinchBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (remainingFlinchTime > 0f)
			{
				remainingFlinchTime = Mathf.Clamp(remainingFlinchTime -= Time.deltaTime, 0f, 1.25f);
			}
			if (remainingFlinchTime <= 0f)
			{
				Disable_Networked(null);
			}
		}

		public override void Disable()
		{
			base.Disable();
			End();
		}

		public void Flinch()
		{
			remainingFlinchTime += 1.25f;
			if (!base.Enabled)
			{
				Enable_Networked(null);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHeavyFlinchBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EHeavyFlinchBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHeavyFlinchBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EHeavyFlinchBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
