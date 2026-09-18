using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Misc;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class ManagementClipboard_Equippable : Equippable_Viewmodel
	{
		[Header("References")]
		public Transform Clipboard;

		public Transform LoweredPosition;

		public Transform RaisedPosition;

		public ToggleableLight Light;

		public SelectionInfoUI SelectionInfo;

		public TextMeshProUGUI OverrideText;

		private Coroutine moveRoutine;

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			Singleton<ManagementWorldspaceCanvas>.Instance.Open();
			Clipboard.transform.position = LoweredPosition.position;
			OverrideText.gameObject.SetActive(value: false);
			SelectionInfo.gameObject.SetActive(value: true);
			Singleton<ManagementClipboard>.Instance.IsEquipped = true;
			Singleton<ManagementClipboard>.Instance.onOpened.AddListener(FullscreenEnter);
			Singleton<ManagementClipboard>.Instance.onClosed.AddListener(FullscreenExit);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("clipboard");
			if (Singleton<ManagementClipboard>.Instance.onClipboardEquipped != null)
			{
				Singleton<ManagementClipboard>.Instance.onClipboardEquipped.Invoke();
			}
		}

		public override void Unequip()
		{
			base.Unequip();
			if (Singleton<ManagementClipboard>.Instance.IsOpen)
			{
				Singleton<ManagementClipboard>.Instance.Close();
			}
			Singleton<ManagementWorldspaceCanvas>.Instance.Close();
			Singleton<ManagementClipboard>.Instance.IsEquipped = false;
			if (Singleton<ManagementClipboard>.Instance.onClipboardUnequipped != null)
			{
				Singleton<ManagementClipboard>.Instance.onClipboardUnequipped.Invoke();
			}
			if (Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "clipboard")
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!GameInput.GetButtonDown(GameInput.ButtonCode.Interact) || GameInput.IsTyping || !(Singleton<InteractionManager>.Instance.hoveredValidInteractableObject == null))
			{
				return;
			}
			if (Singleton<ManagementClipboard>.Instance.IsOpen)
			{
				Singleton<ManagementClipboard>.Instance.Close();
				return;
			}
			List<IConfigurable> list = new List<IConfigurable>();
			list.AddRange(Singleton<ManagementWorldspaceCanvas>.Instance.SelectedConfigurables);
			if (Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable != null && !list.Contains(Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable))
			{
				list.Add(Singleton<ManagementWorldspaceCanvas>.Instance.HoveredConfigurable);
			}
			Singleton<ManagementClipboard>.Instance.Open(list, this);
		}

		private void FullscreenEnter()
		{
			Singleton<ManagementWorldspaceCanvas>.Instance.Close(preserveSelection: true);
			Clipboard.gameObject.SetActive(value: false);
			if (Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "clipboard")
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
		}

		private void FullscreenExit()
		{
			Clipboard.gameObject.SetActive(value: true);
			if (!Singleton<ManagementClipboard>.Instance.IsOpen && !Singleton<ManagementClipboard>.Instance.StatePreserved)
			{
				Singleton<ManagementWorldspaceCanvas>.Instance.Open();
				Singleton<InputPromptsCanvas>.Instance.LoadModule("clipboard");
			}
		}

		public void OverrideClipboardText(string overriddenText)
		{
			OverrideText.text = overriddenText;
			OverrideText.gameObject.SetActive(value: true);
			SelectionInfo.gameObject.SetActive(value: false);
		}

		public void EndOverride()
		{
			OverrideText.gameObject.SetActive(value: false);
			SelectionInfo.gameObject.SetActive(value: true);
		}
	}
}
