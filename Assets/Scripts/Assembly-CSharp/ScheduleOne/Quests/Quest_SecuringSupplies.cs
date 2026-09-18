using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;

namespace ScheduleOne.Quests
{
	public class Quest_SecuringSupplies : Quest
	{
		public Supplier Supplier;

		protected override void MinPass()
		{
			base.MinPass();
			if (InstanceFinder.IsServer && base.QuestState == EQuestState.Inactive && NetworkSingleton<TimeManager>.Instance.ElapsedDays >= 1 && (NetworkSingleton<TimeManager>.Instance.CurrentTime > 900 || NetworkSingleton<TimeManager>.Instance.ElapsedDays >= 2))
			{
				Begin();
				Supplier.SetUnlockMessage();
			}
		}
	}
}
