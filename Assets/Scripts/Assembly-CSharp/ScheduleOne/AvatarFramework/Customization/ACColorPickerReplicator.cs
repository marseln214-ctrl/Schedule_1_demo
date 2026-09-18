using HSVPicker;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACColorPickerReplicator : ACReplicator
	{
		public HSVPicker.ColorPicker picker;

		protected override void AvatarSettingsChanged(AvatarSettings newSettings)
		{
			base.AvatarSettingsChanged(newSettings);
			picker.CurrentColor = (Color)newSettings[propertyName];
		}
	}
}
