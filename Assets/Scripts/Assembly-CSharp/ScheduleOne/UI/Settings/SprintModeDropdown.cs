using System;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class SprintModeDropdown : SettingsDropdown
	{
		protected override void Awake()
		{
			base.Awake();
			InputSettings.EActionMode[] array = (InputSettings.EActionMode[])Enum.GetValues(typeof(InputSettings.EActionMode));
			for (int i = 0; i < array.Length; i++)
			{
				string option = array[i].ToString();
				AddOption(option);
			}
		}

		protected virtual void Start()
		{
			dropdown.SetValueWithoutNotify((int)Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings.SprintMode);
		}

		protected override void OnValueChanged(int value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings.SprintMode = (InputSettings.EActionMode)value;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadInputSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteInputSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.InputSettings);
		}
	}
}
