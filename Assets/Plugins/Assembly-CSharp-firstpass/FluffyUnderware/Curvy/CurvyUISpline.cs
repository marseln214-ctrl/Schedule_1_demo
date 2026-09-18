using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Curvy/Curvy UI Spline")]
	[HelpURL("https://curvyeditor.com/doclink/curvyuispline")]
	public class CurvyUISpline : CurvySpline
	{
		public static CurvyUISpline CreateUISpline(string gameObjectName = "Curvy UI Spline")
		{
			CurvyUISpline component = new GameObject(gameObjectName, typeof(CurvyUISpline)).GetComponent<CurvyUISpline>();
			component.SetupUISpline();
			return component;
		}

		protected override void Reset()
		{
			base.Reset();
			SetupUISpline();
		}

		private void SetupUISpline()
		{
			base.RestrictTo2D = true;
			base.MaxPointsPerUnit = 1f;
			base.Orientation = CurvyOrientation.None;
		}
	}
}
