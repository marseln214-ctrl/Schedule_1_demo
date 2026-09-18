using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class BrickPressConfigPanel : ConfigPanel
	{
		[Header("References")]
		public ObjectFieldUI DestinationUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<ObjectField> list = new List<ObjectField>();
			foreach (BrickPressConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to BrickPressConfiguration");
					return;
				}
				list.Add(config.Destination);
			}
			DestinationUI.Bind(list);
		}
	}
}
