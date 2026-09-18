using UnityEngine;

namespace LiquidVolumeFX
{
	public class VerticalBounce : MonoBehaviour
	{
		[Range(0f, 0.1f)]
		public float acceleration = 0.1f;

		private float direction = 1f;

		private float y;

		private float speed = 0.01f;

		private void Update()
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, y, base.transform.localPosition.z);
			y += speed;
			direction = ((y < 0f) ? 1f : (-1f));
			speed += Time.deltaTime * direction * acceleration;
		}
	}
}
