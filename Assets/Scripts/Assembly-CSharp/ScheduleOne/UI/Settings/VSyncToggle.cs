using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;

namespace ScheduleOne.UI.Settings
{
	public class VSyncToggle : SettingsToggle
	{
		protected virtual void OnEnable()
		{
			toggle.SetIsOnWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.VSync);
		}

		protected override void OnValueChanged(bool value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.VSync = value;
			GetComponentInParent<SettingsScreen>().DisplayChanged();
		}
	}
}
