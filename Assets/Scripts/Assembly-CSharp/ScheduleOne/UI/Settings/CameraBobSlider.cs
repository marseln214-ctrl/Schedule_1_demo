using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;

namespace ScheduleOne.UI.Settings
{
	public class CameraBobSlider : SettingsSlider
	{
		protected virtual void Start()
		{
			slider.SetValueWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.CameraBobbing * 10f);
		}

		protected override void OnValueChanged(float value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.CameraBobbing = value / 10f;
			GetComponentInParent<SettingsScreen>().DisplayChanged();
		}
	}
}
