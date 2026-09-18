using System;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Variables;

namespace ScheduleOne.Quests
{
	public class Quest_DownToBusiness : Quest
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onDayPass = (Action)Delegate.Combine(instance.onDayPass, new Action(DayPass));
		}

		private void DayPass()
		{
			if (InstanceFinder.IsServer && base.QuestState == EQuestState.Completed)
			{
				float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Days_Since_Tutorial_Completed");
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Days_Since_Tutorial_Completed", (value + 1f).ToString());
			}
		}
	}
}
