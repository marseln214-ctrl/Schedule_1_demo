using System;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.Quests
{
	[Serializable]
	public class QuestStateSetter
	{
		public string QuestName;

		public bool SetQuestState;

		public QuestManager.EQuestAction QuestState;

		public bool SetQuestEntryState;

		public int QuestEntryIndex;

		public EQuestState QuestEntryState;

		public void Execute()
		{
			Quest quest = Quest.GetQuest(QuestName);
			if (quest == null)
			{
				Console.LogWarning("Failed to find quest with name: " + QuestName);
				return;
			}
			if (SetQuestState)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestAction(quest.GUID.ToString(), QuestState);
			}
			if (SetQuestEntryState)
			{
				NetworkSingleton<QuestManager>.Instance.SendQuestEntryState(quest.GUID.ToString(), QuestEntryIndex, QuestEntryState);
			}
		}
	}
}
