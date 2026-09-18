using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Clothing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator
{
	public class CharacterCreatorOptionList : CharacterCreatorField<string>
	{
		[Serializable]
		public class Option
		{
			public string Label;

			public string AssetPath;

			public ClothingDefinition ClothingItemEquivalent;
		}

		[Header("References")]
		public RectTransform OptionContainer;

		[Header("Settings")]
		public bool CanSelectNone = true;

		public List<Option> Options;

		public GameObject OptionPrefab;

		private List<Button> optionButtons = new List<Button>();

		private Button selectedButton;

		protected override void Awake()
		{
			base.Awake();
			if (CanSelectNone)
			{
				Options.Insert(0, new Option
				{
					AssetPath = "",
					Label = "None"
				});
			}
			for (int i = 0; i < Options.Count; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(OptionPrefab, OptionContainer);
				gameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Options[i].Label;
				string option = Options[i].AssetPath;
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					OptionClicked(option);
				});
				optionButtons.Add(gameObject.GetComponent<Button>());
			}
		}

		public override void ApplyValue()
		{
			base.ApplyValue();
			Button button = null;
			for (int i = 0; i < Options.Count && i < optionButtons.Count; i++)
			{
				if (base.value == Options[i].AssetPath)
				{
					button = optionButtons[i];
					break;
				}
			}
			if (selectedButton != null)
			{
				selectedButton.interactable = true;
			}
			selectedButton = button;
			if (selectedButton != null)
			{
				selectedButton.interactable = false;
			}
		}

		public void OptionClicked(string option)
		{
			base.value = option;
			Option option2 = Options.FirstOrDefault((Option o) => o.AssetPath == option);
			if (option2 != null)
			{
				selectedClothingDefinition = option2.ClothingItemEquivalent;
			}
			else
			{
				selectedClothingDefinition = null;
			}
			WriteValue();
		}
	}
}
