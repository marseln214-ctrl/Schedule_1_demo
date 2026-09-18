using System.ComponentModel;

namespace FluffyUnderware.Curvy.Controllers
{
	public class CurvySplineMoveEventArgs : CancelEventArgs
	{
		public SplineController Sender { get; private set; }

		public CurvySpline Spline { get; private set; }

		public CurvySplineSegment ControlPoint { get; private set; }

		public bool WorldUnits { get; private set; }

		public MovementDirection MovementDirection { get; private set; }

		public float Delta { get; private set; }

		public float Position { get; private set; }

		public CurvySplineMoveEventArgs(SplineController sender, CurvySpline spline, CurvySplineSegment controlPoint, float position, bool usingWorldUnits, float delta, MovementDirection direction)
		{
			Set_INTERNAL(sender, spline, controlPoint, position, delta, direction, usingWorldUnits);
		}

		internal void Set_INTERNAL(SplineController sender, CurvySpline spline, CurvySplineSegment controlPoint, float position, float delta, MovementDirection direction, bool usingWorldUnits)
		{
			Sender = sender;
			Spline = spline;
			ControlPoint = controlPoint;
			MovementDirection = direction;
			Delta = delta;
			Position = position;
			WorldUnits = usingWorldUnits;
			base.Cancel = false;
		}
	}
}
