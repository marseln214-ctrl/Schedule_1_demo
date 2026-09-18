using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class VehiclePatrolRoute : MonoBehaviour
	{
		[Header("Settings")]
		public string RouteName = "Vehicle patrol route";

		public Transform[] Waypoints;

		public int StartWaypointIndex;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(base.transform.position + Vector3.up * 0.5f, 0.5f);
			Gizmos.color = Color.yellow;
			for (int i = 0; i < Waypoints.Length; i++)
			{
				if (!(Waypoints[i] == null))
				{
					Gizmos.DrawWireSphere(Waypoints[i].position + Vector3.up * 0.5f, 0.5f);
				}
			}
			Gizmos.color = Color.red;
			for (int j = 0; j < Waypoints.Length - 1; j++)
			{
				if (!(Waypoints[j] == null))
				{
					Gizmos.DrawLine(Waypoints[j].position + Vector3.up * 0.5f, Waypoints[j + 1].position + Vector3.up * 0.5f);
				}
			}
		}
	}
}
