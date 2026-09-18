using FluffyUnderware.Curvy.Controllers;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	[RequireComponent(typeof(Rigidbody))]
	public class E11_RigidBodySplineController : MonoBehaviour
	{
		public CurvySpline Spline;

		public SplineController CameraController;

		public float VSpeed = 10f;

		public float HSpeed = 0.5f;

		public float CenterDrag = 0.5f;

		public float JumpForce = 10f;

		private Rigidbody mRigidBody;

		private float mTF;

		private float velocity;

		[UsedImplicitly]
		private void Start()
		{
			mRigidBody = GetComponent<Rigidbody>();
		}

		[UsedImplicitly]
		private void LateUpdate()
		{
			if ((bool)CameraController)
			{
				float target = Spline.TFToDistance(mTF) - 5f;
				CameraController.AbsolutePosition = Mathf.SmoothDamp(CameraController.AbsolutePosition, target, ref velocity, 0.5f);
			}
		}

		[UsedImplicitly]
		private void FixedUpdate()
		{
			if ((bool)Spline)
			{
				float num = Input.GetAxis("Vertical") * VSpeed;
				float num2 = Input.GetAxis("Horizontal") * HSpeed;
				mTF = Spline.GetNearestPointTF(base.transform.localPosition, out var nearestPoint);
				if (num != 0f)
				{
					mRigidBody.AddForce(Spline.GetTangentFast(mTF) * num, ForceMode.Force);
				}
				if (num2 != 0f)
				{
					Vector3 vector = Spline.InterpolateFast(mTF) + Quaternion.AngleAxis(90f, Spline.GetTangentFast(mTF)) * Spline.GetOrientationUpFast(mTF);
					Vector3 vector2 = nearestPoint - vector;
					mRigidBody.AddForce(vector2 * num2, ForceMode.Force);
				}
				if (Input.GetKeyDown(KeyCode.Space))
				{
					mRigidBody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
				}
				mRigidBody.AddForce((Spline.Interpolate(mTF) - base.transform.localPosition) * CenterDrag, ForceMode.VelocityChange);
			}
		}
	}
}
