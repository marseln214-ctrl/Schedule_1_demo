using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E98_CurvyCamController : SplineController
	{
		[Section("Curvy Cam", true, false, 100)]
		public float MinSpeed;

		public float MaxSpeed;

		public float Mass;

		public float Down;

		public float Up;

		public float Fric = 0.9f;

		protected override void OnEnable()
		{
			base.OnEnable();
			base.Speed = MinSpeed;
		}

		protected override void Advance(float speed, float deltaTime)
		{
			base.Advance(speed, deltaTime);
			Vector3 tangent = GetTangent(base.RelativePosition);
			float num = ((!(tangent.y < 0f)) ? (Up * (0f - tangent.y) * Fric) : (Down * tangent.y * Fric));
			base.Speed = Mathf.Clamp(base.Speed + Mass * num * deltaTime, MinSpeed, MaxSpeed);
			if (base.RelativePosition == 1f)
			{
				base.Speed = 0f;
			}
		}
	}
}
