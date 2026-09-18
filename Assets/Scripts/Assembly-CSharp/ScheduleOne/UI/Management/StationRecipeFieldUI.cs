using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class StationRecipeFieldUI : MonoBehaviour
	{
		[Header("References")]
		public StationRecipeEntry RecipeEntry;

		public GameObject None;

		public GameObject Mixed;

		public GameObject ClearButton;

		public List<StationRecipeField> Fields { get; protected set; } = new List<StationRecipeField>();


		public void Bind(List<StationRecipeField> field)
		{
			Fields = new List<StationRecipeField>();
			Fields.AddRange(field);
			Fields[Fields.Count - 1].onRecipeChanged.AddListener(Refresh);
			Refresh(Fields[0].SelectedRecipe);
		}

		private void Refresh(StationRecipe newVal)
		{
			None.gameObject.SetActive(value: false);
			Mixed.gameObject.SetActive(value: false);
			ClearButton.gameObject.SetActive(value: false);
			RecipeEntry.gameObject.SetActive(value: false);
			if (AreFieldsUniform())
			{
				if (newVal != null)
				{
					ClearButton.gameObject.SetActive(value: true);
					RecipeEntry.AssignRecipe(newVal);
					RecipeEntry.gameObject.SetActive(value: true);
				}
				else
				{
					None.SetActive(value: true);
				}
			}
			else
			{
				Mixed.gameObject.SetActive(value: true);
				ClearButton.gameObject.SetActive(value: true);
			}
			ClearButton.gameObject.SetActive(value: false);
		}

		private bool AreFieldsUniform()
		{
			for (int i = 0; i < Fields.Count - 1; i++)
			{
				if (Fields[i].SelectedRecipe != Fields[i + 1].SelectedRecipe)
				{
					return false;
				}
			}
			return true;
		}

		public void Clicked()
		{
			bool num = AreFieldsUniform();
			StationRecipe selectedOption = null;
			if (num)
			{
				selectedOption = Fields[0].SelectedRecipe;
			}
			List<StationRecipe> options = Fields[0].Options.Where((StationRecipe x) => x.Unlocked).ToList();
			Singleton<ManagementInterface>.Instance.RecipeSelectorScreen.Initialize("Select Recipe", options, selectedOption, OptionSelected);
			Singleton<ManagementInterface>.Instance.RecipeSelectorScreen.Open();
		}

		private void OptionSelected(StationRecipe option)
		{
			foreach (StationRecipeField field in Fields)
			{
				field.SetRecipe(option, network: true);
			}
		}

		public void ClearClicked()
		{
			foreach (StationRecipeField field in Fields)
			{
				field.SetRecipe(null, network: true);
			}
		}
	}
}
