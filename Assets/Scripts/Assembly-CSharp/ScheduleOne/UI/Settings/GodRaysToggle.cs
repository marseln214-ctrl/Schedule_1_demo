using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class GodRaysToggle : SettingsToggle
	{
		protected virtual void Start()
		{
			toggle.SetIsOnWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.GodRays);
		}

		protected override void OnValueChanged(bool value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.GodRays = value;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadGraphicsSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteGraphicsSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings);
		}
	}
}
