using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using Pathfinding;
using ScheduleOne.DevUtilities;
using ScheduleOne.Math;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	[RequireComponent(typeof(LandVehicle))]
	public class VehicleAgent : MonoBehaviour
	{
		public enum ENavigationResult
		{
			Failed = 0,
			Complete = 1,
			Stopped = 2
		}

		public enum EAgentStatus
		{
			Inactive = 0,
			MovingToRoad = 1,
			OnRoad = 2
		}

		public enum EPathGroupStatus
		{
			Inactive = 0,
			Calculating = 1
		}

		public enum ESweepType
		{
			FL = 0,
			FR = 1,
			RL = 2,
			RR = 3
		}

		public delegate void NavigationCallback(ENavigationResult status);

		public const string VehicleGraphName = "General Vehicle Graph";

		public const string RoadGraphName = "Road Nodes";

		public const float MaxDistanceFromPath = 6f;

		public const float MaxDistanceFromPathWhenReversing = 8f;

		public static Vector3 MainGraphSamplePoint = new Vector3(31.5f, 0f, 51f);

		public static float MinRenavigationRate = 2f;

		public const float Steer_P = 40f;

		public const float Steer_I = 5f;

		public const float Steer_D = 10f;

		public const float Throttle_P = 0.08f;

		public const float Throttle_I = 0f;

		public const float Throttle_D = 0f;

		public const float Steer_Rate = 135f;

		public const float MaxAxlePositionShift = 3f;

		public const float OBSTACLE_MIN_RANGE = 1.5f;

		public const float OBSTACLE_MAX_RANGE = 15f;

		public const float MAX_STEER_ANGLE_OVERRIDE = 35f;

		public const float KINEMATIC_MODE_MIN_DISTANCE = 40f;

		public const float INFREQUENT_UPDATE_RATE = 0.033f;

		public bool DEBUG_MODE;

		public DriveFlags Flags;

		[Header("Seekers")]
		[SerializeField]
		protected Seeker roadSeeker;

		[SerializeField]
		protected Seeker generalSeeker;

		[Header("References")]
		[SerializeField]
		protected Transform CTE_Origin;

		[SerializeField]
		protected Transform FrontAxlePosition;

		[SerializeField]
		protected Transform RearAxlePosition;

		[Header("Sensors")]
		[SerializeField]
		protected Sensor sensor_FL;

		[SerializeField]
		protected Sensor sensor_FM;

		[SerializeField]
		protected Sensor sensor_FR;

		[SerializeField]
		protected Sensor sensor_RR;

		[SerializeField]
		protected Sensor sensor_RL;

		[Header("Sweeping")]
		[SerializeField]
		protected LayerMask sweepMask;

		[SerializeField]
		protected Transform sweepOrigin_FL;

		[SerializeField]
		protected Transform sweepOrigin_FR;

		[SerializeField]
		protected Transform sweepOrigin_RL;

		[SerializeField]
		protected Transform sweepOrigin_RR;

		[SerializeField]
		protected Wheel leftWheel;

		[SerializeField]
		protected Wheel rightWheel;

		protected const float sweepSegment = 15f;

		[Header("Path following")]
		[SerializeField]
		[Range(0.1f, 5f)]
		protected float sampleStepSizeMin = 2.5f;

		[SerializeField]
		[Range(0.1f, 5f)]
		protected float sampleStepSizeMax = 5f;

		protected int aheadPointSamples = 4;

		protected const float DestinationDistanceSlowThreshold = 8f;

		protected const float DestinationArrivalThreshold = 3f;

		[Header("Steer settings")]
		[SerializeField]
		protected float steerTargetFollowRate = 2f;

		private SteerPID steerPID;

		[Header("Turning speed reduction")]
		protected float turnSpeedReductionMinRange = 2f;

		protected float turnSpeedReductionMaxRange = 10f;

		protected float turnSpeedReductionDivisor = 90f;

		private float minTurnSpeedReductionAngleThreshold = 15f;

		private float minTurningSpeed = 10f;

		[Header("Throttle")]
		[SerializeField]
		protected float throttleMin = -1f;

		[SerializeField]
		protected float throttleMax = 1f;

		private PID throttlePID;

		public static float UnmarkedSpeed = 25f;

		public static float ReverseSpeed = 5f;

		private ValueTracker speedReductionTracker;

		[Header("Pursuit Mode")]
		public bool PursuitModeEnabled;

		public Transform PursuitTarget;

		public float PursuitDistanceUpdateThreshold = 5f;

		private Vector3 PursuitTargetLastPosition = Vector3.zero;

		[Header("Stuck Detection")]
		public VehicleTeleporter Teleporter;

		public PositionHistoryTracker PositionHistoryTracker;

		public float StuckTimeThreshold = 10f;

		public int StuckSamples = 4;

		public float StuckDistanceThreshold = 1f;

		protected NavigationCallback storedNavigationCallback;

		protected SpeedZone currentSpeedZone;

		protected LandVehicle vehicle;

		protected float wheelbase;

		protected float wheeltrack;

		protected float vehicleLength;

		protected float vehicleWidth;

		protected float turnRadius;

		protected float sweepTrack;

		private float wheelBottomOffset;

		[Header("Control info - READONLY")]
		[SerializeField]
		protected float targetSpeed;

		[SerializeField]
		protected float targetSteerAngle_Normalized;

		protected float lateralOffset;

		protected PathSmoothingUtility.SmoothedPath path;

		private float timeSinceLastNavigationCall;

		private float sweepTestFailedTime;

		private NavigationSettings currentNavigationSettings;

		private Coroutine navigationCalculationRoutine;

		private Coroutine reverseCoroutine;

		public bool KinematicMode { get; protected set; }

		public bool AutoDriving { get; protected set; }

		public bool IsReversing => reverseCoroutine != null;

		public Vector3 TargetLocation { get; protected set; } = Vector3.zero;


		protected float sampleStepSize => Mathf.Lerp(sampleStepSizeMin, sampleStepSizeMax, Mathf.Clamp01(vehicle.speed_Kmh / vehicle.TopSpeed));

		protected float turnSpeedReductionRange => Mathf.Lerp(turnSpeedReductionMinRange, turnSpeedReductionMaxRange, Mathf.Clamp(vehicle.speed_Kmh / vehicle.TopSpeed, 0f, 1f));

		protected float maxSteerAngle => vehicle.ActualMaxSteeringAngle;

		private Vector3 FrontOfVehiclePosition => base.transform.position + base.transform.forward * vehicleLength / 2f;

		public bool NavigationCalculationInProgress => navigationCalculationRoutine != null;

		private void Awake()
		{
			vehicle = GetComponent<LandVehicle>();
			throttlePID = new PID(0.08f, 0f, 0f);
			steerPID = new SteerPID();
			speedReductionTracker = new ValueTracker(10f);
			PositionHistoryTracker.historyDuration = StuckTimeThreshold;
		}

		protected virtual void Start()
		{
			InvokeRepeating("RefreshSpeedZone", 0f, 0.25f);
			InvokeRepeating("UpdateStuckDetection", 1f, 1f);
			InvokeRepeating("InfrequentUpdate", 0f, 0.033f);
			InitializeVehicleData();
		}

		private void InitializeVehicleData()
		{
			vehicleLength = vehicle.boundingBox.transform.localScale.z;
			vehicleWidth = vehicle.boundingBox.transform.localScale.x;
			Transform transform = null;
			Transform transform2 = null;
			Transform transform3 = null;
			Transform transform4 = null;
			foreach (Wheel wheel in vehicle.wheels)
			{
				if (transform == null || vehicle.transform.InverseTransformPoint(wheel.transform.position).z > vehicle.transform.InverseTransformPoint(transform.position).z)
				{
					transform = wheel.transform;
				}
				if (transform2 == null || vehicle.transform.InverseTransformPoint(wheel.transform.position).z < vehicle.transform.InverseTransformPoint(transform2.position).z)
				{
					transform2 = wheel.transform;
				}
				if (transform4 == null || vehicle.transform.InverseTransformPoint(wheel.transform.position).x > vehicle.transform.InverseTransformPoint(transform4.position).x)
				{
					transform4 = wheel.transform;
				}
				if (transform3 == null || vehicle.transform.InverseTransformPoint(wheel.transform.position).x < vehicle.transform.InverseTransformPoint(transform3.position).x)
				{
					transform3 = wheel.transform;
				}
			}
			wheelbase = vehicle.transform.InverseTransformPoint(transform.position).z - vehicle.transform.InverseTransformPoint(transform2.position).z;
			wheeltrack = vehicle.transform.InverseTransformPoint(transform4.position).x - vehicle.transform.InverseTransformPoint(transform3.position).x;
			sweepTrack = sweepOrigin_FR.localPosition.x - sweepOrigin_FL.localPosition.x;
			wheelBottomOffset = 0f - base.transform.InverseTransformPoint(leftWheel.transform.position).y + leftWheel.wheelCollider.radius;
			turnRadius = wheelbase / Mathf.Sin(maxSteerAngle * (MathF.PI / 180f)) + 1.35f;
		}

		protected virtual void FixedUpdate()
		{
			if (Time.timeScale == 0f)
			{
				return;
			}
			timeSinceLastNavigationCall += Time.deltaTime;
			if (!AutoDriving)
			{
				return;
			}
			Player.GetClosestPlayer(base.transform.position, out var distance);
			if (KinematicMode)
			{
				if (distance < 40f * QualitySettings.lodBias)
				{
					KinematicMode = false;
					vehicle.Rb.isKinematic = KinematicMode || !InstanceFinder.IsHost;
					for (int i = 0; i < vehicle.wheels.Count; i++)
					{
						vehicle.wheels[i].wheelCollider.enabled = !vehicle.Rb.isKinematic;
					}
					if (InstanceFinder.IsHost)
					{
						vehicle.Rb.velocity = base.transform.forward * targetSpeed / 3.6f * 0.5f;
					}
				}
			}
			else if (distance > 65f * QualitySettings.lodBias)
			{
				KinematicMode = true;
			}
			vehicle.Rb.isKinematic = KinematicMode || !InstanceFinder.IsHost;
			for (int j = 0; j < vehicle.wheels.Count; j++)
			{
				vehicle.wheels[j].wheelCollider.enabled = !vehicle.Rb.isKinematic;
			}
		}

		protected void InfrequentUpdate()
		{
			if (Time.timeScale == 0f)
			{
				return;
			}
			UpdatePursuitMode();
			if (AutoDriving)
			{
				CheckDistanceFromPath();
				UpdateOvertaking();
				if (reverseCoroutine == null)
				{
					UpdateSpeed();
					UpdateSteering();
					UpdateSweep();
					UpdateSpeedReduction();
				}
				if (KinematicMode)
				{
					UpdateKinematic(0.033f);
				}
			}
		}

		protected void LateUpdate()
		{
			if (AutoDriving && Time.timeScale != 0f)
			{
				if (DEBUG_MODE)
				{
					Debug.Log("Target speed: " + targetSpeed);
				}
				throttlePID.pFactor = 0.08f;
				throttlePID.iFactor = 0f;
				throttlePID.dFactor = 0f;
				float num = throttlePID.Update(targetSpeed, vehicle.speed_Kmh, Time.deltaTime);
				float num2 = 0.01f;
				if (Mathf.Abs(num) < num2)
				{
					num = 0f;
				}
				vehicle.throttleOverride = Mathf.Clamp(num, throttleMin, throttleMax);
				vehicle.steerOverride = Mathf.Lerp(vehicle.steerOverride, targetSteerAngle_Normalized, Time.deltaTime * steerTargetFollowRate);
			}
		}

		protected void UpdateKinematic(float deltaTime)
		{
			if (!AutoDriving || path == null)
			{
				return;
			}
			float distance = targetSpeed * 0.2f * deltaTime;
			Vector3 referencePoint = vehicle.boundingBox.transform.position - vehicle.boundingBox.transform.up * vehicle.boundingBoxDimensions.y * 0.5f;
			Vector3 aheadPoint = PathUtility.GetAheadPoint(path, referencePoint, distance);
			if (aheadPoint == Vector3.zero)
			{
				return;
			}
			base.transform.position = aheadPoint;
			int startPointIndex;
			int endPointIndex;
			float pointLerp;
			Vector3 vector = PathUtility.GetClosestPointOnPath(path, base.transform.position, out startPointIndex, out endPointIndex, out pointLerp);
			Vector3 origin = vector + Vector3.up * 2f;
			LayerMask layerMask = LayerMask.GetMask("Default");
			layerMask = (int)layerMask | LayerMask.GetMask("Terrain");
			RaycastHit[] source = Physics.RaycastAll(origin, Vector3.down, 3f, layerMask, QueryTriggerInteraction.Ignore);
			source = source.OrderBy((RaycastHit h) => h.distance).ToArray();
			bool flag = false;
			RaycastHit raycastHit = default(RaycastHit);
			for (int i = 0; i < source.Length; i++)
			{
				if (!source[i].collider.transform.IsChildOf(base.transform))
				{
					raycastHit = source[i];
					flag = true;
					break;
				}
			}
			if (flag)
			{
				vector = raycastHit.point;
			}
			base.transform.position = vector + base.transform.up * wheelBottomOffset;
			Vector3 zero = Vector3.zero;
			int num = 3;
			for (int j = 0; j < num; j++)
			{
				zero += PathUtility.GetAheadPoint(path, base.transform.position, vehicleLength / 2f + 1f * (float)(j + 1), startPointIndex, endPointIndex);
			}
			zero /= (float)num;
			Vector3 normalized = (zero - vector).normalized;
			base.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			Vector3 axleGroundHit = GetAxleGroundHit(front: true);
			Vector3 axleGroundHit2 = GetAxleGroundHit(front: false);
			normalized = (axleGroundHit - axleGroundHit2).normalized;
			base.transform.forward = normalized;
		}

		private Vector3 GetAxleGroundHit(bool front)
		{
			Vector3 origin = FrontAxlePosition.position + Vector3.up * 1f;
			if (!front)
			{
				origin = RearAxlePosition.position + Vector3.up * 1f;
			}
			LayerMask layerMask = LayerMask.GetMask("Default");
			layerMask = (int)layerMask | LayerMask.GetMask("Terrain");
			RaycastHit[] source = Physics.RaycastAll(origin, Vector3.down, 2f, layerMask, QueryTriggerInteraction.Ignore);
			source = source.OrderBy((RaycastHit h) => h.distance).ToArray();
			for (int i = 0; i < source.Length; i++)
			{
				if (!source[i].collider.transform.IsChildOf(base.transform))
				{
					return source[i].point;
				}
			}
			if (front)
			{
				return FrontAxlePosition.position - base.transform.up * wheelBottomOffset;
			}
			return RearAxlePosition.position - base.transform.up * wheelBottomOffset;
		}

		private void UpdateSweep()
		{
			if (KinematicMode)
			{
				return;
			}
			if (Mathf.Abs(vehicle.speed_Kmh) > 5f)
			{
				sweepTestFailedTime = 0f;
			}
			else if (Mathf.Abs(targetSteerAngle_Normalized) * maxSteerAngle > 5f)
			{
				float num = 1.5f;
				float hitDistance;
				Vector3 hitPoint;
				bool num2 = SweepTurn(ESweepType.FR, Mathf.Sign(targetSteerAngle_Normalized) * 30f, reverse: false, out hitDistance, out hitPoint, targetSteerAngle_Normalized * maxSteerAngle);
				float hitDistance2;
				Vector3 hitPoint2;
				bool flag = SweepTurn(ESweepType.FL, Mathf.Sign(targetSteerAngle_Normalized) * 30f, reverse: false, out hitDistance2, out hitPoint2, targetSteerAngle_Normalized * maxSteerAngle);
				if ((num2 && hitDistance < num) || (flag && hitDistance2 < num))
				{
					sweepTestFailedTime += Time.deltaTime;
					if ((double)sweepTestFailedTime > 0.25)
					{
						StartReverse();
						sweepTestFailedTime = 0f;
					}
				}
				else
				{
					sweepTestFailedTime = 0f;
				}
			}
			else
			{
				sweepTestFailedTime = 0f;
			}
		}

		private void UpdateSpeedReduction()
		{
			if (path == null)
			{
				return;
			}
			if (path != null && Vector3.Distance(base.transform.position, path.vectorPath[path.vectorPath.Count - 1]) < 3f)
			{
				path = null;
				vehicle.overrideControls = false;
				vehicle.steerOverride = 0f;
				vehicle.throttleOverride = 0f;
				AutoDriving = false;
				vehicle.ResetMaxSteerAngle();
				if (storedNavigationCallback != null)
				{
					storedNavigationCallback(ENavigationResult.Complete);
					storedNavigationCallback = null;
				}
			}
			else
			{
				if (KinematicMode)
				{
					return;
				}
				PathUtility.GetClosestPointOnPath(path, base.transform.position, out var startPointIndex, out var _, out var pointLerp);
				float num = 1f;
				float num2 = 1f;
				float num3 = targetSpeed;
				if (Flags.TurnBasedSpeedReduction)
				{
					float num4 = Mathf.Max(PathUtility.CalculateAngleChangeOverPath(path, startPointIndex, pointLerp, turnSpeedReductionRange), targetSteerAngle_Normalized * maxSteerAngle);
					if (num4 > minTurnSpeedReductionAngleThreshold)
					{
						num3 = Mathf.Lerp(num3, minTurningSpeed, Mathf.Clamp(num4 / turnSpeedReductionDivisor, 0f, 1f));
					}
				}
				if (Flags.ObstacleMode != DriveFlags.EObstacleMode.IgnoreAll)
				{
					BetterSweepTurn(ESweepType.FL, vehicle.SyncAccessor_currentSteerAngle, reverse: false, sensor_FM.checkMask, out var hitDistance, out var _);
					BetterSweepTurn(ESweepType.FR, vehicle.SyncAccessor_currentSteerAngle, reverse: false, sensor_FM.checkMask, out var hitDistance2, out var _);
					float num5 = Mathf.Min(hitDistance, hitDistance2);
					float num6 = Mathf.Lerp(1.5f, 15f, Mathf.Clamp01(vehicle.speed_Kmh / vehicle.TopSpeed));
					if (num5 < num6)
					{
						if (DEBUG_MODE)
						{
							Console.Log("Obstacle detected at " + num5 + "m:");
						}
						num = Mathf.Clamp((num5 - 1.5f) / (num6 - 1.5f), 0.002f, 1f);
					}
				}
				if (Flags.AutoBrakeAtDestination && path != null)
				{
					float num7 = Vector3.Distance(base.transform.position, path.vectorPath[path.vectorPath.Count - 1]);
					if (num7 < 8f)
					{
						num2 = Mathf.Clamp(num7 / 8f, 0f, 1f);
						if (num7 < 3f)
						{
							num2 = 0f;
						}
						if (num2 < 0.2f)
						{
							vehicle.ApplyHandbrake();
						}
					}
				}
				if (DEBUG_MODE)
				{
					Debug.Log("Obstacle speed multiplier: " + num);
					Debug.Log("Destination speed multiplier: " + num2);
					Debug.Log("Turn target speed: " + num3);
				}
				float num8 = num * num2;
				speedReductionTracker.SubmitValue(num8);
				targetSpeed *= num8;
				targetSpeed = Mathf.Min(targetSpeed, num3);
			}
		}

		private void UpdatePursuitMode()
		{
			if (PursuitModeEnabled && !(PursuitTarget == null) && Vector3.Distance(PursuitTarget.position, PursuitTargetLastPosition) > PursuitDistanceUpdateThreshold)
			{
				PursuitTargetLastPosition = PursuitTarget.position;
				Navigate(PursuitTarget.position);
			}
		}

		private void UpdateStuckDetection()
		{
			if (!AutoDriving)
			{
				PositionHistoryTracker.ClearHistory();
			}
			else
			{
				if (!Flags.StuckDetection || speedReductionTracker.RecordedHistoryLength() < StuckTimeThreshold || speedReductionTracker.GetLowestValue() < 0.1f || !(PositionHistoryTracker.RecordedTime >= StuckTimeThreshold))
				{
					return;
				}
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < StuckSamples; i++)
				{
					zero += PositionHistoryTracker.GetPositionXSecondsAgo(StuckTimeThreshold / (float)StuckSamples * (float)(i + 1));
				}
				zero /= (float)StuckSamples;
				if (Vector3.Distance(base.transform.position, zero) < StuckDistanceThreshold)
				{
					if (DEBUG_MODE)
					{
						Console.LogWarning("Vehicle stuck");
					}
					if (IsOnVehicleGraph())
					{
						Teleporter.MoveToRoadNetwork();
					}
					else
					{
						Teleporter.MoveToGraph();
					}
					PositionHistoryTracker.ClearHistory();
				}
			}
		}

		private void CheckDistanceFromPath()
		{
			if (timeSinceLastNavigationCall < MinRenavigationRate || KinematicMode || path == null)
			{
				return;
			}
			int startPointIndex;
			int endPointIndex;
			float pointLerp;
			Vector3 closestPointOnPath = PathUtility.GetClosestPointOnPath(path, base.transform.position, out startPointIndex, out endPointIndex, out pointLerp);
			closestPointOnPath += GetPathLateralDirection() * lateralOffset;
			if (Vector3.Distance(base.transform.position, closestPointOnPath) > (IsReversing ? 8f : 6f))
			{
				if (DEBUG_MODE)
				{
					Console.Log("Too far from path! Re-navigating.");
				}
				Navigate(TargetLocation, currentNavigationSettings, storedNavigationCallback);
			}
		}

		private void UpdateOvertaking()
		{
			lateralOffset = 0f;
			if (sensor_FM.obstruction != null && sensor_FM.obstruction.GetComponentInParent<LandVehicle>() != null && sensor_FM.obstructionDistance < 8f)
			{
				_ = sensor_FM.obstructionDistance / 8f;
			}
		}

		protected virtual void RefreshSpeedZone()
		{
			List<SpeedZone> speedZones = SpeedZone.GetSpeedZones(base.transform.position);
			if (speedZones.Count > 0)
			{
				currentSpeedZone = speedZones[0];
			}
			else
			{
				currentSpeedZone = null;
			}
		}

		protected virtual void UpdateSpeed()
		{
			if (path == null)
			{
				targetSpeed = 0f;
				return;
			}
			if (currentSpeedZone != null)
			{
				targetSpeed = currentSpeedZone.speed * Flags.SpeedLimitMultiplier;
			}
			else
			{
				targetSpeed = UnmarkedSpeed * Flags.SpeedLimitMultiplier;
			}
			if (Flags.OverrideSpeed)
			{
				targetSpeed = Flags.OverriddenSpeed;
			}
		}

		protected void UpdateSteering()
		{
			if (path == null || path.vectorPath.Count < 2 || KinematicMode)
			{
				targetSteerAngle_Normalized = 0f;
				return;
			}
			int startPointIndex;
			int endPointIndex;
			float pointLerp;
			Vector3 closestPointOnPath = PathUtility.GetClosestPointOnPath(path, base.transform.position, out startPointIndex, out endPointIndex, out pointLerp);
			Vector3 aheadPoint = PathUtility.GetAheadPoint(path, base.transform.position, vehicleLength / 2f + sampleStepSize);
			aheadPoint = closestPointOnPath;
			Vector3 averageAheadPoint = PathUtility.GetAverageAheadPoint(path, base.transform.position, aheadPointSamples, sampleStepSize);
			averageAheadPoint = PathUtility.GetAheadPoint(path, base.transform.position, 0.5f);
			Vector3 normalized = (averageAheadPoint - aheadPoint).normalized;
			Debug.DrawLine(base.transform.position, aheadPoint, Color.yellow, 0.5f);
			Debug.DrawLine(base.transform.position, averageAheadPoint, Color.magenta, 0.5f);
			float error = PathUtility.CalculateCTE(CTE_Origin.position + base.transform.forward * Mathf.Clamp01(vehicle.speed_Kmh / vehicle.TopSpeed) * (vehicle.TopSpeed * 0.2778f * 0.3f), base.transform, aheadPoint, averageAheadPoint, path);
			float num = Mathf.Clamp(steerPID.GetNewValue(error, new PID_Parameters(40f, 5f, 10f)) / maxSteerAngle, -1f, 1f);
			float num2 = Vector3.SignedAngle(base.transform.forward, normalized, Vector3.up);
			float num3 = 45f;
			if (Mathf.Abs(num2) > 45f)
			{
				num += Mathf.Clamp01(Mathf.Abs(num2 - num3) / (180f - num3)) * Mathf.Sign(num2);
			}
			targetSteerAngle_Normalized = Mathf.Clamp(num, -1f, 1f);
		}

		public void Navigate(Vector3 location, NavigationSettings settings = null, NavigationCallback callback = null)
		{
			if (navigationCalculationRoutine != null)
			{
				Console.LogWarning("Navigate called before previous navigation calculation was complete!");
				StopCoroutine(navigationCalculationRoutine);
			}
			if (GetIsStuck())
			{
				Console.LogWarning("Navigate called but vehilc is stuck! Navigation will still be attemped");
			}
			if (reverseCoroutine != null)
			{
				StopReversing();
			}
			if (!InstanceFinder.IsHost)
			{
				return;
			}
			path = null;
			timeSinceLastNavigationCall = 0f;
			if (settings == null)
			{
				settings = new NavigationSettings();
			}
			if (GetDistanceFromVehicleGraph() > 6f)
			{
				if (settings.ensureProximityToGraph)
				{
					Teleporter.MoveToGraph();
				}
				else if (callback != null)
				{
					callback(ENavigationResult.Failed);
					return;
				}
			}
			vehicle.Rb.isKinematic = KinematicMode;
			vehicle.Rb.interpolation = RigidbodyInterpolation.Interpolate;
			if (DEBUG_MODE)
			{
				Console.Log("Navigate called...");
			}
			TargetLocation = location;
			AutoDriving = true;
			storedNavigationCallback = callback;
			vehicle.OverrideMaxSteerAngle(35f);
			vehicle.overrideControls = true;
			currentNavigationSettings = settings;
			navigationCalculationRoutine = NavigationUtility.CalculatePath(FrontOfVehiclePosition, TargetLocation, currentNavigationSettings, Flags, generalSeeker, roadSeeker, NavigationCalculationCallback);
		}

		private void NavigationCalculationCallback(NavigationUtility.ENavigationCalculationResult result, PathSmoothingUtility.SmoothedPath _path)
		{
			navigationCalculationRoutine = null;
			if (result == NavigationUtility.ENavigationCalculationResult.Failed)
			{
				if (storedNavigationCallback != null)
				{
					storedNavigationCallback(ENavigationResult.Failed);
				}
				EndDriving();
			}
			else
			{
				path = _path;
			}
		}

		private void EndDriving()
		{
			AutoDriving = false;
			vehicle.ResetMaxSteerAngle();
			path = null;
			storedNavigationCallback = null;
			vehicle.overrideControls = false;
			currentNavigationSettings = null;
		}

		public void StopNavigating()
		{
			if (navigationCalculationRoutine != null)
			{
				StopCoroutine(navigationCalculationRoutine);
			}
			if (storedNavigationCallback != null)
			{
				storedNavigationCallback(ENavigationResult.Stopped);
			}
			EndDriving();
		}

		public void RecalculateNavigation()
		{
			if (AutoDriving)
			{
				Navigate(TargetLocation, currentNavigationSettings, storedNavigationCallback);
			}
		}

		public bool SweepTurn(ESweepType sweep, float sweepAngle, bool reverse, out float hitDistance, out Vector3 hitPoint, float steerAngle = 0f)
		{
			hitDistance = float.MaxValue;
			hitPoint = Vector3.zero;
			if (steerAngle == 0f)
			{
				steerAngle = maxSteerAngle;
			}
			steerAngle = Mathf.Abs(steerAngle);
			float num = Mathf.Sign(sweepAngle);
			FrontAxlePosition.localEulerAngles = new Vector3(0f, num * steerAngle, 0f);
			float num2 = turnRadius;
			Vector3 zero = Vector3.zero;
			Vector3 castStart = Vector3.zero;
			zero = ((!(sweepAngle > 0f)) ? (sweepOrigin_FR.position - FrontAxlePosition.right * turnRadius) : (sweepOrigin_FL.position + FrontAxlePosition.right * turnRadius));
			switch (sweep)
			{
			case ESweepType.FL:
				castStart = sweepOrigin_FL.position;
				break;
			case ESweepType.FR:
				castStart = sweepOrigin_FR.position;
				break;
			case ESweepType.RL:
				castStart = sweepOrigin_RL.position;
				break;
			case ESweepType.RR:
				castStart = sweepOrigin_RR.position;
				break;
			}
			Vector3 normalized = (castStart - zero).normalized;
			Vector3 vector = Quaternion.AngleAxis(90f * num, base.transform.up) * normalized;
			num2 = Vector3.Distance(zero, castStart);
			float num3 = 0f;
			float num4 = 0f;
			do
			{
				float num5 = num3;
				float num6 = Mathf.Clamp(num5 + Mathf.Abs(15f), 0f, Mathf.Abs(sweepAngle));
				num3 += num6 - num5;
				float num7 = num2 * Mathf.Cos(num6 * (MathF.PI / 180f));
				float num8 = num2 * Mathf.Sin(num6 * (MathF.PI / 180f));
				Vector3 vector2 = zero;
				vector2 += vector * num8 * (reverse ? (-1f) : 1f);
				vector2 += normalized * num7;
				RaycastHit[] array = Physics.SphereCastAll(castStart, 0.1f, (vector2 - castStart).normalized, Vector3.Distance(castStart, vector2), sweepMask, QueryTriggerInteraction.Ignore);
				if (array.Length != 0)
				{
					array = array.OrderBy((RaycastHit x) => Vector3.Distance(castStart, x.point)).ToArray();
				}
				RaycastHit raycastHit = default(RaycastHit);
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].collider.transform.IsChildOf(base.transform))
					{
						raycastHit = array[i];
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (raycastHit.point == Vector3.zero)
					{
						raycastHit.point = castStart;
					}
					num4 += Vector3.Distance(castStart, raycastHit.point);
					hitDistance = num4;
					hitPoint = raycastHit.point;
					return true;
				}
				num4 += Vector3.Distance(castStart, vector2);
				castStart = vector2;
			}
			while (!(num3 >= Mathf.Abs(sweepAngle)));
			return false;
		}

		public void BetterSweepTurn(ESweepType sweep, float steerAngle, bool reverse, LayerMask mask, out float hitDistance, out Vector3 hitPoint)
		{
			hitDistance = float.MaxValue;
			hitPoint = Vector3.zero;
			float num = Mathf.Sign(steerAngle);
			FrontAxlePosition.localEulerAngles = new Vector3(0f, steerAngle, 0f);
			Vector3 zero = Vector3.zero;
			Vector3 castStart = Vector3.zero;
			float num2 = Mathf.Clamp(wheelbase / Mathf.Sin(steerAngle * (MathF.PI / 180f)), -100f, 100f);
			zero = sweepOrigin_FL.position + FrontAxlePosition.right * num2;
			switch (sweep)
			{
			case ESweepType.FL:
				castStart = sweepOrigin_FL.position;
				break;
			case ESweepType.FR:
				castStart = sweepOrigin_FR.position;
				break;
			case ESweepType.RL:
				castStart = sweepOrigin_RL.position;
				break;
			case ESweepType.RR:
				castStart = sweepOrigin_RR.position;
				break;
			default:
				Console.LogWarning("Invalid sweep type: " + sweep);
				break;
			}
			Debug.DrawLine(castStart, zero, Color.white);
			Vector3 normalized = (castStart - zero).normalized;
			Vector3 vector = Quaternion.AngleAxis(90f * num, base.transform.up) * normalized;
			num2 = Vector3.Distance(zero, castStart);
			float num3 = 0f;
			int num4 = 6;
			float num5 = Mathf.Clamp(Mathf.Abs(steerAngle), 5f, 30f);
			for (float num6 = 0f; num6 < (float)num4; num6 += 1f)
			{
				float num7 = num5 * (num6 + 1f);
				float num8 = num2 * Mathf.Cos(num7 * (MathF.PI / 180f));
				float num9 = num2 * Mathf.Sin(num7 * (MathF.PI / 180f));
				Vector3 vector2 = zero;
				vector2 += vector * num9 * (reverse ? (-1f) : 1f);
				vector2 += normalized * num8;
				RaycastHit[] array = Physics.SphereCastAll(castStart, sensor_FM.checkRadius, (vector2 - castStart).normalized, Vector3.Distance(castStart, vector2), mask);
				if (array.Length != 0)
				{
					array = array.OrderBy((RaycastHit x) => Vector3.Distance(castStart, x.point)).ToArray();
				}
				RaycastHit raycastHit = default(RaycastHit);
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].collider.transform.IsChildOf(base.transform) || array[i].collider.transform.IsChildOf(vehicle.HumanoidColliderContainer.transform.transform))
					{
						continue;
					}
					if (Flags.IgnoreTrafficLights)
					{
						VehicleObstacle componentInParent = array[i].transform.GetComponentInParent<VehicleObstacle>();
						if ((object)componentInParent != null && componentInParent.type == VehicleObstacle.EObstacleType.TrafficLight)
						{
							continue;
						}
					}
					VehicleObstacle componentInParent2 = array[i].collider.transform.GetComponentInParent<VehicleObstacle>();
					if (componentInParent2 != null)
					{
						if (!componentInParent2.twoSided && Vector3.Angle(-componentInParent2.transform.forward, base.transform.forward) > 90f)
						{
							continue;
						}
					}
					else if (array[i].collider.isTrigger)
					{
						continue;
					}
					if (Flags.ObstacleMode != DriveFlags.EObstacleMode.IgnoreOnlySquishy || (!(array[i].transform.GetComponentInParent<LandVehicle>() != null) && !(array[i].transform.GetComponentInParent<Player>() != null) && !(array[i].transform.GetComponentInParent<NPC>() != null)))
					{
						raycastHit = array[i];
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (raycastHit.point == Vector3.zero)
					{
						raycastHit.point = castStart;
					}
					num3 += Vector3.Distance(castStart, raycastHit.point);
					hitDistance = num3;
					hitPoint = raycastHit.point;
					Debug.DrawLine(castStart, raycastHit.point, Color.red);
					break;
				}
				num3 += Vector3.Distance(castStart, vector2);
				Debug.DrawLine(castStart, vector2, (num6 % 2f == 0f) ? Color.green : Color.cyan);
				castStart = vector2;
			}
		}

		public void StartReverse()
		{
			if (reverseCoroutine != null)
			{
				StopReversing();
			}
			reverseCoroutine = StartCoroutine(Reverse());
		}

		public IEnumerator Reverse()
		{
			if (DEBUG_MODE)
			{
				Console.Log("Starting reverse operation");
			}
			targetSpeed = 0f;
			targetSteerAngle_Normalized = 0f;
			PathUtility.GetClosestPointOnPath(path, base.transform.position, out var startPointIndex, out var endPointIndex, out var pointLerp);
			float num = 3f;
			_ = Vector3.zero;
			_ = Vector3.zero;
			int num2 = 0;
			Vector3 futureTarget;
			float steerAngleNormal;
			while (true)
			{
				Vector3 zero = Vector3.zero;
				Vector3 vector = Vector3.zero;
				for (int i = 1; i <= aheadPointSamples; i++)
				{
					zero += PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, num + (float)i * sampleStepSizeMin);
					if (i == aheadPointSamples)
					{
						vector = PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, num + (float)i * sampleStepSizeMin + 1f);
					}
				}
				zero /= (float)aheadPointSamples;
				if (Mathf.Abs(base.transform.InverseTransformPoint(zero).x) > 1f)
				{
					futureTarget = zero;
					_ = vector - futureTarget;
					steerAngleNormal = 0f - Mathf.Sign(base.transform.InverseTransformPoint(futureTarget).x);
					yield return new WaitForSeconds(1f);
					break;
				}
				if (num2 >= 25)
				{
					reverseCoroutine = null;
					Console.LogWarning("Can't calculate average ahead point!");
					yield break;
				}
				num2++;
				num += 1f;
			}
			ESweepType frontWheel = ESweepType.FL;
			if (steerAngleNormal < 0f)
			{
				frontWheel = ESweepType.FR;
			}
			float num3 = 10f;
			float num4 = 90f;
			Vector3 to = futureTarget - base.transform.position;
			to.y = 0f;
			to.Normalize();
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			float sweepAngle = num3 + (num4 - num3) * Mathf.Clamp(Vector3.Angle(forward, to) / 90f, 0f, 1f);
			if (DEBUG_MODE)
			{
				Console.Log("Beginning straight reverse...");
			}
			float reverseSweepDistanceMin = 1.25f;
			targetSpeed = (Flags.OverrideSpeed ? (0f - Flags.OverriddenReverseSpeed) : (0f - ReverseSpeed));
			bool canBeginSwing = false;
			while (!canBeginSwing)
			{
				yield return new WaitForEndOfFrame();
				float hitDistance = 0f;
				float hitDistance2 = 0f;
				float hitDistance3 = 0f;
				Vector3 hitPoint = Vector3.zero;
				Vector3 hitPoint2 = Vector3.zero;
				if (SweepTurn(frontWheel, sweepAngle * steerAngleNormal, reverse: true, out hitDistance, out hitPoint) || SweepTurn(ESweepType.RL, sweepAngle * steerAngleNormal, reverse: true, out hitDistance2, out hitPoint2) || SweepTurn(ESweepType.RR, sweepAngle * steerAngleNormal, reverse: true, out hitDistance3, out hitPoint2))
				{
					float num5 = 2f;
					if (sensor_RR.obstructionDistance < num5 || sensor_RL.obstructionDistance < num5)
					{
						if (DEBUG_MODE)
						{
							Console.Log("Continued straight reversing will result in collision; starting turn");
						}
						canBeginSwing = true;
					}
				}
				else if (base.transform.InverseTransformPoint(futureTarget).z > 0f - vehicleLength)
				{
					canBeginSwing = true;
				}
			}
			if (DEBUG_MODE)
			{
				Console.Log("Beginning swing...");
			}
			targetSteerAngle_Normalized = steerAngleNormal;
			Vector3 faceTarget = PathUtility.GetClosestPointOnPath(path, base.transform.position, out startPointIndex, out endPointIndex, out pointLerp);
			Vector3 normalized = (PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, 0.5f) - faceTarget).normalized;
			faceTarget += normalized * vehicleLength / 2f;
			bool continueReversing = true;
			while (continueReversing)
			{
				yield return new WaitForEndOfFrame();
				if (path == null)
				{
					continueReversing = false;
					continue;
				}
				to = faceTarget - base.transform.position;
				to.y = 0f;
				to.Normalize();
				forward = base.transform.forward;
				forward.y = 0f;
				Debug.DrawLine(base.transform.position, faceTarget, Color.magenta);
				Debug.DrawLine(base.transform.position, base.transform.position + forward * 5f, Color.cyan);
				if (Vector3.Angle(to, forward) < 20f)
				{
					continueReversing = false;
				}
				float hitDistance4 = float.MaxValue;
				float hitDistance5 = float.MaxValue;
				float hitDistance6 = float.MaxValue;
				if ((SweepTurn(frontWheel, 30f * steerAngleNormal, reverse: true, out hitDistance4, out var hitPoint3) || SweepTurn(ESweepType.RL, 30f * steerAngleNormal, reverse: true, out hitDistance5, out hitPoint3) || SweepTurn(ESweepType.RR, 30f * steerAngleNormal, reverse: true, out hitDistance6, out hitPoint3)) && (hitDistance4 < reverseSweepDistanceMin || hitDistance5 < reverseSweepDistanceMin || hitDistance6 < reverseSweepDistanceMin))
				{
					continueReversing = false;
					if (DEBUG_MODE)
					{
						Console.Log("Reverse sweep obstructed");
					}
				}
			}
			targetSpeed = 0f;
			yield return new WaitUntil(() => vehicle.speed_Kmh >= -1f);
			if (DEBUG_MODE)
			{
				Console.Log("Reverse finished");
			}
			reverseCoroutine = null;
		}

		private void StopReversing()
		{
			if (DEBUG_MODE)
			{
				Console.Log("Reverse stop");
			}
			if (reverseCoroutine != null)
			{
				StopCoroutine(reverseCoroutine);
				reverseCoroutine = null;
				targetSpeed = 0f;
			}
		}

		private Collider GetClosestForwardObstruction(out float obstructionDist)
		{
			Collider result = null;
			obstructionDist = float.MaxValue;
			foreach (Sensor item in new List<Sensor> { sensor_FL, sensor_FM, sensor_FR })
			{
				if (!(item.obstruction != null))
				{
					continue;
				}
				if (Flags.IgnoreTrafficLights)
				{
					VehicleObstacle componentInParent = item.obstruction.GetComponentInParent<VehicleObstacle>();
					if ((object)componentInParent != null && componentInParent.type == VehicleObstacle.EObstacleType.TrafficLight)
					{
						continue;
					}
				}
				if ((Flags.ObstacleMode != DriveFlags.EObstacleMode.IgnoreOnlySquishy || (!(item.obstruction.GetComponentInParent<LandVehicle>() != null) && !(item.obstruction.GetComponentInParent<Player>() != null) && !(item.obstruction.GetComponentInParent<NPC>() != null))) && item.obstructionDistance < obstructionDist)
				{
					result = item.obstruction;
					obstructionDist = item.obstructionDistance;
				}
			}
			return result;
		}

		public bool IsOnVehicleGraph()
		{
			return GetDistanceFromVehicleGraph() < 2.5f;
		}

		private float GetDistanceFromVehicleGraph()
		{
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.graphMask = GraphMask.FromGraphName("General Vehicle Graph");
			return Vector3.Distance(AstarPath.active.GetNearest(base.transform.position, nNConstraint).position, base.transform.position - base.transform.up * vehicle.boundingBoxDimensions.y / 2f);
		}

		private Vector3 GetPathLateralDirection()
		{
			if (path == null)
			{
				Console.LogWarning("Path is null!");
				return Vector3.zero;
			}
			int startPointIndex;
			int endPointIndex;
			float pointLerp;
			Vector3 closestPointOnPath = PathUtility.GetClosestPointOnPath(path, base.transform.position, out startPointIndex, out endPointIndex, out pointLerp);
			Vector3 pointAheadOfPathPoint = PathUtility.GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, 0.01f);
			return Quaternion.AngleAxis(90f, base.transform.up) * (pointAheadOfPathPoint - closestPointOnPath).normalized;
		}

		public bool GetIsStuck()
		{
			if (speedReductionTracker.RecordedHistoryLength() < StuckTimeThreshold)
			{
				return false;
			}
			if (speedReductionTracker.GetLowestValue() < 0.1f)
			{
				return false;
			}
			if (PositionHistoryTracker.RecordedTime >= StuckTimeThreshold)
			{
				Vector3 zero = Vector3.zero;
				for (int i = 0; i < StuckSamples; i++)
				{
					zero += PositionHistoryTracker.GetPositionXSecondsAgo(StuckTimeThreshold / (float)StuckSamples * (float)(i + 1));
				}
				zero /= (float)StuckSamples;
				if (Vector3.Distance(base.transform.position, zero) < StuckDistanceThreshold)
				{
					if (DEBUG_MODE)
					{
						Console.LogWarning("Vehicle stuck");
					}
					return true;
				}
			}
			return false;
		}
	}
}
