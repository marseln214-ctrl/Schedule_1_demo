using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Settings
{
	public class SettingsToggle : MonoBehaviour
	{
		protected Toggle toggle;

		protected virtual void Awake()
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(OnValueChanged);
		}

		protected virtual void OnValueChanged(bool value)
		{
		}
	}
}
