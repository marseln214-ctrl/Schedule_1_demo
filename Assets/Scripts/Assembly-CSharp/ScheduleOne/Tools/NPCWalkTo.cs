using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.Tools
{
	[RequireComponent(typeof(NPCMovement))]
	public class NPCWalkTo : MonoBehaviour
	{
		public Transform Target;

		public float RepathRate = 0.5f;

		private float timeSinceLastPath;

		private void Update()
		{
			timeSinceLastPath += Time.deltaTime;
			if (timeSinceLastPath >= RepathRate)
			{
				timeSinceLastPath = 0f;
				GetComponent<NPCMovement>().SetDestination(Target.position);
			}
		}
	}
}
