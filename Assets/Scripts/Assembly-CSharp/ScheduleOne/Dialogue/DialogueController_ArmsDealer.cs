using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_ArmsDealer : DialogueController
	{
		[Serializable]
		public class WeaponOption
		{
			public string Name;

			public float Price;

			public bool IsAvailable;

			public string NotAvailableReason;

			public StorableItemDefinition Item;
		}

		public List<WeaponOption> MeleeWeapons;

		public List<WeaponOption> RangedWeapons;

		public List<WeaponOption> Ammo;

		private List<WeaponOption> allWeapons;

		private WeaponOption chosenWeapon;

		private void Awake()
		{
			allWeapons = new List<WeaponOption>();
			allWeapons.AddRange(MeleeWeapons);
			allWeapons.AddRange(RangedWeapons);
			allWeapons.AddRange(Ammo);
		}

		public override void ChoiceCallback(string choiceLabel)
		{
			WeaponOption weaponOption = allWeapons.Find((WeaponOption x) => x.Name == choiceLabel);
			if (weaponOption != null)
			{
				chosenWeapon = weaponOption;
				handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));
			}
			if (choiceLabel == "CONFIRM" && chosenWeapon != null)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - chosenWeapon.Price);
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(chosenWeapon.Item.GetDefaultInstance());
			}
			base.ChoiceCallback(choiceLabel);
		}

		public override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
		{
			if (dialogueLabel == "MELEE_SELECTION")
			{
				existingChoices.AddRange(GetWeaponChoices(MeleeWeapons));
			}
			if (dialogueLabel == "RANGED_SELECTION")
			{
				existingChoices.AddRange(GetWeaponChoices(RangedWeapons));
			}
			if (dialogueLabel == "AMMO_SELECTION")
			{
				existingChoices.AddRange(GetWeaponChoices(Ammo));
			}
			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
		}

		private List<DialogueChoiceData> GetWeaponChoices(List<WeaponOption> options)
		{
			List<DialogueChoiceData> list = new List<DialogueChoiceData>();
			foreach (WeaponOption option in options)
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
			WeaponOption weaponOption = allWeapons.Find((WeaponOption x) => x.Name == choiceLabel);
			if (weaponOption != null)
			{
				if (!weaponOption.IsAvailable)
				{
					invalidReason = weaponOption.NotAvailableReason;
					return false;
				}
				if (weaponOption.Item.RequiresLevelToPurchase && NetworkSingleton<LevelManager>.Instance.GetFullRank() < weaponOption.Item.RequiredRank)
				{
					FullRank requiredRank = weaponOption.Item.RequiredRank;
					invalidReason = "Available at " + requiredRank.ToString();
					return false;
				}
				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < weaponOption.Price)
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
				dialogueText = dialogueText.Replace("<WEAPON>", chosenWeapon.Name);
				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(chosenWeapon.Price) + "</color>");
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}
	}
}
