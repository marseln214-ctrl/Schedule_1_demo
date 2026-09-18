using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Money;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_Supplier : DialogueController
	{
		public Dealer dealer;

		protected override void Start()
		{
			base.Start();
			dealer = npc as Dealer;
		}

		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && dialogueLabel == "ENTRY")
			{
				dialogueText = dialogueText.Replace("<SIGNING_FEE>", "<color=#54E717>" + MoneyManager.FormatAmount(dealer.SigningFee) + "</color>");
				dialogueText = dialogueText.Replace("<CUT>", "<color=#54E717>" + Mathf.RoundToInt(dealer.Cut * 100f) + "%</color>");
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}

		public override string ModifyChoiceText(string choiceLabel, string choiceText)
		{
			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && choiceLabel == "CONFIRM")
			{
				choiceText = choiceText.Replace("<SIGNING_FEE>", "<color=#54E717>" + MoneyManager.FormatAmount(dealer.SigningFee) + "</color>");
			}
			return base.ModifyChoiceText(choiceLabel, choiceText);
		}

		public override bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && choiceLabel == "CONFIRM" && NetworkSingleton<MoneyManager>.Instance.cashBalance < dealer.SigningFee)
			{
				invalidReason = "Insufficient cash";
				return false;
			}
			return base.CheckChoice(choiceLabel, out invalidReason);
		}

		public override void ChoiceCallback(string choiceLabel)
		{
			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && choiceLabel == "CONFIRM")
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - dealer.SigningFee);
				dealer.InitialRecruitment();
			}
			base.ChoiceCallback(choiceLabel);
		}
	}
}
