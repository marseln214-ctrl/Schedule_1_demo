using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;

namespace ScheduleOne.UI.Settings
{
	public class DisplayModeDropdown : SettingsDropdown
	{
		protected override void Awake()
		{
			base.Awake();
			DisplaySettings.EDisplayMode[] array = (DisplaySettings.EDisplayMode[])Enum.GetValues(typeof(DisplaySettings.EDisplayMode));
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].ToString();
				text = text.Replace("ExclusiveFullscreen", "Exclusive Fullscreen");
				text = text.Replace("FullscreenWindow", "Fullscreen Window");
				AddOption(text);
			}
		}

		protected virtual void OnEnable()
		{
			dropdown.SetValueWithoutNotify((int)Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.DisplayMode);
		}

		protected override void OnValueChanged(int value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.DisplayMode = (DisplaySettings.EDisplayMode)value;
			GetComponentInParent<SettingsScreen>().DisplayChanged();
		}
	}
}
