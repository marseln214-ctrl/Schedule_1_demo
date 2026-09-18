using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Examples
{
	[ExecuteAlways]
	public class E05_MoveToNearestPoint : MonoBehaviour
	{
		public Transform Lookup;

		public CurvySpline Spline;

		public Text StatisticsText;

		public Slider Density;

		private readonly TimeMeasure Timer = new TimeMeasure(30);

		[UsedImplicitly]
		private void Update()
		{
			if ((bool)Spline && Spline.IsInitialized && (bool)Lookup && !Spline.Dirty)
			{
				Timer.Start();
				base.transform.position = Spline.GetNearestPoint(Lookup.position, Space.World);
				Timer.Stop();
				StatisticsText.text = $"Blue Curve Cache Points: {Spline.CacheSize} \nAverage Lookup (ms): {Timer.AverageMS:0.000}";
			}
		}

		public void OnSliderChange()
		{
			Spline.CacheDensity = (int)Density.value;
		}
	}
}
