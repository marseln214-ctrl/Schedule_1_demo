using ScheduleOne.Dialogue;
using ScheduleOne.Interaction;
using UnityEngine;

public class TestNPC : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	protected InteractableObject intObj;

	[SerializeField]
	protected DialogueHandler handler;

	public void Hovered()
	{
		if (DialogueHandler.activeDialogue == null)
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void Interacted()
	{
		handler.InitializeDialogue("TestDialogue", enableDialogueBehaviour: true, "BRANCH_CHECKPASS");
	}
}
