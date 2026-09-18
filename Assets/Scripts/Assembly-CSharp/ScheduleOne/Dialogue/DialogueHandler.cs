using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Dialogue
{
	public class DialogueHandler : MonoBehaviour
	{
		public const float TimePerChar = 0.2f;

		public const float WorldspaceDialogueMinDuration = 1.5f;

		public const float WorldspaceDialogueMaxDuration = 5f;

		public static DialogueContainer activeDialogue;

		public static DialogueNodeData activeDialogueNode;

		public DialogueDatabase Database;

		[Header("References")]
		public Transform LookPosition;

		public WorldspaceDialogueRenderer WorldspaceRend;

		public VOEmitter VOEmitter;

		[HideInInspector]
		public List<DialogueChoiceData> CurrentChoices = new List<DialogueChoiceData>();

		[Header("Events")]
		public DialogueEvent[] DialogueEvents;

		public UnityEvent onConversationStart;

		public UnityEvent<string> onDialogueNodeDisplayed;

		public UnityEvent<string> onDialogueChoiceChosen;

		protected string overrideText = string.Empty;

		[SerializeField]
		private List<DialogueContainer> dialogueContainers = new List<DialogueContainer>();

		private List<NodeLinkData> TempLinks = new List<NodeLinkData>();

		private bool skipNextDialogueBehaviourEnd;

		private bool passChecked;

		public bool IsPlaying { get; private set; }

		public NPC NPC { get; protected set; }

		private DialogueCanvas canvas => Singleton<DialogueCanvas>.Instance;

		public List<DialogueModule> runtimeModules { get; private set; } = new List<DialogueModule>();


		protected virtual void Awake()
		{
			if (NPC == null)
			{
				NPC = GetComponentInParent<NPC>();
			}
			if (Database == null)
			{
				Console.LogWarning(NPC.fullName + " dialogue database isn't assigned! Using default database.");
				Database = Singleton<DialogueManager>.Instance.DefaultDatabase;
			}
			if (VOEmitter == null && NPC != null)
			{
				VOEmitter = NPC.VoiceOverEmitter;
			}
			DialogueModule dialogueModule = base.gameObject.AddComponent<DialogueModule>();
			dialogueModule.ModuleType = EDialogueModule.Generic;
			dialogueModule.Entries = Database.GenericEntries;
			runtimeModules.Add(dialogueModule);
			runtimeModules.AddRange(Database.Modules);
			Database.Initialize(this);
		}

		public void InitializeDialogue(DialogueContainer container)
		{
			InitializeDialogue(container, enableDialogueBehaviour: true, "ENTRY");
		}

		public void InitializeDialogue(DialogueContainer dialogueContainer, bool enableDialogueBehaviour = true, string entryNodeLabel = "ENTRY")
		{
			if (dialogueContainer == null)
			{
				Console.LogWarning("InitializeDialogue: provided dialogueContainer is null");
				return;
			}
			if (enableDialogueBehaviour)
			{
				NPC.behaviour.GenericDialogueBehaviour.SendTargetPlayer(Player.Local.NetworkObject);
				NPC.behaviour.GenericDialogueBehaviour.SendEnable();
				NPC.behaviour.Update();
			}
			if (WorldspaceRend.ShownText != null)
			{
				WorldspaceRend.HideText();
			}
			if (onConversationStart != null)
			{
				onConversationStart.Invoke();
			}
			NPC npc = GetComponentInParent<NPC>();
			if (npc != null && npc.Avatar.Anim.TimeSinceSitEnd < 0.5f && enableDialogueBehaviour)
			{
				StartCoroutine(Wait());
			}
			else
			{
				Open();
			}
			void Open()
			{
				IsPlaying = true;
				if (dialogueContainer.GetDialogueNodeByLabel(entryNodeLabel) != null)
				{
					activeDialogue = dialogueContainer;
					ShowNode(dialogueContainer.GetDialogueNodeByLabel(entryNodeLabel));
					NPC.SetConversant(Player.Local.NetworkObject);
				}
				else if (dialogueContainer.GetBranchNodeByLabel(entryNodeLabel) != null)
				{
					activeDialogue = dialogueContainer;
					EvaluateBranch(dialogueContainer.GetBranchNodeByLabel(entryNodeLabel));
					NPC.SetConversant(Player.Local.NetworkObject);
				}
				else
				{
					Console.LogWarning("InitializeDialogue: could not find dialogue or branch node with label '" + entryNodeLabel + "'");
				}
			}
			IEnumerator Wait()
			{
				yield return new WaitForSeconds(0.5f - npc.Avatar.Anim.TimeSinceSitEnd);
				Open();
			}
		}

		public void InitializeDialogue(string dialogueContainerName, bool enableDialogueBehaviour = true, string entryNodeLabel = "ENTRY")
		{
			DialogueContainer dialogueContainer = dialogueContainers.Find((DialogueContainer x) => x.name.ToLower() == dialogueContainerName.ToLower());
			if (dialogueContainer == null)
			{
				Console.LogWarning("InitializeDialogue: Could not find DialogueContainer with name '" + dialogueContainerName + "'");
			}
			else
			{
				InitializeDialogue(dialogueContainer, enableDialogueBehaviour, entryNodeLabel);
			}
		}

		public virtual bool CanBeginConversation()
		{
			return NPC.SyncAccessor_PlayerConversant == null;
		}

		public void OverrideShownDialogue(string _overrideText)
		{
			overrideText = _overrideText;
			canvas.OverrideText(overrideText);
		}

		public void StopOverride()
		{
			overrideText = string.Empty;
			canvas.StopTextOverride();
			if (activeDialogueNode != null)
			{
				ShowNode(activeDialogueNode);
			}
		}

		public virtual void EndDialogue()
		{
			if (skipNextDialogueBehaviourEnd)
			{
				skipNextDialogueBehaviourEnd = false;
			}
			else
			{
				NPC.behaviour.GenericDialogueBehaviour.SendDisable();
			}
			DialogueEvent[] dialogueEvents = DialogueEvents;
			foreach (DialogueEvent dialogueEvent in dialogueEvents)
			{
				if (!(dialogueEvent.Dialogue != activeDialogue) && dialogueEvent.onDialogueEnded != null)
				{
					dialogueEvent.onDialogueEnded.Invoke();
				}
			}
			canvas.EndDialogue();
			IsPlaying = false;
			activeDialogue = null;
			activeDialogueNode = null;
			NPC.SetConversant(null);
		}

		public void SkipNextDialogueBehaviourEnd()
		{
			skipNextDialogueBehaviourEnd = true;
		}

		protected virtual DialogueNodeData FinalizeDialogueNode(DialogueNodeData data)
		{
			return data;
		}

		public void ShowNode(DialogueNodeData node)
		{
			node = FinalizeDialogueNode(node);
			activeDialogueNode = node;
			if (overrideText != string.Empty)
			{
				return;
			}
			string dialogueText = ModifyDialogueText(node.DialogueNodeLabel, node.DialogueText);
			CurrentChoices = new List<DialogueChoiceData>();
			DialogueChoiceData[] choices = node.choices;
			foreach (DialogueChoiceData dialogueChoiceData in choices)
			{
				if (ShouldChoiceBeShown(dialogueChoiceData.ChoiceLabel))
				{
					CurrentChoices.Add(dialogueChoiceData);
				}
			}
			TempLinks.Clear();
			ModifyChoiceList(node.DialogueNodeLabel, ref CurrentChoices);
			List<string> list = new List<string>();
			foreach (DialogueChoiceData currentChoice in CurrentChoices)
			{
				list.Add(ModifyChoiceText(currentChoice.ChoiceLabel, currentChoice.ChoiceText));
			}
			DialogueCallback(node.DialogueNodeLabel);
			if (VOEmitter != null && node.VoiceLine != 0)
			{
				VOEmitter.Play(node.VoiceLine);
			}
			canvas.DisplayDialogueNode(this, activeDialogueNode, dialogueText, list);
		}

		private void EvaluateBranch(BranchNodeData node)
		{
			int num = CheckBranch(node.BranchLabel);
			if (node.options.Length > num)
			{
				NodeLinkData link = GetLink(node.options[num].Guid);
				if (link != null)
				{
					if (activeDialogue.GetDialogueNodeByGUID(link.TargetNodeGuid) != null)
					{
						ShowNode(activeDialogue.GetDialogueNodeByGUID(link.TargetNodeGuid));
					}
					else if (activeDialogue.GetBranchNodeByGUID(link.TargetNodeGuid) != null)
					{
						EvaluateBranch(activeDialogue.GetBranchNodeByGUID(link.TargetNodeGuid));
					}
				}
				else
				{
					EndDialogue();
				}
			}
			else
			{
				Console.LogWarning("EvaluateBranch: optionIndex is out of range");
				EndDialogue();
			}
			TempLinks.Clear();
		}

		public void ChoiceSelected(int choiceIndex)
		{
			DialogueNodeData dialogueNodeData = activeDialogueNode;
			ChoiceCallback(CurrentChoices[choiceIndex].ChoiceLabel);
			if (activeDialogueNode != dialogueNodeData || activeDialogueNode == null)
			{
				return;
			}
			NodeLinkData link = GetLink(CurrentChoices[choiceIndex].Guid);
			if (link != null)
			{
				if (activeDialogue.GetDialogueNodeByGUID(link.TargetNodeGuid) != null)
				{
					ShowNode(activeDialogue.GetDialogueNodeByGUID(link.TargetNodeGuid));
				}
				else if (activeDialogue.GetBranchNodeByGUID(link.TargetNodeGuid) != null)
				{
					EvaluateBranch(activeDialogue.GetBranchNodeByGUID(link.TargetNodeGuid));
				}
			}
			else
			{
				EndDialogue();
			}
		}

		public void ContinueSubmitted()
		{
			if (activeDialogueNode.choices.Length == 0)
			{
				EndDialogue();
				return;
			}
			NodeLinkData link = GetLink(activeDialogueNode.choices[0].Guid);
			if (link != null)
			{
				if (activeDialogue.GetDialogueNodeByGUID(link.TargetNodeGuid) != null)
				{
					ShowNode(activeDialogue.GetDialogueNodeByGUID(link.TargetNodeGuid));
				}
				else if (activeDialogue.GetBranchNodeByGUID(link.TargetNodeGuid) != null)
				{
					EvaluateBranch(activeDialogue.GetBranchNodeByGUID(link.TargetNodeGuid));
				}
			}
			else
			{
				EndDialogue();
			}
		}

		public virtual bool CheckChoice(string choiceLabel, out string invalidReason)
		{
			if (choiceLabel == "CHOICE_TEST")
			{
				invalidReason = "IT JUST CAN'T BE DONE";
				return false;
			}
			invalidReason = string.Empty;
			return true;
		}

		public virtual bool ShouldChoiceBeShown(string choiceLabel)
		{
			return true;
		}

		protected virtual int CheckBranch(string branchLabel)
		{
			if (branchLabel == "BRANCH_REJECTION")
			{
				return Random.Range(0, 2);
			}
			if (branchLabel == "BRANCH_CHECKPASS")
			{
				if (passChecked)
				{
					return 1;
				}
				return 0;
			}
			if (branchLabel != string.Empty)
			{
				Console.LogWarning("CheckBranch: branch label '" + branchLabel + "' not accounted for!");
			}
			return 0;
		}

		protected virtual string ModifyDialogueText(string dialogueLabel, string dialogueText)
		{
			return dialogueText;
		}

		protected virtual string ModifyChoiceText(string choiceLabel, string choiceText)
		{
			return choiceText;
		}

		protected virtual void ChoiceCallback(string choiceLabel)
		{
			if (onDialogueChoiceChosen != null)
			{
				onDialogueChoiceChosen.Invoke(choiceLabel);
			}
		}

		protected virtual void DialogueCallback(string dialogueLabel)
		{
			if (onDialogueNodeDisplayed != null)
			{
				onDialogueNodeDisplayed.Invoke(dialogueLabel);
			}
			DialogueEvent[] dialogueEvents = DialogueEvents;
			foreach (DialogueEvent dialogueEvent in dialogueEvents)
			{
				if (dialogueEvent.Dialogue != activeDialogue)
				{
					continue;
				}
				DialogueNodeEvent[] nodeEvents = dialogueEvent.NodeEvents;
				foreach (DialogueNodeEvent dialogueNodeEvent in nodeEvents)
				{
					if (dialogueNodeEvent.NodeLabel == dialogueLabel)
					{
						dialogueNodeEvent.onNodeDisplayed.Invoke();
					}
				}
			}
		}

		protected virtual void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
		{
		}

		protected void CreateTempLink(string baseNodeGUID, string baseOptionGUID, string targetNodeGUID)
		{
			NodeLinkData nodeLinkData = new NodeLinkData();
			nodeLinkData.BaseDialogueOrBranchNodeGuid = baseNodeGUID;
			nodeLinkData.BaseChoiceOrOptionGUID = baseOptionGUID;
			nodeLinkData.TargetNodeGuid = targetNodeGUID;
			TempLinks.Add(nodeLinkData);
		}

		private NodeLinkData GetLink(string baseChoiceOrOptionGUID)
		{
			NodeLinkData nodeLinkData = activeDialogue.GetLink(baseChoiceOrOptionGUID);
			if (nodeLinkData == null)
			{
				nodeLinkData = TempLinks.Find((NodeLinkData x) => x.BaseChoiceOrOptionGUID == baseChoiceOrOptionGUID);
			}
			return nodeLinkData;
		}

		public virtual void Hovered()
		{
		}

		public virtual void Interacted()
		{
		}

		public virtual void PlayReaction_Local(string key)
		{
			PlayReaction(key, -1f, network: false);
		}

		public virtual void PlayReaction_Networked(string key)
		{
			PlayReaction(key, -1f, network: true);
		}

		public virtual void PlayReaction(string key, float duration, bool network)
		{
			if (!NPC.IsConscious)
			{
				return;
			}
			if (network)
			{
				NPC.SendWorldspaceDialogueKey(key, duration);
				return;
			}
			if (key == string.Empty)
			{
				HideWorldspaceDialogue();
				return;
			}
			string line = Database.GetLine(EDialogueModule.Reactions, key);
			if (duration == -1f)
			{
				duration = Mathf.Clamp((float)line.Length * 0.2f, 1.5f, 5f);
			}
			WorldspaceRend.ShowText(line, duration);
		}

		public virtual void HideWorldspaceDialogue()
		{
			WorldspaceRend.HideText();
		}

		public virtual void ShowWorldspaceDialogue(string text, float duration)
		{
			if (NPC.IsConscious)
			{
				WorldspaceRend.ShowText(text, duration);
			}
		}

		public virtual void ShowWorldspaceDialogue_5s(string text)
		{
			ShowWorldspaceDialogue(text, 5f);
		}
	}
}
