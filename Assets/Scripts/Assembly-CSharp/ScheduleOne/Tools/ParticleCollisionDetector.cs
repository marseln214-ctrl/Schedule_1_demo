using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	public class ParticleCollisionDetector : MonoBehaviour
	{
		public UnityEvent<GameObject> onCollision = new UnityEvent<GameObject>();

		public void OnParticleCollision(GameObject other)
		{
			if (onCollision != null)
			{
				onCollision.Invoke(other);
			}
		}
	}
}
