using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Controllers
{
	public abstract class ConnectedControlPointsSelector : DTVersionedMonoBehaviour
	{
		public abstract CurvySplineSegment SelectConnectedControlPoint(SplineController caller, CurvyConnection connection, CurvySplineSegment currentControlPoint);
	}
}
