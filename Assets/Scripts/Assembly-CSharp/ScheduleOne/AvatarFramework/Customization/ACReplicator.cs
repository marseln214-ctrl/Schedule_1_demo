using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACReplicator : MonoBehaviour
	{
		public string propertyName = string.Empty;

		private void Start()
		{
			CustomizationManager instance = Singleton<CustomizationManager>.Instance;
			instance.OnAvatarSettingsChanged = (CustomizationManager.AvatarSettingsChanged)Delegate.Combine(instance.OnAvatarSettingsChanged, new CustomizationManager.AvatarSettingsChanged(AvatarSettingsChanged));
		}

		protected virtual void AvatarSettingsChanged(AvatarSettings newSettings)
		{
		}
	}
}
