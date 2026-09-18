using System.Collections.Generic;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class PatrolGroup
	{
		public List<NPC> Members = new List<NPC>();

		public FootPatrolRoute Route;

		public int CurrentWaypoint;

		public PatrolGroup(FootPatrolRoute route)
		{
			Route = route;
			CurrentWaypoint = route.StartWaypointIndex;
		}

		public Vector3 GetDestination(NPC member)
		{
			if (!Members.Contains(member))
			{
				Console.LogWarning(member.name + " is not a member of this patrol group!");
				return member.transform.position;
			}
			return Route.Waypoints[CurrentWaypoint].TransformPoint(GetMemberOffset(member));
		}

		public void DisbandGroup()
		{
			foreach (NPC item in new List<NPC>(Members))
			{
				(item as PoliceOfficer).FootPatrolBehaviour.Disable_Networked(null);
				(item as PoliceOfficer).FootPatrolBehaviour.End_Networked(null);
			}
		}

		public void AdvanceGroup()
		{
			CurrentWaypoint++;
			if (CurrentWaypoint == Route.Waypoints.Length)
			{
				CurrentWaypoint = 0;
			}
		}

		private Vector3 GetMemberOffset(NPC member)
		{
			if (!Members.Contains(member))
			{
				Console.LogWarning(member.name + " is not a member of this patrol group!");
				return Vector3.zero;
			}
			int num = Members.IndexOf(member);
			Vector3 zero = Vector3.zero;
			zero.z -= (float)num * 1f;
			zero.x += ((num % 2 == 0) ? 0.6f : (-0.6f));
			return zero;
		}

		public bool IsGroupReadyToAdvance()
		{
			for (int i = 0; i < Members.Count; i++)
			{
				if (!(Members[i] as PoliceOfficer).FootPatrolBehaviour.IsReadyToAdvance())
				{
					return false;
				}
			}
			return true;
		}

		public bool IsPaused()
		{
			for (int i = 0; i < Members.Count; i++)
			{
				if (Members[i].behaviour.activeBehaviour == null || Members[i].behaviour.activeBehaviour.GetType() != typeof(FootPatrolBehaviour))
				{
					return true;
				}
			}
			return false;
		}
	}
}
