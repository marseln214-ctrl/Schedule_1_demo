using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Examples
{
	[ExecuteAlways]
	public class E05_PanelUpdater : MonoBehaviour
	{
		public CurvySpline Spline;

		public Text StatisticsText;

		public Slider Density;

		[UsedImplicitly]
		private void Start()
		{
			StartCoroutine(UpdateCoroutine());
		}

		private IEnumerator UpdateCoroutine()
		{
			while (true)
			{
				TryUpdateDisplay();
				yield return new WaitForSeconds(0.25f);
			}
		}

		private void TryUpdateDisplay()
		{
			if ((bool)Spline && Spline.IsInitialized && !Spline.Dirty)
			{
				StatisticsText.text = $"Red Curve Cache Points: {Spline.CacheSize} \nFrame rate: {1f / Time.smoothDeltaTime:000}";
			}
		}

		public void OnSliderChange()
		{
			Spline.CacheDensity = (int)Density.value;
		}
	}
}
