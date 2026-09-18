namespace ScheduleOne.NPCs.Behaviour
{
	public class StationaryBehaviour : Behaviour
	{
		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStationaryBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStationaryBehaviourAssembly_002DCSharp_002Edll_Excuted;

		protected override void Begin()
		{
			base.Begin();
			base.Npc.Movement.Stop();
		}

		protected override void Resume()
		{
			base.Resume();
			base.Npc.Movement.Stop();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStationaryBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStationaryBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStationaryBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStationaryBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
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
