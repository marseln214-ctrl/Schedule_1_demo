using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class FOVSLider : SettingsSlider
	{
		protected virtual void Start()
		{
			slider.SetValueWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.FOV);
		}

		protected override void OnValueChanged(float value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.FOV = value;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadGraphicsSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteGraphicsSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings);
		}
	}
}
