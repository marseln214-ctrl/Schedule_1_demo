using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	[ExecuteAlways]
	public class ChaseCam : MonoBehaviour
	{
		public Transform LookAt;

		public Transform MoveTo;

		public Transform RollTo;

		[Positive]
		public float ChaseTime = 0.5f;

		private Vector3 mVelocity;

		private Vector3 mRollVelocity;

		[UsedImplicitly]
		private void LateUpdate()
		{
			if ((bool)MoveTo)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, MoveTo.position, ref mVelocity, ChaseTime);
			}
			if ((bool)LookAt)
			{
				if (!RollTo)
				{
					base.transform.LookAt(LookAt);
				}
				else
				{
					base.transform.LookAt(LookAt, Vector3.SmoothDamp(base.transform.up, RollTo.up, ref mRollVelocity, ChaseTime));
				}
			}
		}
	}
}
