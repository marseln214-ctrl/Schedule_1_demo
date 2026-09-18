using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class ItemSelector : ClipboardScreen
	{
		[Serializable]
		public class Option
		{
			public string Title;

			public ItemDefinition Item;

			public Option(string title, ItemDefinition item)
			{
				Title = title;
				Item = item;
			}
		}

		[Header("References")]
		public RectTransform OptionContainer;

		public TextMeshProUGUI TitleLabel;

		public TextMeshProUGUI HoveredItemLabel;

		public GameObject OptionPrefab;

		[Header("Settings")]
		public Sprite EmptyOptionSprite;

		private Coroutine lerpRoutine;

		private List<Option> options = new List<Option>();

		private Option selectedOption;

		private List<RectTransform> optionButtons = new List<RectTransform>();

		private Action<Option> optionCallback;

		public void Initialize(string selectionTitle, List<Option> _options, Option _selectedOption = null, Action<Option> _optionCallback = null)
		{
			TitleLabel.text = selectionTitle;
			options = new List<Option>();
			options.AddRange(_options);
			selectedOption = _selectedOption;
			optionCallback = _optionCallback;
			DeleteOptions();
			CreateOptions(options);
			HoveredItemLabel.enabled = false;
		}

		public override void Open()
		{
			base.Open();
			Singleton<ManagementInterface>.Instance.MainScreen.Close();
		}

		public override void Close()
		{
			base.Close();
			HoveredItemLabel.enabled = false;
			Singleton<ManagementInterface>.Instance.MainScreen.Open();
		}

		private void ButtonClicked(Option option)
		{
			if (optionCallback != null)
			{
				optionCallback(option);
			}
			Close();
		}

		private void ButtonHovered(Option option)
		{
			HoveredItemLabel.text = option.Title;
			HoveredItemLabel.enabled = true;
			HoveredItemLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f - Mathf.Ceil((float)optionButtons.Count / 5f) * optionButtons[0].sizeDelta.y);
		}

		private void ButtonHoverEnd(Option option)
		{
			HoveredItemLabel.enabled = false;
		}

		private void CreateOptions(List<Option> options)
		{
			for (int i = 0; i < options.Count; i++)
			{
				Button component = UnityEngine.Object.Instantiate(OptionPrefab, OptionContainer).GetComponent<Button>();
				if (options[i].Item != null)
				{
					component.transform.Find("None").gameObject.SetActive(value: false);
					component.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = options[i].Item.Icon;
					component.transform.Find("Icon").gameObject.SetActive(value: true);
				}
				else
				{
					component.transform.Find("None").gameObject.SetActive(value: true);
					component.transform.Find("Icon").gameObject.SetActive(value: false);
				}
				if (options[i] == selectedOption)
				{
					component.transform.Find("Outline").gameObject.GetComponent<Image>().color = new Color32(90, 90, 90, byte.MaxValue);
				}
				Option opt = options[i];
				component.onClick.AddListener(delegate
				{
					ButtonClicked(opt);
				});
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback.AddListener(delegate
				{
					ButtonHovered(opt);
				});
				component.GetComponent<EventTrigger>().triggers.Add(entry);
				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerExit;
				entry.callback.AddListener(delegate
				{
					ButtonHoverEnd(opt);
				});
				component.GetComponent<EventTrigger>().triggers.Add(entry);
				optionButtons.Add(component.GetComponent<RectTransform>());
			}
		}

		private void DeleteOptions()
		{
			for (int i = 0; i < optionButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(optionButtons[i].gameObject);
			}
			optionButtons.Clear();
		}
	}
}
