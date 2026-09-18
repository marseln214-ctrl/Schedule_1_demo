using System;
using ScheduleOne.Management.Presets;
using UnityEngine;

namespace ScheduleOne.Management.Objects
{
	public abstract class ManageableObject : MonoBehaviour
	{
		public abstract ManageableObjectType GetObjectType();

		public abstract Preset GetCurrentPreset();

		public void SetPreset(Preset newPreset)
		{
			if (GetCurrentPreset() != null)
			{
				Preset currentPreset = GetCurrentPreset();
				currentPreset.onDeleted = (Preset.PresetDeletion)Delegate.Remove(currentPreset.onDeleted, new Preset.PresetDeletion(ExistingPresetDeleted));
			}
			SetPreset_Internal(newPreset);
		}

		protected virtual void SetPreset_Internal(Preset preset)
		{
			preset.onDeleted = (Preset.PresetDeletion)Delegate.Combine(preset.onDeleted, new Preset.PresetDeletion(ExistingPresetDeleted));
		}

		public void ExistingPresetDeleted(Preset replacement)
		{
			SetPreset(replacement);
		}
	}
}
