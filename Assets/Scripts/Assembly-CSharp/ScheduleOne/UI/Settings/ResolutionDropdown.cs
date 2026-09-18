using ScheduleOne.DevUtilities;
using ScheduleOne.UI.MainMenu;
using UnityEngine;

namespace ScheduleOne.UI.Settings
{
	public class ResolutionDropdown : SettingsDropdown
	{
		protected override void Awake()
		{
			base.Awake();
			Resolution[] array = DisplaySettings.GetResolutions().ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Resolution resolution = array[i];
				AddOption(resolution.width + "x" + resolution.height);
			}
		}

		protected virtual void OnEnable()
		{
			dropdown.SetValueWithoutNotify(Mathf.Clamp(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings.ResolutionIndex, 0, dropdown.options.Count - 1));
		}

		protected override void OnValueChanged(int value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings.ResolutionIndex = value;
			GetComponentInParent<SettingsScreen>().DisplayChanged();
		}
	}
}
