using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	public class DialogueManager : Singleton<DialogueManager>
	{
		public DialogueDatabase DefaultDatabase;

		public List<DialogueModule> DefaultModules = new List<DialogueModule>();

		public DialogueModule Get(EDialogueModule moduleType)
		{
			DialogueModule dialogueModule = DefaultModules.Find((DialogueModule x) => x.ModuleType == moduleType);
			if (dialogueModule == null)
			{
				Debug.LogError("Generic module not found for: " + moduleType);
			}
			return dialogueModule;
		}
	}
}
