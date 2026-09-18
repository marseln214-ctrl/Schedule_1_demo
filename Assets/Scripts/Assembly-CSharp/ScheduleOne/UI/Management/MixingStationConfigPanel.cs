using System.Collections.Generic;
using ScheduleOne.Management;
using ScheduleOne.Management.UI;
using UnityEngine;

namespace ScheduleOne.UI.Management
{
	public class MixingStationConfigPanel : ConfigPanel
	{
		[Header("References")]
		public ObjectFieldUI DestinationUI;

		public NumberFieldUI StartThresholdUI;

		public override void Bind(List<EntityConfiguration> configs)
		{
			List<ObjectField> list = new List<ObjectField>();
			List<NumberField> list2 = new List<NumberField>();
			foreach (MixingStationConfiguration config in configs)
			{
				if (config == null)
				{
					Console.LogError("Failed to cast EntityConfiguration to MixingStationConfiguration");
					return;
				}
				list.Add(config.Destination);
				list2.Add(config.StartThrehold);
			}
			DestinationUI.Bind(list);
			StartThresholdUI.Bind(list2);
		}
	}
}
