using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class BotanistConfigPanel : ConfigPanel
	{
		[Header("References")]
		public ObjectFieldUI BedUI;

		public ObjectFieldUI SuppliesUI;

		public ObjectListFieldUI PotsUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<ObjectField> list = new List<ObjectField>();
			List<ObjectField> list2 = new List<ObjectField>();
			List<ObjectListField> list3 = new List<ObjectListField>();
			foreach (BotanistConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to BotanistConfiguration");
					return;
				}
				list.Add(config.Bed);
				list2.Add(config.Supplies);
				list3.Add(config.Pots);
			}
			BedUI.Bind(list);
			SuppliesUI.Bind(list2);
			PotsUI.Bind(list3);
		}
	}
}
