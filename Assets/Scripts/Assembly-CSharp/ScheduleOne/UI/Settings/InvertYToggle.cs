using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class InvertYToggle : SettingsToggle
	{
		protected virtual void Start()
		{
			toggle.SetIsOnWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings.InvertMouse);
		}

		protected override void OnValueChanged(bool value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings.InvertMouse = value;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadInputSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteInputSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings);
		}
	}
}
