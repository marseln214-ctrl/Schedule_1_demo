using UnityEngine.UI;

namespace ScheduleOne.AvatarFramework.Customization
{
	public class ACSliderReplicator : ACReplicator
	{
		public Slider slider;

		protected override void AvatarSettingsChanged(AvatarSettings newSettings)
		{
			base.AvatarSettingsChanged(newSettings);
			slider.SetValueWithoutNotify((float)newSettings[propertyName]);
		}
	}
}
