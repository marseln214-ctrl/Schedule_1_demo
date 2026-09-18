using System;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	[Serializable]
	public class DialogueNodeData
	{
		public string Guid;

		public string DialogueText;

		public string DialogueNodeLabel;

		public Vector2 Position;

		public DialogueChoiceData[] choices;

		public EVOLineType VoiceLine;

		public DialogueNodeData GetCopy()
		{
			DialogueNodeData dialogueNodeData = new DialogueNodeData();
			dialogueNodeData.Guid = Guid;
			dialogueNodeData.DialogueText = DialogueText;
			dialogueNodeData.DialogueNodeLabel = DialogueNodeLabel;
			dialogueNodeData.Position = Position;
			for (int i = 0; i < choices.Length; i++)
			{
				choices.CopyTo(dialogueNodeData.choices, 0);
			}
			dialogueNodeData.VoiceLine = VoiceLine;
			return dialogueNodeData;
		}
	}
}
