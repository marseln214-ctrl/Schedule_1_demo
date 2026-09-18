using UnityEngine;

namespace ScheduleOne.Tools
{
	public class SmoothRotate : MonoBehaviour
	{
		public bool Active = true;

		public float Speed = 5f;

		public float Aceleration = 2f;

		public Vector3 Axis = Vector3.up;

		private float currentSpeed;

		private void Update()
		{
			if (Active)
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, Speed, Aceleration * Time.deltaTime);
			}
			else
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Aceleration * Time.deltaTime);
			}
			base.transform.Rotate(Axis, currentSpeed * Time.deltaTime, Space.Self);
		}

		public void SetActive(bool active)
		{
			Active = active;
		}
	}
}
