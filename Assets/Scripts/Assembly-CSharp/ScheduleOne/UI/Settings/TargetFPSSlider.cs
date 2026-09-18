using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings
{
	public class TargetFPSSlider : SettingsSlider
	{
		protected virtual void OnEnable()
		{
			slider.SetValueWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.TargetFPS);
		}

		protected override void OnValueChanged(float value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.TargetFPS = Mathf.RoundToInt(value);
			GetComponentInParent<SettingsScreen>().DisplayChanged();
		}
	}
}
