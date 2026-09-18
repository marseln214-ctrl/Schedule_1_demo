using System;
using FishNet;
using ScheduleOne.Lighting;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using ScheduleOne.Vision;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class VehiclePursuitBehaviour : Behaviour
	{
		public const float MAX_CONSECUTIVE_PATHING_FAILURES = 5f;

		public const float EXTRA_VISIBILITY_TIME = 6f;

		public const float EXIT_VEHICLE_MAX_SPEED = 4f;

		public const float CLOSE_ENOUGH_THRESHOLD = 10f;

		public const float UPDATE_FREQUENCY = 0.2f;

		public const float STATIONARY_THRESHOLD = 1f;

		public const float TIME_STATIONARY_TO_EXIT = 3f;

		[Header("Settings")]
		public AnimationCurve RepathDistanceThresholdMap;

		public LandVehicle vehicle;

		private bool initialContactMade;

		private bool aggressiveDrivingEnabled;

		private bool isTargetVisible;

		private bool isTargetStrictlyVisible;

		private float playerSightedDuration;

		private float timeSinceLastSighting = 10000f;

		private int consecutivePathingFailures;

		private float timeStationary;

		private Vector3 currentDriveTarget = Vector3.zero;

		private int targetChanges;

		private float timeSincePursuitStart;

		private bool beginAsSighted;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Player TargetPlayer { get; protected set; }

		private bool isDriving => vehicle.OccupantNPCs[0] == base.Npc;

		private VehicleAgent Agent => vehicle.Agent;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void OnDestroy()
		{
			PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Remove(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(ProcessThirdPartyVisionEvent));
		}

		public void BeginAsSighted()
		{
			beginAsSighted = true;
		}

		protected override void Begin()
		{
			base.Begin();
			base.Npc.awareness.VisionCone.RangeMultiplier = 1.5f;
			if (beginAsSighted)
			{
				isTargetVisible = true;
				initialContactMade = true;
				isTargetStrictlyVisible = true;
				SetAggressiveDriving(initialContactMade);
				DriveTo(GetPlayerChasePoint());
			}
			StartPursuit();
		}

		protected override void Resume()
		{
			base.Resume();
			StartPursuit();
		}

		protected override void Pause()
		{
			base.Pause();
			initialContactMade = false;
			if (InstanceFinder.IsServer)
			{
				base.Npc.ExitVehicle();
				Agent.StopNavigating();
			}
			base.Npc.awareness.VisionCone.RangeMultiplier = 1f;
			base.Npc.awareness.SetAwarenessActive(active: true);
		}

		protected override void End()
		{
			base.End();
			Disable();
			initialContactMade = false;
			if (vehicle != null)
			{
				PoliceLight componentInChildren = vehicle.GetComponentInChildren<PoliceLight>();
				if (componentInChildren != null)
				{
					componentInChildren.IsOn = false;
				}
			}
			if (InstanceFinder.IsServer)
			{
				base.Npc.ExitVehicle();
				Agent.StopNavigating();
				if (TargetPlayer != null)
				{
					(base.Npc as PoliceOfficer).PursuitBehaviour.AssignTarget(null, TargetPlayer.NetworkObject);
					(base.Npc as PoliceOfficer).PursuitBehaviour.MarkPlayerVisible();
				}
			}
			base.Npc.awareness.VisionCone.RangeMultiplier = 1f;
			base.Npc.awareness.SetAwarenessActive(active: true);
		}

		public virtual void AssignTarget(Player target)
		{
			TargetPlayer = target;
		}

		private void StartPursuit()
		{
			if (vehicle == null)
			{
				Console.LogError("VehiclePursuitBehaviour: Vehicle is unassigned");
				End();
				return;
			}
			if (TargetPlayer == null)
			{
				Console.LogError("VehiclePursuitBehaviour: TargetPlayer is unassigned");
				End();
				return;
			}
			if (InstanceFinder.IsServer && base.Npc.CurrentVehicle != vehicle)
			{
				if (base.Npc.CurrentVehicle != null)
				{
					base.Npc.ExitVehicle();
				}
				base.Npc.EnterVehicle(null, vehicle);
			}
			PoliceLight componentInChildren = vehicle.GetComponentInChildren<PoliceLight>();
			if (componentInChildren != null)
			{
				componentInChildren.IsOn = true;
			}
			if (!isDriving)
			{
				Console.Log("Disabling awareness");
				base.Npc.awareness.SetAwarenessActive(active: false);
			}
			UpdateDestination();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			timeSincePursuitStart += Time.deltaTime;
			if (!IsTargetValid())
			{
				End_Networked(null);
				return;
			}
			CheckExitVehicle();
			if (isDriving)
			{
				SetAggressiveDriving(initialContactMade);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (base.Active)
			{
				CheckPlayerVisibility();
			}
		}

		private void UpdateDestination()
		{
			if (!base.Active || !InstanceFinder.IsServer || Agent.NavigationCalculationInProgress || !isDriving)
			{
				return;
			}
			if (Agent.GetIsStuck() && vehicle.speed_Kmh < 4f)
			{
				End_Networked(null);
				return;
			}
			if (vehicle.VelocityCalculator.Velocity.magnitude < 1f)
			{
				timeStationary += 0.2f;
				if (timeStationary > 3f && timeSincePursuitStart > 10f)
				{
					End_Networked(null);
					return;
				}
			}
			else
			{
				timeStationary = 0f;
			}
			if (isTargetVisible)
			{
				if (IsAsCloseAsPossible(GetPlayerChasePoint(), out var closestPosition) || IsAsCloseAsPossible(TargetPlayer.Avatar.CenterPoint, out closestPosition) || Vector3.Distance(vehicle.transform.position, closestPosition) < 10f)
				{
					vehicle.ApplyHandbrake();
					Agent.StopNavigating();
					if (vehicle.speed_Kmh < 4f)
					{
						End_Networked(null);
						return;
					}
				}
				else if (!Agent.AutoDriving || Vector3.Distance(vehicle.Agent.TargetLocation, GetPlayerChasePoint()) > 10f)
				{
					DriveTo(GetPlayerChasePoint());
				}
				float num = Vector3.Distance(currentDriveTarget, TargetPlayer.CrimeData.LastKnownPosition);
				float value = Vector3.Distance(base.transform.position, TargetPlayer.CrimeData.LastKnownPosition);
				if (num > RepathDistanceThresholdMap.Evaluate(Mathf.Clamp(value, 0f, 100f)))
				{
					DriveTo(GetPlayerChasePoint());
				}
				return;
			}
			if (!Agent.AutoDriving)
			{
				if (IsAsCloseAsPossible(TargetPlayer.CrimeData.LastKnownPosition, out var closestPosition2) || Vector3.Distance(closestPosition2, vehicle.transform.position) < 10f)
				{
					if (vehicle.speed_Kmh < 4f)
					{
						End_Networked(null);
						return;
					}
				}
				else
				{
					DriveTo(TargetPlayer.CrimeData.LastKnownPosition);
				}
			}
			float num2 = Vector3.Distance(currentDriveTarget, TargetPlayer.CrimeData.LastKnownPosition);
			float value2 = Vector3.Distance(base.transform.position, TargetPlayer.CrimeData.LastKnownPosition);
			if (num2 > RepathDistanceThresholdMap.Evaluate(Mathf.Clamp(value2, 0f, 100f)))
			{
				DriveTo(TargetPlayer.CrimeData.LastKnownPosition);
			}
		}

		private bool IsTargetValid()
		{
			if (TargetPlayer == null)
			{
				return false;
			}
			if (TargetPlayer.IsArrested)
			{
				return false;
			}
			if (TargetPlayer.IsUnconscious)
			{
				return false;
			}
			if (TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				return false;
			}
			return true;
		}

		private void CheckExitVehicle()
		{
			if (InstanceFinder.IsServer && !isDriving && vehicle.OccupantNPCs[0] == null)
			{
				End_Networked(null);
			}
		}

		private Vector3 GetPlayerChasePoint()
		{
			Mathf.Min(5f, Vector3.Distance(TargetPlayer.Avatar.CenterPoint, base.transform.position));
			Mathf.Clamp01(TargetPlayer.VelocityCalculator.Velocity.magnitude / 8f);
			return TargetPlayer.Avatar.CenterPoint;
		}

		private void SetAggressiveDriving(bool aggressive)
		{
			bool flag = aggressiveDrivingEnabled;
			aggressiveDrivingEnabled = aggressive;
			if (aggressive)
			{
				vehicle.Agent.Flags.OverriddenSpeed = 80f;
				vehicle.Agent.Flags.OverriddenReverseSpeed = 20f;
				vehicle.Agent.Flags.OverrideSpeed = true;
				vehicle.Agent.Flags.AutoBrakeAtDestination = false;
				vehicle.Agent.Flags.IgnoreTrafficLights = true;
				vehicle.Agent.Flags.UseRoads = false;
				vehicle.Agent.Flags.ObstacleMode = DriveFlags.EObstacleMode.IgnoreOnlySquishy;
			}
			else
			{
				vehicle.Agent.Flags.OverrideSpeed = false;
				vehicle.Agent.Flags.SpeedLimitMultiplier = 1.5f;
				vehicle.Agent.Flags.AutoBrakeAtDestination = true;
				vehicle.Agent.Flags.IgnoreTrafficLights = true;
				vehicle.Agent.Flags.UseRoads = true;
				vehicle.Agent.Flags.ObstacleMode = DriveFlags.EObstacleMode.Default;
			}
			if (aggressive != flag && vehicle.Agent.AutoDriving)
			{
				vehicle.Agent.RecalculateNavigation();
			}
		}

		private void DriveTo(Vector3 location)
		{
			if (!Agent.IsOnVehicleGraph())
			{
				End();
				return;
			}
			targetChanges++;
			currentDriveTarget = location;
			Agent.Navigate(location);
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

		private bool IsPlayerVisible()
		{
			return base.Npc.awareness.VisionCone.IsPlayerVisible(TargetPlayer);
		}

		private void CheckPlayerVisibility()
		{
			if (TargetPlayer == null)
			{
				return;
			}
			if (isTargetVisible)
			{
				playerSightedDuration += Time.fixedDeltaTime;
				if (IsPlayerVisible())
				{
					initialContactMade = true;
					TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: true);
					timeSinceLastSighting = 0f;
				}
				else
				{
					TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
				}
			}
			if (!IsPlayerVisible())
			{
				playerSightedDuration = 0f;
				timeSinceLastSighting += Time.fixedDeltaTime;
				isTargetVisible = false;
				isTargetStrictlyVisible = false;
				if (timeSinceLastSighting < 6f)
				{
					TargetPlayer.CrimeData.RecordLastKnownPosition(resetTimeSinceSighted: false);
					isTargetVisible = true;
				}
			}
			else
			{
				isTargetStrictlyVisible = true;
			}
		}

		private void ProcessVisionEvent(VisionEventReceipt visionEventReceipt)
		{
			if (base.Active && visionEventReceipt.TargetPlayer == TargetPlayer.NetworkObject && visionEventReceipt.State == PlayerVisualState.EVisualState.SearchedFor)
			{
				isTargetVisible = true;
				initialContactMade = true;
				isTargetStrictlyVisible = true;
				DriveTo(GetPlayerChasePoint());
				if (TargetPlayer.IsOwner && TargetPlayer.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.Investigating)
				{
					TargetPlayer.CrimeData.Escalate();
				}
			}
		}

		private void ProcessThirdPartyVisionEvent(VisionEventReceipt visionEventReceipt)
		{
			if (base.Active && visionEventReceipt.TargetPlayer == TargetPlayer.NetworkObject && visionEventReceipt.State == PlayerVisualState.EVisualState.SearchedFor)
			{
				isTargetVisible = true;
				isTargetStrictlyVisible = true;
				DriveTo(GetPlayerChasePoint());
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EVehiclePursuitBehaviour_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			if (InstanceFinder.IsOffline || InstanceFinder.IsServer)
			{
				VisionCone visionCone = base.Npc.awareness.VisionCone;
				visionCone.onVisionEventFull = (VisionCone.EventStateChange)Delegate.Combine(visionCone.onVisionEventFull, new VisionCone.EventStateChange(ProcessVisionEvent));
				InvokeRepeating("UpdateDestination", 0.5f, 0.2f);
			}
			PoliceOfficer.OnPoliceVisionEvent = (Action<VisionEventReceipt>)Delegate.Combine(PoliceOfficer.OnPoliceVisionEvent, new Action<VisionEventReceipt>(ProcessThirdPartyVisionEvent));
		}
	}
}
