using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	public class E29_ControlPointMover : MonoBehaviour
	{
		private Vector3 originalPosition;

		public float Variation;

		public float Magnitude = 3f;

		public float Period = 3f;

		[UsedImplicitly]
		private void Start()
		{
			originalPosition = base.transform.position;
		}

		[UsedImplicitly]
		private void Update()
		{
			Vector3 position = originalPosition;
			position.x += Magnitude * Mathf.Sin(Variation + Time.time * Period);
			position.z += Magnitude * Mathf.Cos(Variation + Time.time * Period);
			base.transform.position = position;
		}
	}
}
