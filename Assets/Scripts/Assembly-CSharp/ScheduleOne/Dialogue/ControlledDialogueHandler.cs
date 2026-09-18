using System.Collections.Generic;

namespace ScheduleOne.Dialogue
{
	public class ControlledDialogueHandler : DialogueHandler
	{
		private DialogueController controller;

		protected override void Awake()
		{
			base.Awake();
			controller = GetComponent<DialogueController>();
		}

		protected override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			dialogueText = controller.ModifyDialogueText(dialogueLabel, dialogueText);
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}

		protected override string ModifyChoiceText(string choiceLabel, string choiceText)
		{
			choiceText = controller.ModifyChoiceText(choiceLabel, choiceText);
			return base.ModifyChoiceText(choiceLabel, choiceText);
		}

		protected override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
		{
			controller.ModifyChoiceList(dialogueLabel, ref existingChoices);
			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
		}

		protected override void ChoiceCallback(string choiceLabel)
		{
			controller.ChoiceCallback(choiceLabel);
			base.ChoiceCallback(choiceLabel);
		}

		protected override int CheckBranch(string branchLabel)
		{
			if (controller.DecideBranch(branchLabel, out var index))
			{
				return index;
			}
			return base.CheckBranch(branchLabel);
		}

		public override bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			if (!controller.CheckChoice(choiceLabel, out invalidReason))
			{
				return false;
			}
			return base.CheckChoice(choiceLabel, out invalidReason);
		}
	}
}
