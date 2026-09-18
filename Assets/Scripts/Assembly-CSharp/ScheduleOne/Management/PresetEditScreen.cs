using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management.Presets;
using ScheduleOne.Management.Presets.Options.SetterScreens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Management
{
	public class PresetEditScreen : MonoBehaviour
	{
		[Serializable]
		public class OptionData
		{
			public GameObject OptionEntryPrefab;

			public OptionSetterScreen OptionSetterScreen;
		}

		public Preset EditedPreset;

		[Header("References")]
		public RectTransform IconBackgroundRect;

		public Image IconBackground;

		public RectTransform InputFieldRect;

		public TMP_InputField InputField;

		public RectTransform EditButtonRect;

		public Button ReturnButton;

		public Button DeleteButton;

		public bool isOpen => EditedPreset != null;

		protected virtual void Awake()
		{
			ReturnButton.onClick.AddListener(ReturnButtonClicked);
			DeleteButton.onClick.AddListener(DeleteButtonClicked);
			InputField.onValueChanged.AddListener(NameFieldChange);
			InputField.onEndEdit.AddListener(NameFieldDone);
			GameInput.RegisterExitListener(Exit, 4);
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && isOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public virtual void Open(Preset preset)
		{
			EditedPreset = preset;
			InputField.text = EditedPreset.PresetName;
			Canvas.ForceUpdateCanvases();
			RefreshIcon();
			RefreshTransforms();
			base.gameObject.SetActive(value: true);
			StartCoroutine(Delay());
			IEnumerator Delay()
			{
				yield return new WaitForEndOfFrame();
				RefreshTransforms();
			}
		}

		public void Close()
		{
			EditedPreset = null;
			base.gameObject.SetActive(value: false);
		}

		private void RefreshIcon()
		{
			IconBackground.color = EditedPreset.PresetColor;
		}

		private void RefreshTransforms()
		{
			InputField.ForceLabelUpdate();
			InputField.textComponent.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
			float renderedWidth = InputField.textComponent.renderedWidth;
			if (InputField.text == string.Empty)
			{
				renderedWidth = ((TextMeshProUGUI)InputField.placeholder).renderedWidth;
			}
			InputFieldRect.sizeDelta = new Vector2(renderedWidth + 3f, InputFieldRect.sizeDelta.y);
			InputFieldRect.anchoredPosition = new Vector2(1.5f, InputFieldRect.anchoredPosition.y);
			float num = 1.75f;
			float min = 5f;
			IconBackgroundRect.anchoredPosition = new Vector2(0f - Mathf.Clamp(renderedWidth / 2f + num, min, float.MaxValue), IconBackgroundRect.anchoredPosition.y);
			EditButtonRect.anchoredPosition = new Vector2(Mathf.Clamp(renderedWidth / 2f + num, min, float.MaxValue), IconBackgroundRect.anchoredPosition.y);
		}

		private void NameFieldChange(string newVal)
		{
			RefreshTransforms();
		}

		private void NameFieldDone(string piss)
		{
			if (IsNameAppropriate(piss))
			{
				EditedPreset.SetName(piss);
				return;
			}
			InputField.text = EditedPreset.PresetName;
			RefreshTransforms();
		}

		private bool IsNameAppropriate(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}
			if (name == string.Empty)
			{
				return false;
			}
			if (name == "Pablo")
			{
				return false;
			}
			return true;
		}

		public void DeleteButtonClicked()
		{
			EditedPreset.DeletePreset(Preset.GetDefault(EditedPreset.ObjectType));
			Close();
		}

		public void ReturnButtonClicked()
		{
			Close();
		}
	}
}
