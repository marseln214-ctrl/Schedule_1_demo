using System;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Settings
{
	public class AntiAliasingDropdown : SettingsDropdown
	{
		protected override void Awake()
		{
			base.Awake();
			GraphicsSettings.EAntiAliasingMode[] array = (GraphicsSettings.EAntiAliasingMode[])Enum.GetValues(typeof(GraphicsSettings.EAntiAliasingMode));
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].ToString();
				text = text.Replace("MSAAx2", "2x MSAA");
				text = text.Replace("MSAAx4", "4x MSAA");
				text = text.Replace("MSAAx8", "8x MSAA");
				AddOption(text);
			}
		}

		protected virtual void Start()
		{
			dropdown.SetValueWithoutNotify((int)Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.AntiAliasingMode);
		}

		protected override void OnValueChanged(int value)
		{
			base.OnValueChanged(value);
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings.AntiAliasingMode = (GraphicsSettings.EAntiAliasingMode)value;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ReloadGraphicsSettings();
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.WriteGraphicsSettings(Singleton<ScheduleOne.DevUtilities.Settings>.Instance.GraphicsSettings);
		}
	}
}
