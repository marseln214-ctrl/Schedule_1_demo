using FishNet.Serializing.Helping;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class ChemistryCookOperation
	{
		[CodegenExclude]
		private StationRecipe recipe;

		public string RecipeID;

		public EQuality ProductQuality;

		public Color StartLiquidColor;

		public float LiquidLevel;

		[CodegenExclude]
		public StationRecipe Recipe
		{
			get
			{
				if (recipe == null)
				{
					recipe = Singleton<ChemistryStationCanvas>.Instance.Recipes.Find((StationRecipe r) => r.RecipeID == RecipeID);
				}
				return recipe;
			}
		}

		[CodegenExclude]
		public int CurrentTime { get; private set; }

		public ChemistryCookOperation(StationRecipe recipe, EQuality productQuality, Color startLiquidColor, float liquidLevel, int currentTime = 0)
		{
			RecipeID = recipe.RecipeID;
			ProductQuality = productQuality;
			StartLiquidColor = startLiquidColor;
			LiquidLevel = liquidLevel;
			CurrentTime = currentTime;
		}

		public ChemistryCookOperation(string recipeID, EQuality productQuality, Color startLiquidColor, float liquidLevel, int currentTime = 0)
		{
			RecipeID = recipeID;
			ProductQuality = productQuality;
			StartLiquidColor = startLiquidColor;
			LiquidLevel = liquidLevel;
			CurrentTime = currentTime;
		}

		public ChemistryCookOperation()
		{
		}

		public void Progress(int mins)
		{
			CurrentTime += mins;
			_ = CurrentTime;
			_ = Recipe.CookTime_Mins;
		}
	}
}
