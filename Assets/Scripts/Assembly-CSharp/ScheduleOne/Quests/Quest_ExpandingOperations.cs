using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class Quest_ExpandingOperations : Quest
	{
		public QuestEntry SetUpGrowTentsEntry;

		public QuestEntry ReachCustomersEntry;

		protected override void MinPass()
		{
			base.MinPass();
			if (base.QuestState == EQuestState.Active)
			{
				int num = Mathf.Clamp(Mathf.RoundToInt(NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Sweatshop_Pots")) - 2, 0, 2);
				SetUpGrowTentsEntry.SetEntryTitle("Set up 2 more grow tents (" + num + "/2)");
				if (num >= 2 && SetUpGrowTentsEntry.State != EQuestState.Completed)
				{
					SetUpGrowTentsEntry.Complete();
				}
				int count = Customer.UnlockedCustomers.Count;
				ReachCustomersEntry.SetEntryTitle("Reach 10 customers (" + count + "/10)");
				if (count >= 10 && ReachCustomersEntry.State != EQuestState.Completed)
				{
					ReachCustomersEntry.Complete();
				}
			}
		}
	}
}
