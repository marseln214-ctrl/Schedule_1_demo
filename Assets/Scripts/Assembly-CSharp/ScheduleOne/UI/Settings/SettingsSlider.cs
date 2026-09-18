using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Settings
{
	public class SettingsSlider : MonoBehaviour
	{
		private const float VALUE_DISPLAY_TIME = 2f;

		public bool DisplayValue = true;

		protected Slider slider;

		protected TextMeshProUGUI valueLabel;

		protected float timeOnValueChange = -100f;

		protected virtual void Awake()
		{
			slider = GetComponent<Slider>();
			slider.onValueChanged.AddListener(OnValueChanged);
			valueLabel = slider.handleRect.Find("Value").GetComponent<TextMeshProUGUI>();
		}

		protected virtual void Update()
		{
			if (DisplayValue && Time.time - timeOnValueChange > 2f)
			{
				valueLabel.enabled = false;
			}
		}

		protected virtual void OnValueChanged(float value)
		{
			timeOnValueChange = Time.time;
			if (DisplayValue)
			{
				valueLabel.text = GetDisplayValue(value);
				valueLabel.enabled = true;
			}
		}

		protected virtual string GetDisplayValue(float value)
		{
			return value.ToString();
		}
	}
}
