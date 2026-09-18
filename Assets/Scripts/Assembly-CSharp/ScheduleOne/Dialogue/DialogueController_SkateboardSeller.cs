using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using UnityEngine.Events;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_SkateboardSeller : DialogueController
	{
		[Serializable]
		public class Option
		{
			public string Name;

			public float Price;

			public bool IsAvailable;

			public string NotAvailableReason;

			public ItemDefinition Item;
		}

		public List<Option> Options = new List<Option>();

		private Option chosenWeapon;

		public UnityEvent onPurchase;

		private void Awake()
		{
		}

		public override void ChoiceCallback(string choiceLabel)
		{
			Option option = Options.Find((Option x) => x.Name == choiceLabel);
			if (option != null)
			{
				chosenWeapon = option;
				handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));
			}
			if (choiceLabel == "CONFIRM" && chosenWeapon != null)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - chosenWeapon.Price);
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(chosenWeapon.Item.GetDefaultInstance());
				npc.Inventory.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(chosenWeapon.Price));
				if (onPurchase != null)
				{
					onPurchase.Invoke();
				}
			}
			base.ChoiceCallback(choiceLabel);
		}

		public override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
		{
			if (dialogueLabel == "ENTRY" && DialogueHandler.activeDialogue.name == "Skateboard_Sell")
			{
				existingChoices.AddRange(GetChoices(Options));
			}
			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
		}

		private List<DialogueChoiceData> GetChoices(List<Option> options)
		{
			List<DialogueChoiceData> list = new List<DialogueChoiceData>();
			foreach (Option option in options)
			{
				DialogueChoiceData dialogueChoiceData = new DialogueChoiceData();
				dialogueChoiceData.ChoiceText = option.Name + "<color=#54E717> (" + MoneyManager.FormatAmount(option.Price) + ")</color>";
				dialogueChoiceData.ChoiceLabel = option.Name;
				list.Add(dialogueChoiceData);
			}
			return list;
		}

		public override bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			Option option = Options.Find((Option x) => x.Name == choiceLabel);
			if (option != null)
			{
				if (!option.IsAvailable)
				{
					invalidReason = option.NotAvailableReason;
					return false;
				}
				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < option.Price)
				{
					invalidReason = "Insufficient cash";
					return false;
				}
			}
			if (choiceLabel == "CONFIRM" && !PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(chosenWeapon.Item.GetDefaultInstance()))
			{
				invalidReason = "Inventory full";
				return false;
			}
			return base.CheckChoice(choiceLabel, out invalidReason);
		}

		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			if (dialogueLabel == "FINALIZE" && chosenWeapon != null)
			{
				dialogueText = dialogueText.Replace("<NAME>", chosenWeapon.Name);
				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(chosenWeapon.Price) + "</color>");
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}
	}
}
