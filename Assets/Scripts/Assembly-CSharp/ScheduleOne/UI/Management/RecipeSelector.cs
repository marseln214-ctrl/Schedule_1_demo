using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class RecipeSelector : ClipboardScreen
	{
		[Header("References")]
		public RectTransform OptionContainer;

		public TextMeshProUGUI TitleLabel;

		public GameObject OptionPrefab;

		[Header("Settings")]
		public Sprite EmptyOptionSprite;

		private Coroutine lerpRoutine;

		private List<StationRecipe> options = new List<StationRecipe>();

		private StationRecipe selectedOption;

		private List<RectTransform> optionButtons = new List<RectTransform>();

		private Action<StationRecipe> optionCallback;

		public void Initialize(string selectionTitle, List<StationRecipe> _options, StationRecipe _selectedOption = null, Action<StationRecipe> _optionCallback = null)
		{
			TitleLabel.text = selectionTitle;
			options = new List<StationRecipe>();
			options.AddRange(_options);
			selectedOption = _selectedOption;
			optionCallback = _optionCallback;
			DeleteOptions();
			CreateOptions(options);
		}

		public override void Open()
		{
			base.Open();
			Debug.Log(Container.gameObject.name + " is active: " + Container.gameObject.activeSelf);
			Singleton<ManagementInterface>.Instance.MainScreen.Close();
		}

		public override void Close()
		{
			base.Close();
			Debug.Log("Closed");
			Singleton<ManagementInterface>.Instance.MainScreen.Open();
		}

		private void ButtonClicked(StationRecipe option)
		{
			if (optionCallback != null)
			{
				optionCallback(option);
			}
			Close();
		}

		private void CreateOptions(List<StationRecipe> options)
		{
			options.Sort((StationRecipe a, StationRecipe b) => a.RecipeTitle.CompareTo(b.RecipeTitle));
			for (int i = 0; i < options.Count; i++)
			{
				StationRecipeEntry component = UnityEngine.Object.Instantiate(OptionPrefab, OptionContainer).GetComponent<StationRecipeEntry>();
				component.AssignRecipe(options[i]);
				if (options[i] == selectedOption)
				{
					component.transform.Find("Selected").gameObject.GetComponent<Image>().color = new Color32(90, 90, 90, byte.MaxValue);
				}
				StationRecipe opt = options[i];
				component.Button.onClick.AddListener(delegate
				{
					ButtonClicked(opt);
				});
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
