using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Money;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.UI.Input;
using UnityEngine.InputSystem;

namespace ScheduleOne.Quests
{
	public class Quest_GettingStarted : Quest
	{
		public float CashAmount = 375f;

		public DeadDrop CashDrop;

		public QuestEntry CashQuestEntry;

		public QuestEntry ReadMessagesQuest;

		public UncleNelson Nelson;

		protected override void MinPass()
		{
			base.MinPass();
			if (base.QuestState == EQuestState.Active && ReadMessagesQuest.State == EQuestState.Active && Nelson.MSGConversation != null && Nelson.MSGConversation.read)
			{
				ReadMessagesQuest.Complete();
			}
		}

		public override void SetQuestState(EQuestState state, bool network = true)
		{
			base.SetQuestState(state, network);
			if (state == EQuestState.Active)
			{
				if (InstanceFinder.IsServer && (CashQuestEntry.State == EQuestState.Inactive || CashQuestEntry.State == EQuestState.Active) && CashDrop.Storage.ItemCount == 0)
				{
					CashDrop.Storage.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(CashAmount));
				}
				Singleton<GameInput>.Instance.GetAction(GameInput.ButtonCode.TogglePhone).GetBindingDisplayString(0, out var _, out var controlPath);
				string displayNameForControlPath = Singleton<InputPromptsManager>.Instance.GetDisplayNameForControlPath(controlPath);
				ReadMessagesQuest.SetEntryTitle("Open your phone (press " + displayNameForControlPath + ") and read your messages");
			}
		}
	}
}
