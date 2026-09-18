using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCEvent : NPCAction
	{
		public int Duration = 60;

		public int EndTime;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Event";

		[Button]
		public void ApplyDuration()
		{
			Debug.Log("Applying duration");
			EndTime = ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(StartTime, Duration);
			GetComponentInParent<NPCScheduleManager>().InitializeActions();
		}

		[Button]
		public void ApplyEndTime()
		{
			if (EndTime > StartTime)
			{
				Debug.Log("Set duration");
				Duration = ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(EndTime) - ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(StartTime);
			}
			else
			{
				Debug.Log("Set duration");
				Duration = 1440 - ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(StartTime) + ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(EndTime);
			}
			GetComponentInParent<NPCScheduleManager>().InitializeActions();
		}

		public override void ActiveMinPassed()
		{
			base.ActiveMinPassed();
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime == GetEndTime())
			{
				End();
			}
		}

		public override void PendingMinPassed()
		{
			base.PendingMinPassed();
		}

		public override string GetName()
		{
			return ActionName;
		}

		public override string GetTimeDescription()
		{
			return ScheduleOne.GameTime.TimeManager.Get12HourTime(StartTime) + " - " + ScheduleOne.GameTime.TimeManager.Get12HourTime(GetEndTime());
		}

		public override int GetEndTime()
		{
			return ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(StartTime, Duration);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCEventAssembly_002DCSharp_002Edll_Excuted = true;
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
