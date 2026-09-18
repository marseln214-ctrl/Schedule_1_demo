using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class NPCSelector : MonoBehaviour
	{
		public const float SELECTION_RANGE = 5f;

		[Header("Settings")]
		public LayerMask DetectionMask;

		public Color HoverOutlineColor;

		private Type TypeRequirement;

		private Action<NPC> callback;

		private NPC hoveredNPC;

		private NPC highlightedNPC;

		public bool IsOpen { get; protected set; }

		private void Start()
		{
			GameInput.RegisterExitListener(Exit, 12);
			Singleton<ManagementClipboard>.Instance.onClipboardUnequipped.AddListener(ClipboardClosed);
		}

		public virtual void Open(string selectionTitle, Type typeRequirement, Action<NPC> _callback)
		{
			IsOpen = true;
			TypeRequirement = typeRequirement;
			callback = _callback;
			Singleton<HUD>.Instance.ShowTopScreenText(selectionTitle);
			Singleton<ManagementInterface>.Instance.EquippedClipboard.OverrideClipboardText(selectionTitle);
			Singleton<ManagementClipboard>.Instance.Close(preserveState: true);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("npcselector");
		}

		public virtual void Close(bool returnToClipboard)
		{
			IsOpen = false;
			if (Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "npcselector")
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			Singleton<HUD>.Instance.HideTopScreenText();
			if (highlightedNPC != null)
			{
				highlightedNPC.HideOutline();
				highlightedNPC = null;
			}
			if (returnToClipboard)
			{
				Singleton<ManagementInterface>.Instance.EquippedClipboard.EndOverride();
				Singleton<ManagementClipboard>.Instance.Open(Singleton<ManagementInterface>.Instance.Configurables, Singleton<ManagementInterface>.Instance.EquippedClipboard);
			}
		}

		private void Update()
		{
			if (!IsOpen)
			{
				return;
			}
			hoveredNPC = GetHoveredNPC();
			if (hoveredNPC != null && IsNPCTypeValid(hoveredNPC))
			{
				if (hoveredNPC != highlightedNPC)
				{
					if (highlightedNPC != null)
					{
						highlightedNPC.HideOutline();
						highlightedNPC = null;
					}
					highlightedNPC = hoveredNPC;
					highlightedNPC.ShowOutline(HoverOutlineColor);
				}
			}
			else if (highlightedNPC != null)
			{
				highlightedNPC.HideOutline();
				highlightedNPC = null;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && hoveredNPC != null && IsNPCTypeValid(hoveredNPC))
			{
				NPCClicked(hoveredNPC);
			}
		}

		private NPC GetHoveredNPC()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(5f, out var hit, DetectionMask, includeTriggers: false, 0.1f))
			{
				return hit.collider.GetComponentInParent<NPC>();
			}
			return null;
		}

		public bool IsNPCTypeValid(NPC npc)
		{
			if (TypeRequirement == null)
			{
				return true;
			}
			return TypeRequirement.IsAssignableFrom(npc.GetType());
		}

		public void NPCClicked(NPC npc)
		{
			if (IsNPCTypeValid(npc))
			{
				callback?.Invoke(hoveredNPC);
				Close(returnToClipboard: true);
			}
		}

		private void ClipboardClosed()
		{
			Close(returnToClipboard: false);
		}

		private void Exit(ExitAction exitAction)
		{
			if (IsOpen && !exitAction.used && exitAction.exitType == ExitType.Escape)
			{
				exitAction.used = true;
				Close(returnToClipboard: true);
			}
		}
	}
}
