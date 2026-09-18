using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;

namespace ScheduleOne.Law
{
	public class LawManager : Singleton<LawManager>
	{
		public const int DISPATCH_OFFICER_COUNT = 2;

		public static float DISPATCH_VEHICLE_USE_THRESHOLD = 25f;

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(delegate
			{
				PoliceOfficer.Officers.Clear();
			});
		}

		public void PoliceCalled(Player target, Crime crime)
		{
			if (!NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				Console.Log("Police called on " + target.PlayerName);
				PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(target.CrimeData.LastKnownPosition);
				target.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
				closestPoliceStation.Dispatch(2, target);
			}
		}

		public PatrolGroup StartFootpatrol(FootPatrolRoute route, int requestedMembers)
		{
			PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(route.Waypoints[route.StartWaypointIndex].position);
			if (closestPoliceStation.OfficerPool.Count == 0)
			{
				Console.LogWarning(closestPoliceStation.name + " has no officers in its pool!");
				return null;
			}
			PatrolGroup patrolGroup = new PatrolGroup(route);
			List<PoliceOfficer> list = new List<PoliceOfficer>();
			for (int i = 0; i < requestedMembers; i++)
			{
				if (closestPoliceStation.OfficerPool.Count == 0)
				{
					break;
				}
				list.Add(closestPoliceStation.PullOfficer());
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].StartFootPatrol(patrolGroup, warpToStartPoint: false);
			}
			return patrolGroup;
		}

		public PoliceOfficer StartVehiclePatrol(VehiclePatrolRoute route)
		{
			PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(route.Waypoints[route.StartWaypointIndex].position);
			if (closestPoliceStation.OfficerPool.Count == 0)
			{
				Console.LogWarning(closestPoliceStation.name + " has no officers in its pool!");
				return null;
			}
			LandVehicle landVehicle = closestPoliceStation.CreateVehicle();
			PoliceOfficer policeOfficer = closestPoliceStation.PullOfficer();
			policeOfficer.AssignedVehicle = landVehicle;
			policeOfficer.EnterVehicle(null, landVehicle);
			policeOfficer.StartVehiclePatrol(route, landVehicle);
			return policeOfficer;
		}
	}
}
