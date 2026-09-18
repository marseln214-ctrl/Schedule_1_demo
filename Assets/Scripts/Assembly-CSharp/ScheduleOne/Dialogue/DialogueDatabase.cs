using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	[Serializable]
	[CreateAssetMenu(fileName = "New Dialogue Database", menuName = "Dialogue/Dialogue Database")]
	public class DialogueDatabase : ScriptableObject
	{
		public List<DialogueModule> Modules;

		public List<Entry> GenericEntries = new List<Entry>();

		private DialogueHandler handler;

		private List<DialogueModule> runtimeModules => handler.runtimeModules;

		public void Initialize(DialogueHandler _handler)
		{
			handler = _handler;
		}

		public DialogueModule GetModule(EDialogueModule moduleType)
		{
			if (runtimeModules == null)
			{
				Console.LogWarning("DialogueDatabase not initialized");
				return null;
			}
			DialogueModule dialogueModule = runtimeModules.Find((DialogueModule module) => module.ModuleType == moduleType);
			if (dialogueModule != null)
			{
				return dialogueModule;
			}
			return Singleton<DialogueManager>.Instance.Get(moduleType);
		}

		public DialogueChain GetChain(EDialogueModule moduleType, string key)
		{
			DialogueModule module = GetModule(moduleType);
			if (module == null)
			{
				Console.LogWarning("Could not find module: " + moduleType);
				return null;
			}
			return module.GetChain(key);
		}

		public bool HasChain(EDialogueModule moduleType, string key)
		{
			DialogueModule module = GetModule(moduleType);
			if (module == null)
			{
				Console.LogWarning("Could not find module: " + moduleType);
				return false;
			}
			return module.HasChain(key);
		}

		public string GetLine(EDialogueModule moduleType, string key)
		{
			DialogueModule module = GetModule(moduleType);
			if (module == null)
			{
				Console.LogWarning("Could not find module: " + moduleType);
				return string.Empty;
			}
			return module.GetLine(key);
		}
	}
}
