using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class CauldronConfigPanel : ConfigPanel
	{
		[Header("References")]
		public ObjectFieldUI DestinationUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<ObjectField> list = new List<ObjectField>();
			foreach (CauldronConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to CauldronConfiguration");
					return;
				}
				list.Add(config.Destination);
			}
			DestinationUI.Bind(list);
		}
	}
}
