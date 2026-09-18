using System.Collections.Generic;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.StationFramework;
using UnityEngine.Events;

namespace ScheduleOne.Management
{
	public class StationRecipeField : ConfigField
	{
		public List<StationRecipe> Options = new List<StationRecipe>();

		public UnityEvent<StationRecipe> onRecipeChanged = new UnityEvent<StationRecipe>();

		public StationRecipe SelectedRecipe { get; protected set; }

		public StationRecipeField(EntityConfiguration parentConfig)
			: base(parentConfig)
		{
		}

		public void SetRecipe(StationRecipe recipe, bool network)
		{
			SelectedRecipe = recipe;
			if (network)
			{
				base.ParentConfig.ReplicateField(this);
			}
			if (onRecipeChanged != null)
			{
				onRecipeChanged.Invoke(SelectedRecipe);
			}
		}

		public override bool IsValueDefault()
		{
			return SelectedRecipe == null;
		}

		public StationRecipeFieldData GetData()
		{
			return new StationRecipeFieldData((SelectedRecipe != null) ? SelectedRecipe.RecipeID.ToString() : "");
		}

		public void Load(StationRecipeFieldData data)
		{
			if (data != null && !string.IsNullOrEmpty(data.RecipeID))
			{
				SelectedRecipe = Options.Find((StationRecipe x) => x.RecipeID == data.RecipeID);
			}
		}
	}
}
