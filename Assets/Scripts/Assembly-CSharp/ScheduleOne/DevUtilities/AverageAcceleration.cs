using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class AverageAcceleration : MonoBehaviour
	{
		public Rigidbody Rb;

		public float TimeWindow = 0.5f;

		private Vector3[] accelerations;

		private int currentIndex;

		private float timer;

		private Vector3 prevVelocity;

		public Vector3 Acceleration { get; private set; } = Vector3.zero;


		private void Start()
		{
			if (Rb == null)
			{
				Rb = GetComponent<Rigidbody>();
			}
			accelerations = new Vector3[Mathf.CeilToInt(TimeWindow / Time.fixedDeltaTime)];
			for (int i = 0; i < accelerations.Length; i++)
			{
				accelerations[i] = Vector3.zero;
			}
			prevVelocity = Rb.velocity;
		}

		private void FixedUpdate()
		{
			timer += Time.fixedDeltaTime;
			if (timer >= TimeWindow)
			{
				timer -= Time.fixedDeltaTime;
				accelerations[currentIndex] = Vector3.zero;
				currentIndex = (currentIndex + 1) % accelerations.Length;
			}
			Vector3 vector = (Rb.velocity - prevVelocity) / Time.fixedDeltaTime;
			accelerations[currentIndex] = vector;
			prevVelocity = Rb.velocity;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < accelerations.Length; i++)
			{
				zero += accelerations[i];
			}
			Acceleration = zero / accelerations.Length;
		}
	}
}
