using UnityEngine;

namespace LiquidVolumeFX
{
	public class RandomRotation : MonoBehaviour
	{
		[Range(1f, 50f)]
		public float speed = 10f;

		[Range(1f, 30f)]
		public float randomChangeInterval = 10f;

		private float lastTime;

		private Vector3 v;

		private float randomization;

		private void Start()
		{
			randomization = Random.value;
		}

		private void Update()
		{
			if (Time.time > lastTime)
			{
				lastTime = Time.time + randomChangeInterval + randomization;
				v = new Vector3(Random.value, Random.value, Random.value);
			}
			base.transform.Rotate(v * Time.deltaTime * speed);
		}
	}
}
