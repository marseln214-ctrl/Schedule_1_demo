using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCSignal : NPCAction
	{
		public int MaxDuration = 60;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignalAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignalAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Signal";

		public bool StartedThisCycle { get; protected set; }

		public override string GetName()
		{
			return ActionName;
		}

		public override void ActiveUpdate()
		{
			base.ActiveUpdate();
		}

		public override string GetTimeDescription()
		{
			return ScheduleOne.GameTime.TimeManager.Get12HourTime(StartTime);
		}

		public override int GetEndTime()
		{
			return ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(StartTime, MaxDuration);
		}

		public override void Started()
		{
			base.Started();
			StartedThisCycle = true;
		}

		public override void LateStarted()
		{
			base.LateStarted();
			StartedThisCycle = true;
		}

		public override bool ShouldStart()
		{
			if (StartedThisCycle)
			{
				return false;
			}
			return base.ShouldStart();
		}

		public override void Interrupt()
		{
			StartedThisCycle = false;
			base.Interrupt();
		}

		public override void MinPassed()
		{
			base.MinPassed();
			if (StartedThisCycle && !NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, GetEndTime()))
			{
				StartedThisCycle = false;
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignalAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignalAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignalAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignalAssembly_002DCSharp_002Edll_Excuted = true;
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
