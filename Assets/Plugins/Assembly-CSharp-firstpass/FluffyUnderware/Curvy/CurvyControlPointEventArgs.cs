using UnityEngine;

namespace FluffyUnderware.Curvy
{
	public class CurvyControlPointEventArgs : CurvySplineEventArgs
	{
		public enum ModeEnum
		{
			None = 0,
			AddBefore = 1,
			AddAfter = 2,
			Delete = 3
		}

		public readonly ModeEnum Mode;

		public readonly CurvySplineSegment ControlPoint;

		public CurvyControlPointEventArgs(MonoBehaviour sender, CurvySpline spline, CurvySplineSegment cp, ModeEnum mode = ModeEnum.None, object data = null)
			: base(sender, spline, data)
		{
			ControlPoint = cp;
			Mode = mode;
		}
	}
}
