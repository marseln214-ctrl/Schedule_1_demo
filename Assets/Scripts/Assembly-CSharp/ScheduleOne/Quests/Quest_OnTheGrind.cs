using ScheduleOne.DevUtilities;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class Quest_OnTheGrind : Quest
	{
		public QuestEntry CompleteDealsEntry;

		protected override void MinPass()
		{
			base.MinPass();
			int num = Mathf.RoundToInt(NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Completed_Contracts_Count"));
			if (CompleteDealsEntry.State == EQuestState.Active)
			{
				CompleteDealsEntry.SetEntryTitle("Complete 3 deals (" + num + "/3)");
			}
		}
	}
}
