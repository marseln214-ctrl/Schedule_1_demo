using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class PotConfigPanel : ConfigPanel
	{
		[Header("References")]
		public ItemFieldUI SeedUI;

		public ItemFieldUI Additive1UI;

		public ItemFieldUI Additive2UI;

		public ItemFieldUI Additive3UI;

		public ObjectFieldUI DestinationUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<ItemField> list = new List<ItemField>();
			List<ItemField> list2 = new List<ItemField>();
			List<ItemField> list3 = new List<ItemField>();
			List<ItemField> list4 = new List<ItemField>();
			List<NPCField> list5 = new List<NPCField>();
			List<ObjectField> list6 = new List<ObjectField>();
			foreach (PotConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to PotConfiguration");
					return;
				}
				list.Add(config.Seed);
				list2.Add(config.Additive1);
				list3.Add(config.Additive2);
				list4.Add(config.Additive3);
				list5.Add(config.AssignedBotanist);
				list6.Add(config.Destination);
			}
			SeedUI.Bind(list);
			Additive1UI.Bind(list2);
			Additive2UI.Bind(list3);
			Additive3UI.Bind(list4);
			DestinationUI.Bind(list6);
		}
	}
}
