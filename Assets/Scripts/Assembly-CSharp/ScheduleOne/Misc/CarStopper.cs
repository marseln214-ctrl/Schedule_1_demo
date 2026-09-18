using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Misc
{
	public class CarStopper : MonoBehaviour
	{
		public bool isActive;

		[Header("References")]
		[SerializeField]
		protected Transform blocker;

		public NavMeshObstacle Obstacle;

		private float moveTime = 0.5f;

		protected virtual void Update()
		{
			float num = 70f;
			if (isActive)
			{
				Obstacle.enabled = true;
				blocker.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(blocker.localEulerAngles.z + Time.deltaTime * num / moveTime, 0f, num));
			}
			else
			{
				Obstacle.enabled = false;
				blocker.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(blocker.localEulerAngles.z - Time.deltaTime * num / moveTime, 0f, num));
			}
		}
	}
}
