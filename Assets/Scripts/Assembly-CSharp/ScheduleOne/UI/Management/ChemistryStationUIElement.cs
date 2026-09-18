using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class ChemistryStationUIElement : WorldspaceUIElement
	{
		[Header("References")]
		public StationRecipeEntry RecipeEntry;

		public GameObject NoRecipe;

		public ChemistryStation AssignedStation { get; protected set; }

		public void Initialize(ChemistryStation oven)
		{
			AssignedStation = oven;
			AssignedStation.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			ChemistryStationConfiguration chemistryStationConfiguration = AssignedStation.Configuration as ChemistryStationConfiguration;
			SetAssignedNPC(chemistryStationConfiguration.AssignedChemist.SelectedNPC);
			if (chemistryStationConfiguration.Recipe.SelectedRecipe != null)
			{
				RecipeEntry.AssignRecipe(chemistryStationConfiguration.Recipe.SelectedRecipe);
				RecipeEntry.gameObject.SetActive(value: true);
				NoRecipe.SetActive(value: false);
			}
			else
			{
				RecipeEntry.gameObject.SetActive(value: false);
				NoRecipe.SetActive(value: true);
			}
		}
	}
}
