using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class ChemistryStationConfigPanel : ConfigPanel
	{
		[Header("References")]
		public StationRecipeFieldUI RecipeUI;

		public ObjectFieldUI DestinationUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<StationRecipeField> list = new List<StationRecipeField>();
			List<ObjectField> list2 = new List<ObjectField>();
			foreach (ChemistryStationConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to ChemistryStationConfiguration");
					return;
				}
				list2.Add(config.Destination);
				list.Add(config.Recipe);
			}
			RecipeUI.Bind(list);
			DestinationUI.Bind(list2);
		}
	}
}
