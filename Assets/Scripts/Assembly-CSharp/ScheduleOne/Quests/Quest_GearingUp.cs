using ScheduleOne.Economy;

namespace ScheduleOne.Quests
{
	public class Quest_GearingUp : Quest
	{
		public QuestEntry WaitForDropEntry;

		public QuestEntry CollectDropEntry;

		public Supplier Supplier;

		private bool setCollectionPosition;

		protected override void Start()
		{
			base.Start();
			Supplier.onDeaddropReady.AddListener(DropReady);
		}

		protected override void MinPass()
		{
			base.MinPass();
			if (CollectDropEntry.State == EQuestState.Active && !setCollectionPosition)
			{
				DeadDrop deadDrop = DeadDrop.DeadDrops.Find((DeadDrop x) => x.Storage.ItemCount > 0);
				if (deadDrop != null)
				{
					setCollectionPosition = true;
					CollectDropEntry.SetPoILocation(deadDrop.transform.position);
				}
			}
			if (WaitForDropEntry.State == EQuestState.Active)
			{
				float num = Supplier.minsUntilDeaddropReady;
				if (num > 0f)
				{
					WaitForDropEntry.SetEntryTitle("Wait for the dead drop (" + num + " mins)");
				}
				else
				{
					WaitForDropEntry.SetEntryTitle("Wait for the dead drop");
				}
			}
		}

		private void DropReady()
		{
			if (WaitForDropEntry.State == EQuestState.Active)
			{
				WaitForDropEntry.Complete();
				MinPass();
			}
		}
	}
}
