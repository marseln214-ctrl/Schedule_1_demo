using FishNet;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class VehiclePatrolBehaviour : Behaviour
	{
		public const float MAX_CONSECUTIVE_PATHING_FAILURES = 5f;

		public const float PROGRESSION_THRESHOLD = 10f;

		public int CurrentWaypoint;

		[Header("Settings")]
		public VehiclePatrolRoute Route;

		public LandVehicle Vehicle;

		private bool aggressiveDrivingEnabled = true;

		private int consecutivePathingFailures;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool isDriving => Vehicle.OccupantNPCs[0] == base.Npc;

		private VehicleAgent Agent => Vehicle.Agent;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Begin()
		{
			base.Begin();
			StartPatrol();
		}

		protected override void Resume()
		{
			base.Resume();
			StartPatrol();
		}

		protected override void Pause()
		{
			base.Pause();
			if (InstanceFinder.IsServer)
			{
				base.Npc.ExitVehicle();
				Agent.StopNavigating();
			}
			base.Npc.awareness.VisionCone.RangeMultiplier = 1f;
			(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
			base.Npc.awareness.SetAwarenessActive(active: true);
		}

		protected override void End()
		{
			base.End();
			if (InstanceFinder.IsServer)
			{
				base.Npc.ExitVehicle();
				Agent.StopNavigating();
			}
			base.Npc.awareness.VisionCone.RangeMultiplier = 1f;
			(base.Npc as PoliceOfficer).BodySearchChance = 0.1f;
			base.Npc.awareness.SetAwarenessActive(active: true);
		}

		public void SetRoute(VehiclePatrolRoute route)
		{
			Route = route;
		}

		private void StartPatrol()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (Vehicle == null)
			{
				Console.LogError("VehiclePursuitBehaviour: Vehicle is unassigned");
				Disable_Networked(null);
				End_Networked(null);
			}
			else if (InstanceFinder.IsServer && base.Npc.CurrentVehicle != Vehicle)
			{
				if (base.Npc.CurrentVehicle != null)
				{
					base.Npc.ExitVehicle();
				}
				base.Npc.EnterVehicle(null, Vehicle);
			}
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer || !isDriving)
			{
				return;
			}
			if (Agent.AutoDriving)
			{
				if (!Agent.NavigationCalculationInProgress && Vector3.Distance(Vehicle.transform.position, Route.Waypoints[CurrentWaypoint].position) < 10f)
				{
					CurrentWaypoint++;
					if (CurrentWaypoint >= Route.Waypoints.Length)
					{
						Disable_Networked(null);
					}
					else
					{
						DriveTo(Route.Waypoints[CurrentWaypoint].position);
					}
				}
			}
			else if (CurrentWaypoint >= Route.Waypoints.Length)
			{
				Disable_Networked(null);
			}
			else
			{
				DriveTo(Route.Waypoints[CurrentWaypoint].position);
			}
		}

		private void DriveTo(Vector3 location)
		{
			if (!Agent.IsOnVehicleGraph())
			{
				End();
			}
			else
			{
				Agent.Navigate(location, null, NavigationCallback);
			}
		}

		private void NavigationCallback(VehicleAgent.ENavigationResult status)
		{
			if (status == VehicleAgent.ENavigationResult.Failed)
			{
				consecutivePathingFailures++;
			}
			else
			{
				consecutivePathingFailures = 0;
			}
			if ((float)consecutivePathingFailures > 5f && InstanceFinder.IsServer)
			{
				End_Networked(null);
			}
		}

		private bool IsAsCloseAsPossible(Vector3 pos, out Vector3 closestPosition)
		{
			closestPosition = NavigationUtility.SampleVehicleGraph(pos);
			return Vector3.Distance(closestPosition, base.transform.position) < 10f;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePatrolBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
		}
	}
}
