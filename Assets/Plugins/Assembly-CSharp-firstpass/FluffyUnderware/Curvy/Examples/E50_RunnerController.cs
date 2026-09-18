using System.Collections;
using System.Linq;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E50_RunnerController : SplineController
	{
		private enum GuideMode
		{
			Guided = 0,
			Jumping = 1,
			FreeFall = 2
		}

		[Section("Jump", true, false, 100)]
		public float JumpHeight = 20f;

		public float JumpSpeed = 0.5f;

		public AnimationCurve JumpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float Gravity = 10f;

		private GuideMode mMode;

		private float jumpHeight;

		private float fallingSpeed;

		private E50_SplineRefMetadata mPossibleSwitchTarget;

		private int mSwitchInProgress;

		protected override void OnDisable()
		{
			base.OnDisable();
			StopAllCoroutines();
		}

		protected override void InitializedApplyDeltaTime(float deltaTime)
		{
			if (Input.GetButtonDown("Fire1") && mMode == GuideMode.Guided)
			{
				StartCoroutine(Jump());
			}
			if (mPossibleSwitchTarget != null && mSwitchInProgress == 0)
			{
				float axisRaw = Input.GetAxisRaw("Horizontal");
				if (mPossibleSwitchTarget.Options == "Right" && axisRaw > 0f)
				{
					Switch(1);
				}
				else if (mPossibleSwitchTarget.Options == "Left" && axisRaw < 0f)
				{
					Switch(-1);
				}
			}
			else if (mSwitchInProgress != 0 && !Switcher.IsSwitching)
			{
				mSwitchInProgress = 0;
				OnCPReached(new CurvySplineMoveEventArgs(this, Spline, Spline.TFToSegment(base.RelativePosition), 0f, usingWorldUnits: false, 0f, MovementDirection.Forward));
			}
			base.InitializedApplyDeltaTime(deltaTime);
			if (mMode == GuideMode.FreeFall)
			{
				fallingSpeed += Gravity * deltaTime;
				base.OffsetRadius -= fallingSpeed;
				if (base.OffsetRadius <= 0f)
				{
					mMode = GuideMode.Guided;
					fallingSpeed = 0f;
					base.OffsetRadius = 0f;
				}
			}
			if (mMode == GuideMode.Jumping)
			{
				base.OffsetRadius = jumpHeight;
			}
		}

		private void Switch(int dir)
		{
			mSwitchInProgress = dir;
			Vector3 vector = mPossibleSwitchTarget.Spline.transform.InverseTransformPoint(base.transform.position);
			Vector3 nearestPoint;
			float nearestPointTF = mPossibleSwitchTarget.Spline.GetNearestPointTF(vector, out nearestPoint, mPossibleSwitchTarget.CP.Spline.GetSegmentIndex(mPossibleSwitchTarget.CP));
			float duration = (nearestPoint - vector).magnitude / base.Speed;
			SwitchTo(mPossibleSwitchTarget.Spline, nearestPointTF, duration);
		}

		private IEnumerator Jump()
		{
			mMode = GuideMode.Jumping;
			float start = Time.time;
			float f = 0f;
			while (f < 1f && mMode == GuideMode.Jumping)
			{
				f = (Time.time - start) / JumpSpeed;
				jumpHeight = JumpCurve.Evaluate(f) * JumpHeight;
				yield return new WaitForEndOfFrame();
			}
			if (mMode == GuideMode.Jumping)
			{
				mMode = GuideMode.Guided;
			}
		}

		public void OnCPReached(CurvySplineMoveEventArgs e)
		{
			mPossibleSwitchTarget = e.ControlPoint.GetMetadata<E50_SplineRefMetadata>();
			if ((bool)mPossibleSwitchTarget && !mPossibleSwitchTarget.Spline)
			{
				mPossibleSwitchTarget = null;
			}
		}

		public void UseFollowUpOrFall(CurvySplineMoveEventArgs e)
		{
			CurvySplineSegment controlPoint = e.ControlPoint;
			if (controlPoint == e.Spline.FirstVisibleControlPoint && (bool)controlPoint.Connection && !controlPoint.FollowUp)
			{
				CurvySplineSegment curvySplineSegment = controlPoint.Connection.ControlPointsList.Where((CurvySplineSegment cp) => cp != controlPoint).First();
				float f = controlPoint.transform.position.y - curvySplineSegment.transform.position.y;
				mMode = GuideMode.FreeFall;
				fallingSpeed = 0f;
				base.OffsetRadius += Mathf.Abs(f);
			}
		}
	}
}
