using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.Money;
using ScheduleOne.Property;
using ScheduleOne.Variables;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_Fixer : DialogueController
	{
		private EEmployeeType selectedEmployeeType;

		private ScheduleOne.Property.Property selectedProperty;

		private bool lastConfirmationWasInitial;

		public override void ChoiceCallback(string choiceLabel)
		{
			base.ChoiceCallback(choiceLabel);
			if (choiceLabel == "CONFIRM" && selectedProperty != null)
			{
				Confirm();
			}
			switch (choiceLabel)
			{
			case "Botanist":
				selectedEmployeeType = EEmployeeType.Botanist;
				break;
			case "Packager":
				selectedEmployeeType = EEmployeeType.Packager;
				break;
			case "Chemist":
				selectedEmployeeType = EEmployeeType.Chemist;
				break;
			case "Cleaner":
				selectedEmployeeType = EEmployeeType.Cleaner;
				break;
			}
			foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
			{
				if (choiceLabel == ownedProperty.PropertyCode)
				{
					selectedProperty = ownedProperty;
					handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));
					break;
				}
			}
		}

		public override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
		{
			if (dialogueLabel == "SELECT_LOCATION")
			{
				foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
				{
					if (ownedProperty.EmployeeCapacity != 0)
					{
						int count = ownedProperty.GetUnassignedBeds().Count;
						string propertyName = ownedProperty.PropertyName;
						propertyName = ((count != 1) ? (propertyName + " (" + count + " free beds)") : (propertyName + " (1 free bed)"));
						existingChoices.Add(new DialogueChoiceData
						{
							ChoiceText = propertyName,
							ChoiceLabel = ownedProperty.PropertyCode
						});
					}
				}
			}
			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
		}

		public override bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			if (choiceLabel == "CONFIRM")
			{
				Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(selectedEmployeeType);
				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < employeePrefab.SigningFee)
				{
					invalidReason = "Insufficient cash";
					return false;
				}
			}
			foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
			{
				if (choiceLabel == ownedProperty.PropertyCode && ownedProperty.Employees.Count >= ownedProperty.EmployeeCapacity)
				{
					invalidReason = "Employee limit reached";
					return false;
				}
			}
			return base.CheckChoice(choiceLabel, out invalidReason);
		}

		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			if (dialogueLabel == "FINALIZE")
			{
				Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(selectedEmployeeType);
				dialogueText = dialogueText.Replace("<SIGN_FEE>", "<color=#54E717>" + MoneyManager.FormatAmount(employeePrefab.SigningFee) + "</color>");
				dialogueText = dialogueText.Replace("<DAILY_WAGE>", "<color=#54E717>" + MoneyManager.FormatAmount(employeePrefab.DailyWage) + "</color>");
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}

		public override bool DecideBranch(string branchLabel, out int index)
		{
			if (branchLabel == "IS_FIRST_WORKER")
			{
				if (lastConfirmationWasInitial)
				{
					index = 1;
				}
				else
				{
					index = 0;
				}
				return true;
			}
			return base.DecideBranch(branchLabel, out index);
		}

		private void Confirm()
		{
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ClipboardAcquired"))
			{
				lastConfirmationWasInitial = true;
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ClipboardAcquired", true.ToString());
			}
			else
			{
				lastConfirmationWasInitial = false;
			}
			Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(selectedEmployeeType);
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - employeePrefab.SigningFee);
			NetworkSingleton<EmployeeManager>.Instance.CreateNewEmployee(selectedProperty, selectedEmployeeType);
		}
	}
}
