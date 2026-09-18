using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Property;

namespace ScheduleOne.Dialogue
{
	public class DialogueHandler_EstateAgent : ControlledDialogueHandler
	{
		private ScheduleOne.Property.Property selectedProperty;

		private Business selectedBusiness;

		public override bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			if (property != null && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < property.Price)
			{
				invalidReason = "Insufficient balance";
				return false;
			}
			if (business != null && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < business.Price)
			{
				invalidReason = "Insufficient balance";
				return false;
			}
			return base.CheckChoice(choiceLabel, out invalidReason);
		}

		public override bool ShouldChoiceBeShown(string choiceLabel)
		{
			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.OwnedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			Business business = Business.OwnedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			if (property != null && property.IsOwned)
			{
				return false;
			}
			if (business != null && business.IsOwned)
			{
				return false;
			}
			return base.ShouldChoiceBeShown(choiceLabel);
		}

		protected override void ChoiceCallback(string choiceLabel)
		{
			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			if (property != null)
			{
				selectedProperty = property;
			}
			if (business != null)
			{
				selectedBusiness = business;
			}
			base.ChoiceCallback(choiceLabel);
		}

		protected override void DialogueCallback(string choiceLabel)
		{
			if (choiceLabel == "CONFIRM_BUY")
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedProperty.PropertyName + " purchase", 0f - selectedProperty.Price, 1f, string.Empty);
				selectedProperty.SetOwned();
			}
			if (choiceLabel == "CONFIRM_BUY_BUSINESS")
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedBusiness.PropertyName + " purchase", 0f - selectedBusiness.Price, 1f, string.Empty);
				selectedBusiness.SetOwned();
			}
			base.DialogueCallback(choiceLabel);
		}

		protected override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			if (dialogueLabel == "CONFIRM")
			{
				return dialogueText.Replace("<PROPERTY>", selectedProperty.PropertyName.ToLower());
			}
			if (dialogueLabel == "CONFIRM_BUSINESS")
			{
				return dialogueText.Replace("<BUSINESS>", selectedBusiness.PropertyName.ToLower());
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}

		protected override string ModifyChoiceText(string choiceLabel, string choiceText)
		{
			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
			if (property != null)
			{
				return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(property.Price) + ")</color>");
			}
			if (business != null)
			{
				return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(business.Price) + ")</color>");
			}
			if (choiceLabel == "CONFIRM_CHOICE")
			{
				if (selectedProperty != null)
				{
					return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedProperty.Price) + ")</color>");
				}
				if (selectedBusiness != null)
				{
					return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedBusiness.Price) + ")</color>");
				}
			}
			return base.ModifyChoiceText(choiceLabel, choiceText);
		}
	}
}
