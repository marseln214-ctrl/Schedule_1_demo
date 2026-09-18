using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings
{
	public class IntefaceScaleSlider : SettingsSlider
	{
		public const float MULTIPLIER = 0.1f;

		public const float MinScale = 0.7f;

		public const float MaxScale = 1.4f;

		protected virtual void OnEnable()
		{
			slider.minValue = 7f;
			slider.maxValue = 14f;
			slider.SetValueWithoutNotify(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.UIScale / 0.1f);
		}

		protected override void OnValueChanged(float value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.UIScale = value * 0.1f;
			GetComponentInParent<SettingsScreen>().DisplayChanged();
		}

		protected override string GetDisplayValue(float value)
		{
			return Mathf.Round(value * 10f) + "%";
		}
	}
}
