using System;

namespace ScheduleOne.Quests
{
	[Serializable]
	public class QuestWindowConfig
	{
		public bool IsEnabled;

		public int WindowStartTime = 1200;

		public int WindowEndTime = 1800;
	}
}
