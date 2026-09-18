using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACAssetPathReplicator<T> : ACReplicator where T : Object
	{
		private ACSelection<T> selection;

		protected virtual void Awake()
		{
			selection = GetComponent<ACSelection<T>>();
		}

		protected override void AvatarSettingsChanged(AvatarSettings newSettings)
		{
			base.AvatarSettingsChanged(newSettings);
			selection.SelectOption(selection.GetAssetPathIndex((string)newSettings[propertyName]), notify: false);
		}
	}
}
