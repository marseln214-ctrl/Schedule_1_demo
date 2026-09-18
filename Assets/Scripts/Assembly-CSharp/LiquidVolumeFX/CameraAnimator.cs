using UnityEngine;

namespace LiquidVolumeFX
{
	public class CameraAnimator : MonoBehaviour
	{
		public float baseHeight = 0.6f;

		public float speedY = 0.005f;

		public float speedX = 5f;

		public float distAcceleration = 0.0002f;

		public float distSpeed = 0.0001f;

		public Vector3 lookAt;

		private float y;

		private float dy;

		private float distDirection = 1f;

		private float distSum;

		private void Start()
		{
			y = base.transform.position.y;
		}

		private void Update()
		{
			base.transform.RotateAround(lookAt, Vector3.up, Time.deltaTime * speedX);
			y += dy;
			dy -= (base.transform.position.y - baseHeight) * Time.deltaTime * speedY;
			base.transform.position = new Vector3(base.transform.position.x, y, base.transform.position.z);
			Quaternion rotation = base.transform.rotation;
			base.transform.LookAt(lookAt);
			base.transform.rotation = Quaternion.Lerp(rotation, base.transform.rotation, 0.2f);
			base.transform.position += base.transform.forward * distSum;
			distSum += distSpeed;
			distDirection = ((distSum < 0f) ? 1f : (-1f));
			distSpeed += Time.deltaTime * distDirection * distAcceleration;
		}
	}
}
