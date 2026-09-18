using FishNet;
using ScheduleOne.Economy;

namespace ScheduleOne.Quests
{
	public class Quest_WeNeedToCook : Quest
	{
		public Quest[] PrerequisiteQuests;

		public Supplier MethSupplier;

		protected override void MinPass()
		{
			base.MinPass();
			if (!InstanceFinder.IsServer || base.QuestState != 0 || !MethSupplier.RelationData.Unlocked)
			{
				return;
			}
			Quest[] prerequisiteQuests = PrerequisiteQuests;
			for (int i = 0; i < prerequisiteQuests.Length; i++)
			{
				if (prerequisiteQuests[i].QuestState != EQuestState.Completed)
				{
					return;
				}
			}
			Begin();
		}
	}
}
