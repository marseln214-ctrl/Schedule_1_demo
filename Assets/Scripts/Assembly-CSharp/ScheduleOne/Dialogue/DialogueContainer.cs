using System;
using System.Collections.Generic;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Dialogue
{
	[Serializable]
	public class DialogueContainer : ScriptableObject
	{
		public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();

		public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();

		public List<BranchNodeData> BranchNodeData = new List<BranchNodeData>();

		public bool allowExit { get; private set; } = true;


		public bool AllowExit
		{
			get
			{
				if (!allowExit && !Player.Local.IsArrested)
				{
					return !Player.Local.Health.IsAlive;
				}
				return true;
			}
		}

		public DialogueNodeData GetDialogueNodeByLabel(string dialogueNodeLabel)
		{
			return DialogueNodeData.Find((DialogueNodeData x) => x.DialogueNodeLabel == dialogueNodeLabel);
		}

		public BranchNodeData GetBranchNodeByLabel(string branchLabel)
		{
			return BranchNodeData.Find((BranchNodeData x) => x.BranchLabel == branchLabel);
		}

		public DialogueNodeData GetDialogueNodeByGUID(string dialogueNodeGUID)
		{
			return DialogueNodeData.Find((DialogueNodeData x) => x.Guid == dialogueNodeGUID);
		}

		public BranchNodeData GetBranchNodeByGUID(string branchGUID)
		{
			return BranchNodeData.Find((BranchNodeData x) => x.Guid == branchGUID);
		}

		public NodeLinkData GetLink(string baseChoiceOrOptionGUID)
		{
			return NodeLinks.Find((NodeLinkData x) => x.BaseChoiceOrOptionGUID == baseChoiceOrOptionGUID);
		}

		public void SetAllowExit(bool allowed)
		{
			allowExit = allowed;
		}
	}
}
