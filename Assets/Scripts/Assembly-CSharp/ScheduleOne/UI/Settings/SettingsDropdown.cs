using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Settings
{
	public class SettingsDropdown : MonoBehaviour
	{
		public string[] DefaultOptions;

		protected TMP_Dropdown dropdown;

		protected virtual void Awake()
		{
			dropdown = GetComponent<TMP_Dropdown>();
			dropdown.onValueChanged.AddListener(OnValueChanged);
			string[] defaultOptions = DefaultOptions;
			foreach (string option in defaultOptions)
			{
				AddOption(option);
			}
		}

		protected virtual void OnValueChanged(int value)
		{
		}

		protected void AddOption(string option)
		{
			dropdown.options.Add(new TMP_Dropdown.OptionData(option));
		}
	}
}
