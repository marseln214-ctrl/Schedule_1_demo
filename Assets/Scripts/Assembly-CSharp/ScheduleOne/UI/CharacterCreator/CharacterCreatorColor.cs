using System.Collections.Generic;
using ScheduleOne.Clothing;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator
{
	public class CharacterCreatorColor : CharacterCreatorField<Color>
	{
		public static EClothingColor[] ClothingColorsToUse = new EClothingColor[20]
		{
			EClothingColor.White,
			EClothingColor.LightGrey,
			EClothingColor.DarkGrey,
			EClothingColor.Charcoal,
			EClothingColor.Black,
			EClothingColor.Red,
			EClothingColor.Crimson,
			EClothingColor.Orange,
			EClothingColor.Tan,
			EClothingColor.Brown,
			EClothingColor.Yellow,
			EClothingColor.Lime,
			EClothingColor.DarkGreen,
			EClothingColor.Cyan,
			EClothingColor.SkyBlue,
			EClothingColor.Blue,
			EClothingColor.Navy,
			EClothingColor.Purple,
			EClothingColor.Magenta,
			EClothingColor.BrightPink
		};

		[Header("References")]
		public RectTransform OptionContainer;

		[Header("Settings")]
		public bool UseClothingColors;

		public List<Color> Colors;

		public GameObject OptionPrefab;

		private List<Button> optionButtons = new List<Button>();

		private Button selectedButton;

		protected override void Awake()
		{
			base.Awake();
			if (UseClothingColors)
			{
				Colors = new List<Color>();
				EClothingColor[] clothingColorsToUse = ClothingColorsToUse;
				foreach (EClothingColor color in clothingColorsToUse)
				{
					Colors.Add(color.GetActualColor());
				}
			}
			for (int j = 0; j < Colors.Count; j++)
			{
				GameObject gameObject = Object.Instantiate(OptionPrefab, OptionContainer);
				gameObject.transform.Find("Color").GetComponent<Image>().color = Colors[j];
				Color col = Colors[j];
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					OptionClicked(col);
				});
				optionButtons.Add(gameObject.GetComponent<Button>());
			}
		}

		public override void ApplyValue()
		{
			base.ApplyValue();
			Button button = null;
			for (int i = 0; i < Colors.Count; i++)
			{
				if (base.value == Colors[i] && i < optionButtons.Count)
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

		public void OptionClicked(Color color)
		{
			base.value = color;
			WriteValue();
		}
	}
}
