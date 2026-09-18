using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class ProgressSlider : Singleton<ProgressSlider>
	{
		[Header("References")]
		public GameObject Container;

		public TextMeshProUGUI Label;

		public Slider Slider;

		public Image SliderFill;

		private bool progressSetThisFrame;

		private void LateUpdate()
		{
			if (progressSetThisFrame)
			{
				Container.SetActive(value: true);
				progressSetThisFrame = false;
			}
			else
			{
				Container.SetActive(value: false);
			}
		}

		public void ShowProgress(float progress)
		{
			progressSetThisFrame = true;
			Slider.value = progress;
		}

		public void Configure(string label, Color sliderFillColor)
		{
			Label.text = label;
			Label.color = sliderFillColor;
			SliderFill.color = sliderFillColor;
		}
	}
}
