using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E10_MotorController : SplineController
	{
		[Section("Motor", true, false, 100)]
		public float MaxSpeed = 30f;

		protected override void Update()
		{
			float axis = Input.GetAxis("Vertical");
			base.Speed = Mathf.Abs(axis) * MaxSpeed;
			base.MovementDirection = MovementDirectionMethods.FromInt((int)Mathf.Sign(axis));
			base.Update();
		}
	}
}
