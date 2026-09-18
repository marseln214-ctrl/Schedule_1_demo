using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class FootPatrolRoute : MonoBehaviour
	{
		[Header("Settings")]
		public string RouteName = "Foot patrol route";

		public Color PathColor = Color.red;

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
			Gizmos.color = PathColor;
			for (int j = 0; j < Waypoints.Length - 1; j++)
			{
				if (!(Waypoints[j] == null))
				{
					Gizmos.DrawLine(Waypoints[j].position + Vector3.up * 0.5f, Waypoints[j + 1].position + Vector3.up * 0.5f);
				}
			}
		}

		private void OnValidate()
		{
			UpdateWaypoints();
		}

		private void UpdateWaypoints()
		{
			Waypoints = base.transform.GetComponentsInChildren<Transform>();
			Waypoints = Waypoints.Remove(base.transform);
		}
	}
}
