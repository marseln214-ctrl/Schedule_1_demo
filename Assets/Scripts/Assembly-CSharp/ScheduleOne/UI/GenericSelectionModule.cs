using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class GenericSelectionModule : Singleton<GenericSelectionModule>
	{
		[Header("References")]
		public Canvas canvas;

		public TextMeshProUGUI TitleText;

		public RectTransform OptionContainer;

		public Button CloseButton;

		[Header("Prefabs")]
		public GameObject ListOptionPrefab;

		[HideInInspector]
		public bool OptionChosen;

		public bool isOpen { get; protected set; }

		[HideInInspector]
		public int ChosenOptionIndex { get; protected set; } = -1;


		protected override void Awake()
		{
			base.Awake();
			Close();
		}

		protected override void Start()
		{
			base.Start();
			GameInput.RegisterExitListener(Exit, 50);
		}

		private void Exit(ExitAction action)
		{
			if (isOpen && !action.used && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Cancel();
			}
		}

		public void Open(string title, List<string> options)
		{
			isOpen = true;
			OptionChosen = false;
			ChosenOptionIndex = -1;
			ClearOptions();
			TitleText.text = title;
			for (int i = 0; i < options.Count; i++)
			{
				RectTransform component = Object.Instantiate(ListOptionPrefab, OptionContainer).GetComponent<RectTransform>();
				component.Find("Label").GetComponent<TextMeshProUGUI>().text = options[i];
				component.anchoredPosition = new Vector2(0f, (0f - ((float)i + 0.5f)) * component.sizeDelta.y);
				int index = i;
				component.GetComponent<Button>().onClick.AddListener(delegate
				{
					ListOptionClicked(index);
				});
			}
			canvas.enabled = true;
		}

		public void Close()
		{
			isOpen = false;
			canvas.enabled = false;
			ClearOptions();
		}

		public void Cancel()
		{
			ChosenOptionIndex = -1;
			OptionChosen = true;
			Close();
		}

		private void ClearOptions()
		{
			int childCount = OptionContainer.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Object.Destroy(OptionContainer.GetChild(0).gameObject);
			}
		}

		private void ListOptionClicked(int index)
		{
			ChosenOptionIndex = index;
			OptionChosen = true;
			Close();
		}
	}
}
