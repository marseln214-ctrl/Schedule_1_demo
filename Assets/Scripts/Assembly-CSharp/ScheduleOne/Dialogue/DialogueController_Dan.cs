using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;

namespace ScheduleOne.Dialogue
{
	public class DialogueController_Dan : DialogueController
	{
		public ItemDefinition ItemToGive;

		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			if (dialogueLabel == "GIVE_ITEM")
			{
				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(ItemToGive.GetDefaultInstance());
			}
			return base.ModifyDialogueText(dialogueLabel, dialogueText);
		}
	}
}
