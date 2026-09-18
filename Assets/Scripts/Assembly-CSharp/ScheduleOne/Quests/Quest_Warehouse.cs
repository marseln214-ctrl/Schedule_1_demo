using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Variables;

namespace ScheduleOne.Quests
{
	public class Quest_Warehouse : Quest
	{
		protected override void MinPass()
		{
			base.MinPass();
			if (InstanceFinder.IsServer && base.QuestState == EQuestState.Inactive && !NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("WarehouseUnlocked") && NetworkSingleton<LevelManager>.Instance.GetFullRank() >= NetworkSingleton<DarkMarket>.Instance.UnlockRank)
			{
				Begin();
			}
		}
	}
}
