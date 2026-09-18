using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvySplineEventArgs : CurvyEventArgs
	{
		public readonly CurvySpline Spline;

		public CurvySplineEventArgs(MonoBehaviour sender, CurvySpline spline, object data = null)
			: base(sender, data)
		{
			Spline = spline;
		}
	}
}
