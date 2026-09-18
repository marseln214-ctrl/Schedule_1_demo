using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using UnityEngine.Events;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_Ming : DialogueController
	{
		public ScheduleOne.Property.Property Property;

		public float Price = 500f;

		public DialogueContainer BuyDialogue;

		public string BuyText = "I'd like to buy the room";

		public string RemindText = "Where is my room?";

		public DialogueContainer RemindLocationDialogue;

		public QuestEntry[] PurchaseRoomQuests;

		public UnityEvent onPurchase;

		protected override void Start()
		{
			base.Start();
			DialogueChoice dialogueChoice = new DialogueChoice();
			dialogueChoice.ChoiceText = BuyText;
			dialogueChoice.Conversation = BuyDialogue;
			dialogueChoice.Enabled = true;
			dialogueChoice.shouldShowCheck = CanBuyRoom;
			DialogueChoice dialogueChoice2 = new DialogueChoice();
			dialogueChoice2.ChoiceText = RemindText;
			dialogueChoice2.Conversation = RemindLocationDialogue;
			dialogueChoice2.Enabled = true;
			dialogueChoice2.shouldShowCheck = (bool enabled) => Property.IsOwned;
			AddDialogueChoice(dialogueChoice);
			AddDialogueChoice(dialogueChoice2);
		}

		private bool CanBuyRoom(bool enabled)
		{
			if (!Property.IsOwned)
			{
				if (PurchaseRoomQuests.Length != 0)
				{
					return PurchaseRoomQuests.FirstOrDefault((QuestEntry q) => q.State == EQuestState.Active) != null;
				}
				return true;
			}
			return false;
		}

		public override string ModifyChoiceText(string choiceLabel, string choiceText)
		{
			if (choiceLabel == "CHOICE_CONFIRM")
			{
				choiceText = choiceText.Replace("<PRICE>", "<color=#54E717>(" + MoneyManager.FormatAmount(Price) + ")</color>");
			}
			return base.ModifyChoiceText(choiceLabel, choiceText);
		}

		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			if (dialogueLabel == "ENTRY")
			{
				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(Price) + "</color>");
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}

		public override bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			if (choiceLabel == "CHOICE_CONFIRM" && NetworkSingleton<MoneyManager>.Instance.cashBalance < Price)
			{
				invalidReason = "Insufficient cash";
				return false;
			}
			return base.CheckChoice(choiceLabel, out invalidReason);
		}

		public override void ChoiceCallback(string choiceLabel)
		{
			if (choiceLabel == "CHOICE_CONFIRM")
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - Price);
				npc.Inventory.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(Price));
				Property.SetOwned();
				if (onPurchase != null)
				{
					onPurchase.Invoke();
				}
			}
			base.ChoiceCallback(choiceLabel);
		}
	}
}
