using ScheduleOne.Economy;

namespace ScheduleOne.Quests
{
	public class Quest_MovingUp : Quest
	{
		public QuestEntry ReachCustomersEntry;

		protected override void MinPass()
		{
			base.MinPass();
			if (ReachCustomersEntry.State == EQuestState.Active)
			{
				int count = Customer.UnlockedCustomers.Count;
				ReachCustomersEntry.SetEntryTitle("Unlock 10 customers (" + count + "/10)");
				if (count >= 10 && ReachCustomersEntry.State != EQuestState.Completed)
				{
					ReachCustomersEntry.Complete();
				}
			}
		}
	}
}
