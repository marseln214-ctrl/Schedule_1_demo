using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class ChemistConfigPanel : ConfigPanel
	{
		[Header("References")]
		public ObjectFieldUI BedUI;

		public ObjectListFieldUI StationsUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<ObjectField> list = new List<ObjectField>();
			List<ObjectListField> list2 = new List<ObjectListField>();
			foreach (ChemistConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to BotanistConfiguration");
					return;
				}
				list.Add(config.Bed);
				list2.Add(config.Stations);
			}
			BedUI.Bind(list);
			StationsUI.Bind(list2);
		}
	}
}
