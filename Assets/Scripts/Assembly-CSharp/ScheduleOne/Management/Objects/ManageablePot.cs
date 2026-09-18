using ScheduleOne.Management.Presets;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace ScheduleOne.Management.Objects
{
	[RequireComponent(typeof(Pot))]
	public class ManageablePot : ManageableObject
	{
		public PotPreset CurrentPreset;

		protected virtual void Awake()
		{
			CurrentPreset = PotPreset.GetDefaultPreset();
		}

		public override ManageableObjectType GetObjectType()
		{
			return ManageableObjectType.Pot;
		}

		public override Preset GetCurrentPreset()
		{
			return CurrentPreset;
		}

		protected override void SetPreset_Internal(Preset newPreset)
		{
			base.SetPreset_Internal(newPreset);
			PotPreset potPreset = (PotPreset)newPreset;
			if (potPreset == null)
			{
				Console.LogWarning("SetPreset_Internal: preset is not the right type");
			}
			else
			{
				CurrentPreset = potPreset;
			}
		}
	}
}
