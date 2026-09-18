using ScheduleOne.NPCs.Relation;

namespace ScheduleOne.Quests
{
	public class Quest_Connections : Quest
	{
		public override void Begin(bool network = true)
		{
			base.Begin(network);
			foreach (QuestEntry entry in Entries)
			{
				if (entry.GetComponent<NPCUnlockTracker>().Npc.RelationData.Unlocked)
				{
					entry.SetState(EQuestState.Completed);
				}
				else
				{
					entry.SetState(EQuestState.Active);
				}
			}
		}
	}
}
