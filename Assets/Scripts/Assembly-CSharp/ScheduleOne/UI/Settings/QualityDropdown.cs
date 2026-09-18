using System;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class QualityDropdown : SettingsDropdown
	{
		protected override void Awake()
		{
			base.Awake();
			GraphicsSettings.EGraphicsQuality[] array = (GraphicsSettings.EGraphicsQuality[])Enum.GetValues(typeof(GraphicsSettings.EGraphicsQuality));
			for (int i = 0; i < array.Length; i++)
			{
				string option = array[i].ToString();
				AddOption(option);
			}
		}

		protected virtual void Start()
		{
			dropdown.SetValueWithoutNotify((int)Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.GraphicsQuality);
		}

		protected override void OnValueChanged(int value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.GraphicsQuality = (GraphicsSettings.EGraphicsQuality)value;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadGraphicsSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteGraphicsSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings);
		}
	}
}
