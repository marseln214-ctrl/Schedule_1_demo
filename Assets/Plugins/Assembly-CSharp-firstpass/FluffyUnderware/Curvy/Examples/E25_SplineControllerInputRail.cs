using FluffyUnderware.Curvy.Controllers;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E25_SplineControllerInputRail : MonoBehaviour
	{
		public float acceleration = 0.1f;

		public float limit = 30f;

		public SplineController splineController;

		[UsedImplicitly]
		private void Update()
		{
			float num = Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 1f);
			splineController.Speed = Mathf.Clamp(splineController.Speed + num * acceleration * Time.deltaTime, 0.001f, limit);
		}
	}
}
