using FishNet;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;

namespace ScheduleOne.Quests
{
	public class Quest_CleanCash : Quest
	{
		public QuestEntry BuyBusinessEntry;

		public QuestEntry GoToBusinessEntry;

		protected override void MinPass()
		{
			base.MinPass();
			if (base.QuestState == EQuestState.Inactive && InstanceFinder.IsServer && ATM.AmountDepositedToday >= float.MaxValue)
			{
				Begin();
			}
			if (base.QuestState == EQuestState.Completed)
			{
				return;
			}
			if (InstanceFinder.IsServer && BuyBusinessEntry.State == EQuestState.Active && Business.OwnedBusinesses.Count > 0)
			{
				BuyBusinessEntry.Complete();
			}
			if (GoToBusinessEntry.State == EQuestState.Active)
			{
				if (Business.OwnedBusinesses.Count > 0)
				{
					GoToBusinessEntry.transform.position = Business.OwnedBusinesses[0].PoI.transform.position;
				}
				if (Player.Local.CurrentBusiness != null)
				{
					GoToBusinessEntry.Complete();
				}
			}
		}
	}
}
