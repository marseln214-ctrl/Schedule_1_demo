using System.Collections.Generic;
using ScheduleOne.Economy;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.Quests
{
	public class DeaddropQuest : Quest
	{
		public static List<DeaddropQuest> DeaddropQuests = new List<DeaddropQuest>();

		public DeadDrop Drop { get; private set; }

		public override void Begin(bool network = true)
		{
			base.Begin(network);
			if (!DeaddropQuests.Contains(this))
			{
				DeaddropQuests.Add(this);
			}
		}

		public void SetDrop(DeadDrop drop)
		{
			Drop = drop;
			Entries[0].SetPoILocation(Drop.transform.position);
		}

		protected override void MinPass()
		{
			base.MinPass();
			if (base.QuestState == EQuestState.Active && Drop.Storage.ItemCount == 0)
			{
				Entries[0].Complete();
				Complete(network: false);
			}
		}

		private void OnDestroy()
		{
			DeaddropQuests.Remove(this);
		}

		public override void End()
		{
			base.End();
			DeaddropQuests.Remove(this);
		}

		public override string GetSaveString()
		{
			List<QuestEntryData> list = new List<QuestEntryData>();
			for (int i = 0; i < Entries.Count; i++)
			{
				list.Add(Entries[i].GetSaveData());
			}
			return new DeaddropQuestData(base.GUID.ToString(), base.QuestState, base.IsTracked, title, Description, base.Expires, new GameDateTimeData(base.Expiry), list.ToArray(), Drop.GUID.ToString()).GetJson();
		}
	}
}
