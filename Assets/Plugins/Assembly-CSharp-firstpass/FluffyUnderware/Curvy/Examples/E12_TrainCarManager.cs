using FluffyUnderware.Curvy.Controllers;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	[ExecuteAlways]
	public class E12_TrainCarManager : MonoBehaviour
	{
		public SplineController Waggon;

		public SplineController FrontAxis;

		public SplineController BackAxis;

		private E12_TrainManager mTrain;

		public float Position
		{
			get
			{
				return Waggon.AbsolutePosition;
			}
			set
			{
				if (Waggon.AbsolutePosition != value)
				{
					Waggon.AbsolutePosition = value;
					FrontAxis.AbsolutePosition = value + mTrain.AxisDistance / 2f;
					BackAxis.AbsolutePosition = value - mTrain.AxisDistance / 2f;
				}
			}
		}

		[UsedImplicitly]
		private void LateUpdate()
		{
			if ((bool)mTrain && BackAxis.Spline == FrontAxis.Spline && FrontAxis.RelativePosition > BackAxis.RelativePosition)
			{
				float absolutePosition = Waggon.AbsolutePosition;
				float absolutePosition2 = FrontAxis.AbsolutePosition;
				float absolutePosition3 = BackAxis.AbsolutePosition;
				if (Mathf.Abs(Mathf.Abs(absolutePosition2 - absolutePosition3) - mTrain.AxisDistance) >= mTrain.Limit)
				{
					FrontAxis.AbsolutePosition = absolutePosition + mTrain.AxisDistance / 2f;
					BackAxis.AbsolutePosition = absolutePosition - mTrain.AxisDistance / 2f;
				}
			}
		}

		public void setup()
		{
			mTrain = GetComponentInParent<E12_TrainManager>();
			if ((bool)mTrain.Spline)
			{
				setController(Waggon, mTrain.Spline, mTrain.Speed);
				setController(FrontAxis, mTrain.Spline, mTrain.Speed);
				setController(BackAxis, mTrain.Spline, mTrain.Speed);
			}
		}

		private void setController(SplineController c, CurvySpline spline, float speed)
		{
			c.Spline = spline;
			c.Speed = speed;
			c.OnControlPointReached.AddListenerOnce(OnCPReached);
		}

		public void OnCPReached(CurvySplineMoveEventArgs e)
		{
			E12_MDJunctionControl metadata = e.ControlPoint.GetMetadata<E12_MDJunctionControl>();
			e.Sender.ConnectionBehavior = ((!metadata || metadata.UseJunction) ? SplineControllerConnectionBehavior.RandomSpline : SplineControllerConnectionBehavior.CurrentSpline);
		}
	}
}
