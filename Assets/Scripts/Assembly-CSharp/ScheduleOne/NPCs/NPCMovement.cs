using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dragging;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Skating;
using ScheduleOne.Tools;
using ScheduleOne.Vehicles;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.NPCs
{
	public class NPCMovement : NetworkBehaviour
	{
		public enum EAgentType
		{
			Humanoid = 0,
			BigHumanoid = 1,
			IgnoreCosts = 2
		}

		public enum EStance
		{
			None = 0,
			Stanced = 1
		}

		public enum WalkResult
		{
			Failed = 0,
			Interrupted = 1,
			Stopped = 2,
			Partial = 3,
			Success = 4
		}

		public const float VEHICLE_RUNOVER_THRESHOLD = 10f;

		public const float SKATEBOARD_RUNOVER_THRESHOLD = 10f;

		public const float LIGHT_FLINCH_THRESHOLD = 50f;

		public const float HEAVY_FLINCH_THRESHOLD = 100f;

		public const float RAGDOLL_THRESHOLD = 150f;

		public const float MOMENTUM_ANNOYED_THRESHOLD = 10f;

		public const float MOMENTUM_LIGHT_FLINCH_THRESHOLD = 20f;

		public const float MOMENTUM_HEAVY_FLINCH_THRESHOLD = 40f;

		public const float MOMENTUM_RAGDOLL_THRESHOLD = 60f;

		public const bool USE_PATH_CACHE = true;

		public const float STUMBLE_DURATION = 0.66f;

		public const float STUMBLE_FORCE = 7f;

		public const float PLAYER_DIST_IMPACT_THRESHOLD = 30f;

		public static Dictionary<Vector3, Vector3> cachedClosestReachablePoints = new Dictionary<Vector3, Vector3>();

		public static List<Vector3> cachedClosestPointKeys = new List<Vector3>();

		public const float CLOSEST_REACHABLE_POINT_CACHE_MAX_SQR_OFFSET = 1f;

		public bool DEBUG;

		[Header("Settings")]
		public float WalkSpeed = 1.8f;

		public float RunSpeed = 7f;

		public float MoveSpeedMultiplier = 1f;

		[Header("References")]
		public NavMeshAgent Agent;

		public NPCSpeedController SpeedController;

		protected NPC npc;

		public CapsuleCollider capsuleCollider;

		[SerializeField]
		protected NPCAnimation anim;

		[SerializeField]
		protected Rigidbody ragdollCentralRB;

		public SmoothedVelocityCalculator velocityCalculator;

		[SerializeField]
		protected Draggable RagdollDraggable;

		[SerializeField]
		protected Collider RagdollDraggableCollider;

		public float MovementSpeedScale;

		private float ragdollTime;

		private float ragdollStaticTime;

		public UnityEvent<LandVehicle> onHitByCar;

		public UnityEvent onRagdollStart;

		public UnityEvent onRagdollEnd;

		private bool cacheNextPath;

		private Vector3 currentDestination_Reachable = Vector3.zero;

		private Action<WalkResult> walkResultCallback;

		private float currentMaxDistanceForSuccess = 0.5f;

		private bool forceIsMoving;

		private Coroutine FaceDirectionRoutine;

		private List<ConstantForce> ragdollForceComponents = new List<ConstantForce>();

		private float timeUntilNextStumble;

		private float timeSinceStumble = 1000f;

		private Vector3 stumbleDirection = Vector3.zero;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted;

		public bool hasDestination { get; protected set; }

		public bool IsMoving
		{
			get
			{
				if (Agent.hasPath || Agent.pathPending)
				{
					return Agent.remainingDistance > 0.25f;
				}
				return false;
			}
		}

		public bool IsPaused { get; protected set; }

		public Vector3 FootPosition => base.transform.position;

		public float GravityMultiplier { get; protected set; } = 1f;


		public EStance Stance { get; protected set; }

		public float timeSinceHitByCar { get; protected set; }

		public bool FaceDirectionInProgress => FaceDirectionRoutine != null;

		public Vector3 CurrentDestination { get; protected set; } = Vector3.zero;


		public NPCPathCache PathCache { get; private set; } = new NPCPathCache();


		public bool Disoriented { get; set; }

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCMovement_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		private void Start()
		{
			string bakedGUID = npc.BakedGUID;
			if (bakedGUID != string.Empty)
			{
				bakedGUID = ((bakedGUID[bakedGUID.Length - 1] == '1') ? (bakedGUID.Substring(0, bakedGUID.Length - 1) + "2") : (bakedGUID.Substring(0, bakedGUID.Length - 1) + "1"));
				RagdollDraggable.SetGUID(new Guid(bakedGUID));
			}
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			if (!InstanceFinder.IsServer)
			{
				Agent.enabled = false;
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
		}

		protected virtual void Update()
		{
			capsuleCollider.transform.position = ragdollCentralRB.transform.position;
			if (!InstanceFinder.IsServer && InstanceFinder.NetworkManager != null)
			{
				return;
			}
			if (IsPaused)
			{
				Agent.isStopped = true;
			}
			timeSinceHitByCar += Time.deltaTime;
			if ((double)MovementSpeedScale >= 0.0)
			{
				Agent.speed = Mathf.Lerp(WalkSpeed, RunSpeed, MovementSpeedScale) * MoveSpeedMultiplier;
			}
			else
			{
				Agent.speed = 0f;
			}
			if (Disoriented && IsMoving)
			{
				timeUntilNextStumble -= Time.deltaTime;
				if (timeUntilNextStumble <= 0f)
				{
					Stumble();
				}
			}
			timeSinceStumble += Time.deltaTime;
			if (timeSinceStumble < 0.66f)
			{
				Agent.Move(stumbleDirection * (0.66f - timeSinceStumble) * Time.deltaTime * 7f);
			}
			if (DEBUG)
			{
				Debug.Log(npc.fullName + " movement debug: ");
				Debug.Log("HasPath: " + Agent.hasPath);
				Debug.Log("PathPending: " + Agent.pathPending);
				Debug.Log("RemainingDistance: " + Agent.remainingDistance);
				Debug.Log("IsMoving: " + IsMoving);
				Debug.Log("IsPaused: " + IsPaused);
				Debug.Log("IsRagdolled: " + anim.Avatar.Ragdolled);
				Debug.Log("IsInVehicle: " + npc.IsInVehicle);
				Debug.Log("HasDestination: " + hasDestination);
				Debug.Log("CurrentDestination: " + CurrentDestination.ToString());
				Debug.Log("Movement Speed Scale: " + MovementSpeedScale);
			}
			UpdateRagdoll();
			UpdateDestination();
		}

		private void UpdateRagdoll()
		{
			if (npc.IsConscious && InstanceFinder.IsServer && anim.Avatar.Ragdolled && ragdollStaticTime > 1.5f)
			{
				DeactivateRagdoll();
			}
		}

		[Button]
		private void Stumble()
		{
			timeUntilNextStumble = UnityEngine.Random.Range(5f, 15f);
			if (UnityEngine.Random.Range(1f, 0f) < 0.1f)
			{
				ActivateRagdoll_Server();
				return;
			}
			timeSinceStumble = 0f;
			stumbleDirection = UnityEngine.Random.onUnitSphere;
			stumbleDirection.y = 0f;
			stumbleDirection.Normalize();
		}

		private void UpdateDestination()
		{
			if (!hasDestination)
			{
				return;
			}
			if (npc.IsInVehicle)
			{
				EndSetDestination(WalkResult.Interrupted);
			}
			else
			{
				if (!CanMove() || IsMoving || Agent.pathPending)
				{
					return;
				}
				if (IsAsCloseAsPossible(CurrentDestination))
				{
					if (Agent.hasPath)
					{
						Agent.ResetPath();
					}
					if (Vector3.Distance(CurrentDestination, FootPosition) < currentMaxDistanceForSuccess || Vector3.Distance(CurrentDestination, base.transform.position) < currentMaxDistanceForSuccess)
					{
						EndSetDestination(WalkResult.Success);
					}
					else
					{
						EndSetDestination(WalkResult.Partial);
					}
				}
				else
				{
					SetDestination(CurrentDestination, walkResultCallback, interruptExistingCallback: false, currentMaxDistanceForSuccess);
				}
			}
		}

		protected virtual void FixedUpdate()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (Agent.path != null && cacheNextPath && Agent.path.corners.Length > 1)
			{
				cacheNextPath = false;
				PathCache.AddPath(Agent.path.corners[0], Agent.path.corners[Agent.path.corners.Length - 1], Agent.path);
			}
			if (anim.Avatar.Ragdolled)
			{
				ragdollTime += Time.fixedDeltaTime;
				if (ragdollCentralRB.velocity.magnitude < 0.25f)
				{
					ragdollStaticTime += Time.fixedDeltaTime;
				}
				else
				{
					ragdollStaticTime = 0f;
				}
			}
			else
			{
				ragdollStaticTime = 0f;
			}
		}

		private void UpdateAvoidance()
		{
			Player.GetClosestPlayer(base.transform.position, out var distance);
			if (distance > 25f)
			{
				Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
				return;
			}
			Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
			Agent.avoidancePriority = Mathf.RoundToInt(velocityCalculator.Velocity.magnitude * 10f);
		}

		public void OnTriggerEnter(Collider other)
		{
			CheckHit(other, capsuleCollider, isCollision: false, other.transform.position);
		}

		public void OnCollisionEnter(Collision collision)
		{
			CheckHit(collision.collider, collision.contacts[0].thisCollider, isCollision: true, collision.contacts[0].point);
		}

		private void CheckHit(Collider other, Collider thisCollider, bool isCollision, Vector3 hitPoint)
		{
			float distance;
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, out distance);
			if (distance > 30f)
			{
				return;
			}
			if ((other.gameObject.layer == LayerMask.NameToLayer("Vehicle") || other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) && !anim.Avatar.Ragdolled)
			{
				LandVehicle landVehicle = other.GetComponentInParent<LandVehicle>();
				if (landVehicle == null)
				{
					VehicleHumanoidCollider componentInParent = other.GetComponentInParent<VehicleHumanoidCollider>();
					if (componentInParent != null)
					{
						landVehicle = componentInParent.vehicle;
					}
				}
				if (landVehicle != null && !npc.CurrentVehicle != (bool)landVehicle && Mathf.Abs(landVehicle.speed_Kmh) > 10f)
				{
					ActivateRagdoll_Server();
					if (onHitByCar != null)
					{
						onHitByCar.Invoke(other.GetComponentInParent<LandVehicle>());
					}
					timeSinceHitByCar = 0f;
				}
			}
			else if (other.GetComponentInParent<Skateboard>() != null && !anim.Avatar.Ragdolled)
			{
				if (other.GetComponentInParent<Skateboard>().VelocityCalculator.Velocity.magnitude > 2.777778f)
				{
					ActivateRagdoll_Server();
					npc.PlayVO(EVOLineType.Hurt);
				}
			}
			else
			{
				if (!(other.GetComponentInParent<PhysicsDamageable>() != null))
				{
					return;
				}
				PhysicsDamageable componentInParent2 = other.GetComponentInParent<PhysicsDamageable>();
				float num = Mathf.Sqrt(componentInParent2.Rb.mass) * componentInParent2.Rb.velocity.magnitude;
				float magnitude = componentInParent2.Rb.velocity.magnitude;
				if (magnitude > 40f)
				{
					return;
				}
				if (magnitude > 1f)
				{
					magnitude = Mathf.Pow(magnitude, 1.5f);
				}
				else
				{
					magnitude = Mathf.Sqrt(magnitude);
				}
				if (num > 10f)
				{
					float num2 = 1f;
					switch (Stance)
					{
					case EStance.None:
						num2 = 1f;
						break;
					case EStance.Stanced:
						num2 = 0.5f;
						break;
					}
					float num3 = num * 2.5f;
					float num4 = num * 0.3f;
					if (num > 20f)
					{
						npc.Health.TakeDamage(num4, isLethal: false);
						npc.ProcessImpactForce(hitPoint, componentInParent2.Rb.velocity.normalized, num3 * num2);
					}
					Impact impact = new Impact(default(RaycastHit), hitPoint, componentInParent2.Rb.velocity.normalized, num3, num4, EImpactType.PhysicsProp, (distance < 15f) ? closestPlayer : null, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
					npc.responses.ImpactReceived(impact);
				}
			}
		}

		public void Warp(Transform target)
		{
			Warp(target.position);
			FaceDirection(target.forward);
		}

		public void Warp(Vector3 position)
		{
			if (InstanceFinder.IsServer)
			{
				Agent.Warp(position);
				ReceiveWarp(position);
			}
		}

		[ObserversRpc(ExcludeServer = true)]
		private void ReceiveWarp(Vector3 position)
		{
			RpcWriter___Observers_ReceiveWarp_4276783012(position);
		}

		public void VisibilityChange(bool visible)
		{
			capsuleCollider.gameObject.SetActive(visible);
		}

		public bool CanMove()
		{
			if (!anim.Avatar.Ragdolled && !npc.isInBuilding)
			{
				return !npc.IsInVehicle;
			}
			return false;
		}

		public void SetAgentType(EAgentType type)
		{
			string text = type.ToString();
			if (type == EAgentType.BigHumanoid)
			{
				text = "Big Humanoid";
			}
			if (type == EAgentType.IgnoreCosts)
			{
				text = "Ignore Costs";
			}
			Agent.agentTypeID = NavMeshUtility.GetNavMeshAgentID(text);
		}

		public void SetSeat(AvatarSeat seat)
		{
			npc.Avatar.Anim.SetSeat(seat);
			Agent.enabled = seat == null && InstanceFinder.IsServer;
		}

		public void SetStance(EStance stance)
		{
			Stance = stance;
		}

		public void SetGravityMultiplier(float multiplier)
		{
			GravityMultiplier = multiplier;
			foreach (ConstantForce ragdollForceComponent in ragdollForceComponents)
			{
				ragdollForceComponent.force = Physics.gravity * GravityMultiplier * ragdollForceComponent.GetComponent<Rigidbody>().mass;
			}
		}

		public void SetRagdollDraggable(bool draggable)
		{
			RagdollDraggable.enabled = draggable;
			RagdollDraggableCollider.enabled = draggable;
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		private void ActivateRagdoll_Server()
		{
			RpcWriter___Server_ActivateRagdoll_Server_2166136261();
			RpcLogic___ActivateRagdoll_Server_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		public void ActivateRagdoll(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
		{
			RpcWriter___Observers_ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
			RpcLogic___ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
		}

		[ObserversRpc(RunLocally = true)]
		public void ApplyRagdollForce(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
		{
			RpcWriter___Observers_ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
			RpcLogic___ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
		}

		[ObserversRpc(RunLocally = true)]
		public void DeactivateRagdoll()
		{
			RpcWriter___Observers_DeactivateRagdoll_2166136261();
			RpcLogic___DeactivateRagdoll_2166136261();
		}

		private bool SmartSampleNavMesh(Vector3 position, out NavMeshHit hit, float minRadius = 1f, float maxRadius = 10f, int steps = 3)
		{
			hit = default(NavMeshHit);
			NavMeshQueryFilter filter = default(NavMeshQueryFilter);
			filter.agentTypeID = NavMeshUtility.GetNavMeshAgentID("Humanoid");
			filter.areaMask = -1;
			for (int i = 0; i < steps; i++)
			{
				float maxDistance = Mathf.Lerp(minRadius, maxRadius, i / steps);
				if (NavMesh.SamplePosition(base.transform.position, out hit, maxDistance, filter))
				{
					return true;
				}
			}
			return false;
		}

		public void SetDestination(Vector3 pos)
		{
			SetDestination(pos, null, 1f);
		}

		public void SetDestination(Vector3 pos, Action<WalkResult> callback = null, float maximumDistanceForSuccess = 1f)
		{
			SetDestination(pos, callback, interruptExistingCallback: true, maximumDistanceForSuccess);
		}

		private void SetDestination(Vector3 pos, Action<WalkResult> callback = null, bool interruptExistingCallback = true, float successThreshold = 1f)
		{
			if (npc.Avatar.Anim.IsSeated)
			{
				npc.Movement.SetSeat(null);
			}
			if (!InstanceFinder.IsServer)
			{
				Console.LogWarning("NPCMovement.SetDestination called on client");
				return;
			}
			if (npc.isInBuilding)
			{
				npc.ExitBuilding();
			}
			if (!CanMove())
			{
				Console.LogWarning("NPCMovement.SetDestination called but CanWalk == false (" + npc.fullName + ")");
				return;
			}
			if (!Agent.isOnNavMesh)
			{
				Console.LogWarning("NPC is not on navmesh; warping to navmesh");
				if (!SmartSampleNavMesh(base.transform.position, out var hit))
				{
					Console.LogWarning("NavMesh sample failed at " + base.transform.position.ToString());
				}
				Agent.Warp(hit.position);
				Agent.enabled = false;
				Agent.enabled = true;
			}
			if (walkResultCallback != null && interruptExistingCallback)
			{
				EndSetDestination(WalkResult.Interrupted);
			}
			walkResultCallback = callback;
			currentMaxDistanceForSuccess = successThreshold;
			if (npc.IsInVehicle)
			{
				Console.LogWarning("SetDestination called but NPC is in a vehicle; returning WalkResult.Failed");
				EndSetDestination(WalkResult.Failed);
				return;
			}
			Vector3 closestPoint = Vector3.zero;
			if (!GetClosestReachablePoint(pos, out closestPoint))
			{
				EndSetDestination(WalkResult.Failed);
				return;
			}
			hasDestination = true;
			CurrentDestination = pos;
			currentDestination_Reachable = closestPoint;
			NavMeshPath path = PathCache.GetPath(Agent.transform.position, closestPoint, 1f);
			if (path != null)
			{
				if (Agent.SetPath(path))
				{
					Agent.isStopped = false;
				}
				else
				{
					Agent.SetDestination(closestPoint);
					cacheNextPath = true;
				}
			}
			else
			{
				Agent.SetDestination(closestPoint);
				cacheNextPath = true;
			}
			if (IsPaused)
			{
				Agent.isStopped = true;
			}
			forceIsMoving = true;
		}

		private void EndSetDestination(WalkResult result)
		{
			if (walkResultCallback != null)
			{
				walkResultCallback(result);
				walkResultCallback = null;
			}
			hasDestination = false;
			CurrentDestination = Vector3.zero;
			currentDestination_Reachable = Vector3.zero;
		}

		public void Stop()
		{
			if (Agent.isOnNavMesh)
			{
				Agent.ResetPath();
				Agent.velocity = Vector3.zero;
				Agent.isStopped = true;
				Agent.isStopped = false;
			}
			if (InstanceFinder.IsServer)
			{
				EndSetDestination(WalkResult.Stopped);
			}
		}

		public void WarpToNavMesh()
		{
		}

		public void FacePoint(Vector3 point, float lerpTime = 0.5f)
		{
			Vector3 forward = new Vector3(point.x, base.transform.position.y, point.z) - base.transform.position;
			if (FaceDirectionRoutine != null)
			{
				StopCoroutine(FaceDirectionRoutine);
			}
			FaceDirectionRoutine = StartCoroutine(FaceDirection_Process(forward, lerpTime));
		}

		public void FaceDirection(Vector3 forward, float lerpTime = 0.5f)
		{
			if (FaceDirectionRoutine != null)
			{
				Singleton<CoroutineService>.Instance.StopCoroutine(FaceDirectionRoutine);
			}
			FaceDirectionRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(FaceDirection_Process(forward, lerpTime));
		}

		protected IEnumerator FaceDirection_Process(Vector3 forward, float lerpTime)
		{
			if (lerpTime > 0f)
			{
				Quaternion startRot = base.transform.rotation;
				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
				{
					base.transform.rotation = Quaternion.Lerp(startRot, Quaternion.LookRotation(forward, Vector3.up), i / lerpTime);
					yield return new WaitForEndOfFrame();
				}
			}
			base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			FaceDirectionRoutine = null;
		}

		public void PauseMovement()
		{
			IsPaused = true;
			Agent.isStopped = true;
			Agent.velocity = Vector3.zero;
		}

		public void ResumeMovement()
		{
			IsPaused = false;
			if (Agent.isOnNavMesh)
			{
				Agent.isStopped = false;
			}
		}

		public bool IsAsCloseAsPossible(Vector3 location, float distanceThreshold = 0.5f)
		{
			Vector3 closestPoint = Vector3.zero;
			if (!GetClosestReachablePoint(location, out closestPoint))
			{
				return false;
			}
			return Vector3.Distance(FootPosition, closestPoint) < distanceThreshold;
		}

		public bool GetClosestReachablePoint(Vector3 targetPosition, out Vector3 closestPoint)
		{
			closestPoint = Vector3.zero;
			bool flag = false;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < cachedClosestPointKeys.Count; i++)
			{
				if (Vector3.SqrMagnitude(cachedClosestPointKeys[i] - targetPosition) < 1f)
				{
					vector = cachedClosestReachablePoints[cachedClosestPointKeys[i]];
					flag = true;
					break;
				}
			}
			if (flag)
			{
				closestPoint = vector;
				return true;
			}
			float num = 3f;
			for (int j = 0; j < 3; j++)
			{
				if (NavMeshUtility.SamplePosition(targetPosition, out var hit, num * (float)(j + 1), -1) && CanGetTo(hit.position))
				{
					closestPoint = hit.position;
					return true;
				}
			}
			return false;
		}

		public bool CanGetTo(Vector3 position, float proximityReq = 1f)
		{
			NavMeshPath path = null;
			return CanGetTo(position, proximityReq, out path);
		}

		public bool CanGetTo(Vector3 position, float proximityReq, out NavMeshPath path)
		{
			path = null;
			if (Vector3.Distance(position, base.transform.position) < proximityReq)
			{
				return true;
			}
			if (!Agent.isOnNavMesh)
			{
				return false;
			}
			if (!NavMeshUtility.SamplePosition(position, out var hit, 2f, -1))
			{
				return false;
			}
			path = GetPathTo(hit.position, proximityReq);
			if (path == null)
			{
				Debug.DrawLine(FootPosition, hit.position, Color.red, 1f);
				return false;
			}
			if (path.corners.Length < 2)
			{
				Console.LogWarning("Path length < 2");
				return false;
			}
			float num = Vector3.Distance(path.corners[path.corners.Length - 1], hit.position);
			float num2 = Vector3.Distance(hit.position, position);
			if (num <= proximityReq)
			{
				return num2 <= proximityReq;
			}
			return false;
		}

		private NavMeshPath GetPathTo(Vector3 position, float proximityReq = 1f)
		{
			if (!Agent.isOnNavMesh)
			{
				Console.LogWarning("Agent not on nav mesh!");
				return null;
			}
			NavMeshPath navMeshPath = new NavMeshPath();
			NavMeshUtility.SamplePosition(position, out var hit, 2f, -1);
			if (!Agent.CalculatePath(hit.position, navMeshPath))
			{
				return null;
			}
			float num = Vector3.Distance(navMeshPath.corners[navMeshPath.corners.Length - 1], hit.position);
			float num2 = Vector3.Distance(hit.position, position);
			if (num <= proximityReq && num2 <= proximityReq)
			{
				return navMeshPath;
			}
			return null;
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterObserversRpc(0u, RpcReader___Observers_ReceiveWarp_4276783012);
				RegisterServerRpc(1u, RpcReader___Server_ActivateRagdoll_Server_2166136261);
				RegisterObserversRpc(2u, RpcReader___Observers_ActivateRagdoll_2690242654);
				RegisterObserversRpc(3u, RpcReader___Observers_ApplyRagdollForce_2690242654);
				RegisterObserversRpc(4u, RpcReader___Observers_DeactivateRagdoll_2166136261);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCMovementAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_ReceiveWarp_4276783012(Vector3 position)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteVector3(position);
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveWarp_4276783012(Vector3 position)
		{
			Agent.Warp(position);
		}

		private void RpcReader___Observers_ReceiveWarp_4276783012(PooledReader PooledReader0, Channel channel)
		{
			Vector3 position = PooledReader0.ReadVector3();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveWarp_4276783012(position);
			}
		}

		private void RpcWriter___Server_ActivateRagdoll_Server_2166136261()
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendServerRpc(1u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___ActivateRagdoll_Server_2166136261()
		{
			ActivateRagdoll(Vector3.zero, Vector3.zero, 0f);
		}

		private void RpcReader___Server_ActivateRagdoll_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___ActivateRagdoll_Server_2166136261();
			}
		}

		private void RpcWriter___Observers_ActivateRagdoll_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteVector3(forcePoint);
				writer.WriteVector3(forceDir);
				writer.WriteSingle(forceMagnitude);
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ActivateRagdoll_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
		{
			anim.SetRagdollActive(active: true);
			if (onRagdollStart != null)
			{
				onRagdollStart.Invoke();
			}
			if (InstanceFinder.IsServer)
			{
				EndSetDestination(WalkResult.Interrupted);
				Agent.enabled = false;
			}
			capsuleCollider.gameObject.SetActive(value: false);
			if (forceMagnitude > 0f)
			{
				ApplyRagdollForce(forcePoint, forceDir, forceMagnitude);
			}
		}

		private void RpcReader___Observers_ActivateRagdoll_2690242654(PooledReader PooledReader0, Channel channel)
		{
			Vector3 forcePoint = PooledReader0.ReadVector3();
			Vector3 forceDir = PooledReader0.ReadVector3();
			float forceMagnitude = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ActivateRagdoll_2690242654(forcePoint, forceDir, forceMagnitude);
			}
		}

		private void RpcWriter___Observers_ApplyRagdollForce_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				writer.WriteVector3(forcePoint);
				writer.WriteVector3(forceDir);
				writer.WriteSingle(forceMagnitude);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ApplyRagdollForce_2690242654(Vector3 forcePoint, Vector3 forceDir, float forceMagnitude)
		{
			(from x in npc.Avatar.RagdollRBs
				select new
				{
					rb = x,
					dist = Vector3.Distance(x.transform.position, forcePoint)
				} into x
				orderby x.dist
				select x).First().rb.AddForceAtPosition(forceDir.normalized * forceMagnitude, forcePoint, ForceMode.Impulse);
		}

		private void RpcReader___Observers_ApplyRagdollForce_2690242654(PooledReader PooledReader0, Channel channel)
		{
			Vector3 forcePoint = PooledReader0.ReadVector3();
			Vector3 forceDir = PooledReader0.ReadVector3();
			float forceMagnitude = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ApplyRagdollForce_2690242654(forcePoint, forceDir, forceMagnitude);
			}
		}

		private void RpcWriter___Observers_DeactivateRagdoll_2166136261()
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				if ((object)networkManager == null)
				{
					networkManager = InstanceFinder.NetworkManager;
				}
				if ((object)networkManager != null)
				{
					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
				else
				{
					Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				}
			}
			else
			{
				Channel channel = Channel.Reliable;
				PooledWriter writer = WriterPool.GetWriter();
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___DeactivateRagdoll_2166136261()
		{
			capsuleCollider.gameObject.SetActive(npc.isVisible);
			anim.SetRagdollActive(active: false);
			base.transform.position = anim.Avatar.transform.position;
			base.transform.rotation = anim.Avatar.transform.rotation;
			anim.Avatar.transform.localPosition = Vector3.zero;
			anim.Avatar.transform.localRotation = Quaternion.identity;
			velocityCalculator.FlushBuffer();
			if (InstanceFinder.IsServer)
			{
				Agent.enabled = false;
				if (!Agent.isOnNavMesh)
				{
					NavMeshQueryFilter navMeshQueryFilter = default(NavMeshQueryFilter);
					navMeshQueryFilter.agentTypeID = NavMeshUtility.GetNavMeshAgentID("Humanoid");
					navMeshQueryFilter.areaMask = -1;
					if (SmartSampleNavMesh(base.transform.position, out var hit))
					{
						Agent.Warp(hit.position);
					}
					Agent.enabled = false;
					Agent.enabled = true;
				}
			}
			if (onRagdollEnd != null)
			{
				onRagdollEnd.Invoke();
			}
		}

		private void RpcReader___Observers_DeactivateRagdoll_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___DeactivateRagdoll_2166136261();
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCMovement_Assembly_002DCSharp_002Edll()
		{
			npc = GetComponent<NPC>();
			NPC nPC = npc;
			nPC.onVisibilityChanged = (Action<bool>)Delegate.Combine(nPC.onVisibilityChanged, new Action<bool>(VisibilityChange));
			VisibilityChange(npc.isVisible);
			InvokeRepeating("UpdateAvoidance", 0f, 0.5f);
			for (int i = 0; i < npc.Avatar.RagdollRBs.Length; i++)
			{
				ragdollForceComponents.Add(npc.Avatar.RagdollRBs[i].gameObject.AddComponent<ConstantForce>());
			}
			SetRagdollDraggable(draggable: false);
			SetGravityMultiplier(1f);
		}
	}
}
