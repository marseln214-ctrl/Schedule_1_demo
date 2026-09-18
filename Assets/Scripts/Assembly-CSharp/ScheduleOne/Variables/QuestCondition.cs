using System;
using ScheduleOne.Quests;

namespace ScheduleOne.Variables
{
	[Serializable]
	public class QuestCondition
	{
		public bool CheckQuestState = true;

		public string QuestName = "Quest name";

		public EQuestState QuestState = EQuestState.Active;

		public bool CheckQuestEntryState;

		public int QuestEntryIndex;

		public EQuestState QuestEntryState = EQuestState.Active;

		public bool Evaluate()
		{
			Quest quest = Quest.GetQuest(QuestName);
			if (quest == null)
			{
				Console.LogError("Quest " + QuestName + " not found");
				return false;
			}
			if (CheckQuestState && quest.QuestState != QuestState)
			{
				return false;
			}
			if (CheckQuestEntryState)
			{
				if (quest.Entries.Count <= QuestEntryIndex)
				{
					Console.LogError("Quest " + QuestName + " does not have entry " + QuestEntryIndex);
					return false;
				}
				if (quest.Entries[QuestEntryIndex].State != QuestEntryState)
				{
					return false;
				}
			}
			return true;
		}
	}
}
