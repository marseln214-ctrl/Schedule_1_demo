using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class SensitivitySlider : SettingsSlider
	{
		public const float MULTIPLIER = 1f / 30f;

		protected virtual void Start()
		{
			slider.SetValueWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings.MouseSensitivity / (1f / 30f));
		}

		protected override void OnValueChanged(float value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings.MouseSensitivity = value * (1f / 30f);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadInputSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteInputSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings);
		}
	}
}
