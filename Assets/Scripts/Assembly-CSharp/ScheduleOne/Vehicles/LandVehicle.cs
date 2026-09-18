using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EPOOutline;
using FishNet;
using FishNet.Component.Ownership;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using Pathfinding;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.Vehicles.AI;
using ScheduleOne.Vehicles.Modification;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ScheduleOne.Vehicles
{
	[RequireComponent(typeof(VehicleCamera))]
	[RequireComponent(typeof(NetworkTransform))]
	[RequireComponent(typeof(PredictedOwner))]
	[RequireComponent(typeof(VehicleCollisionDetector))]
	[RequireComponent(typeof(PhysicsDamageable))]
	public class LandVehicle : NetworkBehaviour, IGUIDRegisterable, ISaveable
	{
		[Serializable]
		public class BodyMesh
		{
			public MeshRenderer Renderer;

			public int MaterialIndex;
		}

		public delegate void VehiclePlayerEvent(Player player);

		public const float KINEMATIC_THRESHOLD_DISTANCE = 30f;

		public const float MAX_TURNOVER_SPEED = 5f;

		public const float TURNOVER_FORCE = 8f;

		public const bool USE_WHEEL = false;

		public const float SPEED_DISPLAY_MULTIPLIER = 1.4f;

		public bool DEBUG;

		[Header("Settings")]
		[SerializeField]
		protected string vehicleName = "Vehicle";

		[SerializeField]
		protected string vehicleCode = "vehicle_code";

		[SerializeField]
		protected float vehiclePrice = 1000f;

		public bool UseHumanoidCollider = true;

		public bool SpawnAsPlayerOwned;

		[Header("References")]
		[SerializeField]
		protected GameObject vehicleModel;

		[SerializeField]
		protected WheelCollider[] driveWheels;

		[SerializeField]
		protected WheelCollider[] steerWheels;

		[SerializeField]
		protected WheelCollider[] handbrakeWheels;

		[HideInInspector]
		public List<Wheel> wheels = new List<Wheel>();

		[SerializeField]
		protected InteractableObject intObj;

		[SerializeField]
		protected List<Transform> exitPoints = new List<Transform>();

		[SerializeField]
		protected Rigidbody rb;

		public VehicleSeat[] Seats;

		public BoxCollider boundingBox;

		public VehicleAgent Agent;

		public SmoothedVelocityCalculator VelocityCalculator;

		public StorageDoorAnimation Trunk;

		public NavMeshObstacle NavMeshObstacle;

		public NavmeshCut NavmeshCut;

		public VehicleHumanoidCollider HumanoidColliderContainer;

		[SerializeField]
		protected Transform centerOfMass;

		[SerializeField]
		protected Transform cameraOrigin;

		[SerializeField]
		protected VehicleLights lights;

		[Header("Steer settings")]
		[SerializeField]
		protected float maxSteeringAngle = 25f;

		[SerializeField]
		protected float steerRate = 50f;

		[SerializeField]
		protected bool flipSteer;

		[Header("Drive settings")]
		[SerializeField]
		protected AnimationCurve motorTorque = new AnimationCurve(new Keyframe(0f, 200f), new Keyframe(50f, 300f), new Keyframe(200f, 0f));

		public float TopSpeed = 60f;

		[Range(2f, 16f)]
		[SerializeField]
		protected float diffGearing = 4f;

		[SerializeField]
		protected float handBrakeForce = 300f;

		[SerializeField]
		protected AnimationCurve brakeForce = new AnimationCurve(new Keyframe(0f, 200f), new Keyframe(50f, 300f), new Keyframe(200f, 0f));

		[Range(0.5f, 10f)]
		[SerializeField]
		protected float downforce = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		protected float reverseMultiplier = 0.35f;

		[Header("Color Settings")]
		[SerializeField]
		protected BodyMesh[] BodyMeshes;

		public EVehicleColor color = EVehicleColor.White;

		private EVehicleColor appliedColor = EVehicleColor.White;

		[Header("Outline settings")]
		[SerializeField]
		protected List<GameObject> outlineRenderers = new List<GameObject>();

		protected Outlinable outlineEffect;

		[Header("Control overrides")]
		public bool overrideControls;

		public float throttleOverride;

		public float steerOverride;

		[Header("Storage settings")]
		public StorageEntity Storage;

		private VehicleSeat localPlayerSeat;

		private List<float> previousSpeeds = new List<float>();

		private int previousSpeedsSampleSize = 20;

		[SyncVar(Channel = Channel.Unreliable, SendRate = 0.05f, WritePermissions = WritePermission.ClientUnsynchronized)]
		public float currentSteerAngle;

		private float lastFrameSteerAngle;

		private float lastReplicatedSteerAngle;

		private bool justExitedVehicle;

		[CompilerGenerated]
		[SyncVar(Channel = Channel.Unreliable, SendRate = 0.1f, WritePermissions = WritePermission.ClientUnsynchronized)]
		public bool _003CbrakesApplied_003Ek__BackingField;

		[CompilerGenerated]
		[SyncVar(Channel = Channel.Unreliable, SendRate = 0.1f, WritePermissions = WritePermission.ClientUnsynchronized)]
		public bool _003CisReversing_003Ek__BackingField;

		private Vector3 lastFramePosition = Vector3.zero;

		private Transform closestExitPoint;

		[HideInInspector]
		public ParkData CurrentParkData;

		private VehicleLoader loader = new VehicleLoader();

		public VehiclePlayerEvent onPlayerEnterVehicle;

		public VehiclePlayerEvent onPlayerExitVehicle;

		public UnityEvent onVehicleStart;

		public UnityEvent onVehicleStop;

		public UnityEvent onHandbrakeApplied;

		public SyncVar<float> syncVar___currentSteerAngle;

		public SyncVar<bool> syncVar____003CbrakesApplied_003Ek__BackingField;

		public SyncVar<bool> syncVar____003CisReversing_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted;

		public string VehicleName => vehicleName;

		public string VehicleCode => vehicleCode;

		public float VehiclePrice => vehiclePrice;

		public bool IsPlayerOwned { get; protected set; }

		public bool IsVisible { get; protected set; } = true;


		public Guid GUID { get; protected set; }

		public Vector3 boundingBoxDimensions => new Vector3(boundingBox.size.x * boundingBox.transform.localScale.x, boundingBox.size.y * boundingBox.transform.localScale.y, boundingBox.size.z * boundingBox.transform.localScale.z);

		public Transform driverEntryPoint => exitPoints[0];

		public Rigidbody Rb => rb;

		public float ActualMaxSteeringAngle
		{
			get
			{
				if (!MaxSteerAngleOverridden)
				{
					return maxSteeringAngle;
				}
				return OverriddenMaxSteerAngle;
			}
		}

		public bool MaxSteerAngleOverridden { get; private set; }

		public float OverriddenMaxSteerAngle { get; private set; }

		public bool colorOverridden { get; protected set; }

		public EVehicleColor overrideColor { get; protected set; } = EVehicleColor.White;


		public int Capacity => Seats.Length;

		public int CurrentPlayerOccupancy => Seats.Count((VehicleSeat s) => s.isOccupied);

		public bool localPlayerIsDriver { get; protected set; }

		public bool localPlayerIsInVehicle { get; protected set; }

		public bool isOccupied { get; private set; }

		public Player DriverPlayer
		{
			get
			{
				if (Seats[0].Occupant != null)
				{
					return Seats[0].Occupant;
				}
				return null;
			}
		}

		public List<Player> OccupantPlayers => (from s in Seats
			where s.isOccupied
			select s.Occupant).ToList();

		public NPC[] OccupantNPCs { get; protected set; } = new NPC[0];


		public float speed_Kmh { get; protected set; }

		public float speed_Ms => speed_Kmh / 3.6f;

		public float speed_Mph => speed_Kmh * 0.621371f;

		public float currentThrottle { get; protected set; }

		//public bool brakesApplied
		//{
		//	[CompilerGenerated]
		//	get
		//	{
		//		return SyncAccessor__003CbrakesApplied_003Ek__BackingField;
		//	}
		//	[CompilerGenerated]
		//	set
		//	{
		//		syncVar____003CbrakesApplied_003Ek__BackingField.SetValue(value,  true);
		//	}
		//}

        public bool brakesApplied
        {
            get
            {
                return _003CbrakesApplied_003Ek__BackingField;
            }
            set
            {
                _003CbrakesApplied_003Ek__BackingField = value;

                syncVar____003CbrakesApplied_003Ek__BackingField.SetValue(value, true);
            }
        }

        //      public bool isReversing
        //{
        //	[CompilerGenerated]
        //	get
        //	{
        //		return SyncAccessor__003CisReversing_003Ek__BackingField;
        //	}
        //	[CompilerGenerated]
        //	set
        //	{
        //		syncVar____003CisReversing_003Ek__BackingField.SetValue(value,  true);
        //	}
        //}

        public bool isReversing
        {
            get
            {
                return _003CisReversing_003Ek__BackingField;
            }
            set
            {
                _003CisReversing_003Ek__BackingField = value;

                syncVar____003CisReversing_003Ek__BackingField.SetValue(value, true);
            }
        }

        public bool isStatic { get; protected set; }

		public bool handbrakeApplied { get; protected set; }

		public float boundingBaseOffset => base.transform.InverseTransformPoint(boundingBox.transform.position).y + boundingBox.size.y * 0.5f;

		public bool isParked => CurrentParkingLot != null;

		public ParkingLot CurrentParkingLot { get; protected set; }

		public ParkingSpot CurrentParkingSpot { get; protected set; }

		public string SaveFolderName => vehicleCode + "_" + GUID.ToString().Substring(0, 6);

		public string SaveFileName => "Vehicle";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string> { "Contents" };


		public List<string> LocalExtraFolders { get; set; } = new List<string>();


		public bool HasChanged { get; set; }

		public float SyncAccessor_currentSteerAngle
		{
			get
			{
				return currentSteerAngle;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					currentSteerAngle = value;
				}
				if (Application.isPlaying)
				{
					syncVar___currentSteerAngle.SetValue(value, value != 0);
				}
			}
		}

		public bool SyncAccessor__003CbrakesApplied_003Ek__BackingField
		{
			get
			{
				return brakesApplied;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					brakesApplied = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CbrakesApplied_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public bool SyncAccessor__003CisReversing_003Ek__BackingField
		{
			get
			{
				return isReversing;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					isReversing = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CisReversing_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EVehicles_002ELandVehicle_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			base.NetworkObject.GiveOwnership(base.LocalConnection);
			rb.isKinematic = false;
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			if (SpawnAsPlayerOwned)
			{
				IsPlayerOwned = true;
				SetIsPlayerOwned(null, playerOwned: true);
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (connection.IsHost)
			{
				return;
			}
			for (int i = 0; i < Seats.Length; i++)
			{
				if (Seats[i].Occupant != null)
				{
					SetSeatOccupant(connection, i, Seats[i].Occupant.Connection);
				}
			}
			if (isParked)
			{
				Park_Networked(connection, CurrentParkData);
			}
			if (IsPlayerOwned)
			{
				SetIsPlayerOwned(connection, playerOwned: true);
			}
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			rb.isKinematic = false;
			if (!base.IsOwner && !InstanceFinder.IsHost)
			{
				rb.isKinematic = true;
			}
			rb.interpolation = RigidbodyInterpolation.Interpolate;
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void SetIsPlayerOwned(NetworkConnection conn, bool playerOwned)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetIsPlayerOwned_214505783(conn, playerOwned);
				RpcLogic___SetIsPlayerOwned_214505783(conn, playerOwned);
			}
			else
			{
				RpcWriter___Target_SetIsPlayerOwned_214505783(conn, playerOwned);
			}
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		protected virtual void Start()
		{
			intObj.onHovered.AddListener(Hovered);
			intObj.onInteractStart.AddListener(Interacted);
			if (centerOfMass != null)
			{
				rb.centerOfMass = base.transform.InverseTransformPoint(centerOfMass.transform.position);
			}
			ApplyColor(color);
			if (GUID == Guid.Empty)
			{
				GUID = GUIDManager.GenerateUniqueGUID();
			}
			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
			GameInput.RegisterExitListener(Exit);
			if (UseHumanoidCollider)
			{
				HumanoidColliderContainer.vehicle = this;
				HumanoidColliderContainer.transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
				Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
				Collider[] componentsInChildren2 = HumanoidColliderContainer.GetComponentsInChildren<Collider>(includeInactive: true);
				Collider[] array = componentsInChildren;
				foreach (Collider collider in array)
				{
					Collider[] array2 = componentsInChildren2;
					foreach (Collider collider2 in array2)
					{
						if (DEBUG)
						{
							Debug.Log("Ignoring collision between " + collider.name + " and " + collider2.name);
						}
						Physics.IgnoreCollision(collider, collider2, ignore: true);
					}
				}
			}
			else
			{
				HumanoidColliderContainer.gameObject.SetActive(value: false);
			}
		}

		private void Exit(ExitAction action)
		{
			if (!action.used && action.exitType == ExitType.Escape && localPlayerIsInVehicle)
			{
				action.used = true;
				ExitVehicle();
			}
		}

		protected virtual void OnDestroy()
		{
			if (NetworkSingleton<MoneyManager>.InstanceExists)
			{
				MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
				instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
			}
			if (HumanoidColliderContainer != null)
			{
				UnityEngine.Object.Destroy(HumanoidColliderContainer.gameObject);
			}
		}

		private void GetNetworth(MoneyManager.FloatContainer container)
		{
			if (IsPlayerOwned)
			{
				container.ChangeValue(GetVehicleValue());
			}
		}

		protected virtual void Update()
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			bool flag = localPlayerIsDriver || base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost);
			rb.interpolation = (flag ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
			HasChanged = true;
			if (localPlayerIsInVehicle && GameInput.GetButtonDown(GameInput.ButtonCode.Interact) && !GameInput.IsTyping)
			{
				ExitVehicle();
			}
			if (color != appliedColor)
			{
				ApplyColor(color);
			}
			if (IsPlayerOwned)
			{
				if (!localPlayerIsDriver && (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.SleepInProgress || Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.Camera.transform.position) > 30f))
				{
					rb.isKinematic = true;
				}
				else if (base.NetworkObject.Owner == null || base.NetworkObject.OwnerId == -1 || base.NetworkObject.Owner == base.LocalConnection)
				{
					rb.isKinematic = false;
				}
			}
			if (overrideControls)
			{
				currentThrottle = throttleOverride;
				syncVar___currentSteerAngle.SetValue(steerOverride * ActualMaxSteeringAngle,  true);
			}
			else
			{
				UpdateThrottle();
				UpdateSteerAngle();
			}
			ApplySteerAngle();
		}

		private void OnDrawGizmos()
		{
		}

		protected virtual void FixedUpdate()
		{
			float item = base.transform.InverseTransformDirection(base.transform.position - lastFramePosition).z / Time.fixedDeltaTime * 3.6f;
			previousSpeeds.Add(item);
			if (previousSpeeds.Count > previousSpeedsSampleSize)
			{
				previousSpeeds.RemoveAt(0);
			}
			if (isStatic || !localPlayerIsDriver)
			{
				float num = 0f;
				foreach (float previousSpeed in previousSpeeds)
				{
					num += previousSpeed;
				}
				float num2 = num / (float)previousSpeeds.Count;
				speed_Kmh = num2;
			}
			else
			{
				speed_Kmh = base.transform.InverseTransformDirection(rb.velocity).z * 3.6f;
			}
			lastFramePosition = base.transform.position;
			if (!isStatic)
			{
				ApplyThrottle();
				rb.AddForce(-base.transform.up * speed_Kmh * downforce);
			}
			else
			{
				if (SyncAccessor__003CbrakesApplied_003Ek__BackingField)
				{
					brakesApplied = false;
				}
				syncVar___currentSteerAngle.SetValue(0f,  true);
			}
			if ((base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost)) && base.transform.position.y < -20f)
			{
				if (rb != null)
				{
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
				}
				float y = 0f;
				if (MapHeightSampler.Sample(base.transform.position.x, out y, base.transform.position.z))
				{
					SetTransform(new Vector3(base.transform.position.x, y + 3f, base.transform.position.z), Quaternion.identity);
				}
				else
				{
					SetTransform(MapHeightSampler.ResetPosition, Quaternion.identity);
				}
			}
			if (!localPlayerIsDriver || !(Mathf.Abs(speed_Kmh) < 5f))
			{
				return;
			}
			int num3 = 0;
			for (int i = 0; i < wheels.Count; i++)
			{
				if (!wheels[i].IsWheelGrounded())
				{
					num3++;
				}
			}
			if (num3 >= 2)
			{
				rb.AddRelativeTorque(Vector3.forward * 8f * (0f - Mathf.Clamp(SyncAccessor_currentSteerAngle / ActualMaxSteeringAngle, -1f, 1f)), ForceMode.Acceleration);
			}
		}

		protected virtual void LateUpdate()
		{
			if (HumanoidColliderContainer != null)
			{
				HumanoidColliderContainer.transform.position = base.transform.position;
				HumanoidColliderContainer.transform.rotation = base.transform.rotation;
			}
		}

		[ServerRpc(RequireOwnership = false)]
		protected virtual void SetOwner(NetworkConnection conn)
		{
			RpcWriter___Server_SetOwner_328543758(conn);
		}

		[ObserversRpc]
		protected virtual void OnOwnerChanged()
		{
			RpcWriter___Observers_OnOwnerChanged_2166136261();
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetTransform_Server(Vector3 pos, Quaternion rot)
		{
			RpcWriter___Server_SetTransform_Server_3848837105(pos, rot);
			RpcLogic___SetTransform_Server_3848837105(pos, rot);
		}

		[ObserversRpc(RunLocally = true)]
		private void SetTransform(Vector3 pos, Quaternion rot)
		{
			RpcWriter___Observers_SetTransform_3848837105(pos, rot);
			RpcLogic___SetTransform_3848837105(pos, rot);
		}

		public void DestroyVehicle()
		{
			if (!InstanceFinder.IsServer)
			{
				Console.LogWarning("DestroyVehicle called on client!");
				return;
			}
			if (isOccupied)
			{
				Console.LogError("Can't destroy vehicle while occupied.", base.gameObject);
				return;
			}
			if (isParked)
			{
				ExitPark_Networked(null, moveToExitPoint: false);
			}
			if (HumanoidColliderContainer != null)
			{
				UnityEngine.Object.Destroy(HumanoidColliderContainer.gameObject);
			}
			Despawn();
		}

		protected virtual void UpdateThrottle()
		{
			currentThrottle = 0f;
		}

		protected virtual void ApplyThrottle()
		{
			bool flag = handbrakeApplied;
			handbrakeApplied = false;
			if (localPlayerIsDriver || overrideControls)
			{
				if (SyncAccessor__003CbrakesApplied_003Ek__BackingField)
				{
					brakesApplied = false;
				}
				if (SyncAccessor__003CisReversing_003Ek__BackingField)
				{
					isReversing = false;
				}
				foreach (Wheel wheel in wheels)
				{
					wheel.wheelCollider.motorTorque = 0.0001f;
					wheel.wheelCollider.brakeTorque = 0f;
				}
				if (localPlayerIsDriver)
				{
					handbrakeApplied = GameInput.GetButton(GameInput.ButtonCode.Handbrake);
				}
				if (handbrakeApplied && Mathf.Abs(speed_Kmh) > 4f)
				{
					brakesApplied = true;
					if (!flag && onHandbrakeApplied != null)
					{
						onHandbrakeApplied.Invoke();
					}
				}
				if (currentThrottle != 0f && (Mathf.Abs(speed_Kmh) < 4f || Mathf.Sign(speed_Kmh) == Mathf.Sign(currentThrottle)))
				{
					if (speed_Kmh < -0.1f && currentThrottle < 0f && !SyncAccessor__003CisReversing_003Ek__BackingField)
					{
						isReversing = true;
					}
					float num = motorTorque.Evaluate(Mathf.Abs(speed_Kmh));
					if (SyncAccessor__003CisReversing_003Ek__BackingField)
					{
						num = motorTorque.Evaluate(Mathf.Abs(speed_Kmh) / reverseMultiplier);
					}
					WheelCollider[] array = driveWheels;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].motorTorque = currentThrottle * num * diffGearing / 2f;
					}
				}
				else if (currentThrottle != 0f)
				{
					if (Mathf.Abs(currentThrottle) > 0.05f && !SyncAccessor__003CbrakesApplied_003Ek__BackingField)
					{
						brakesApplied = true;
					}
					foreach (Wheel wheel2 in wheels)
					{
						wheel2.wheelCollider.brakeTorque = Mathf.Abs(currentThrottle) * brakeForce.Evaluate(Mathf.Abs(speed_Kmh));
					}
				}
			}
			else
			{
				foreach (Wheel wheel3 in wheels)
				{
					wheel3.wheelCollider.motorTorque = 0f;
				}
				if (!isOccupied)
				{
					if (!handbrakeApplied)
					{
						handbrakeApplied = true;
					}
					if (SyncAccessor__003CisReversing_003Ek__BackingField)
					{
						isReversing = false;
					}
					if (SyncAccessor__003CbrakesApplied_003Ek__BackingField)
					{
						brakesApplied = false;
					}
				}
			}
			if (handbrakeApplied)
			{
				WheelCollider[] array = handbrakeWheels;
				foreach (WheelCollider obj in array)
				{
					obj.motorTorque = 0f;
					obj.brakeTorque = handBrakeForce;
				}
			}
		}

		public void ApplyHandbrake()
		{
			handbrakeApplied = true;
			WheelCollider[] array = handbrakeWheels;
			foreach (WheelCollider obj in array)
			{
				obj.motorTorque = 0f;
				obj.brakeTorque = handBrakeForce;
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void SetSteeringAngle(float sa)
		{
			RpcWriter___Server_SetSteeringAngle_431000436(sa);
		}

		protected virtual void UpdateSteerAngle()
		{
			if (localPlayerIsDriver)
			{
				syncVar___currentSteerAngle.SetValue(lastFrameSteerAngle,  true);
				if (Mathf.Abs(lastReplicatedSteerAngle - SyncAccessor_currentSteerAngle) > 3f)
				{
					lastReplicatedSteerAngle = SyncAccessor_currentSteerAngle;
					SetSteeringAngle(SyncAccessor_currentSteerAngle);
				}
				lastFrameSteerAngle = SyncAccessor_currentSteerAngle;
			}
		}

		protected virtual void ApplySteerAngle()
		{
			float num = SyncAccessor_currentSteerAngle;
			if (flipSteer)
			{
				num *= -1f;
			}
			WheelCollider[] array = steerWheels;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].steerAngle = num;
			}
		}

		private void DelaySetStatic(bool stat)
		{
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitForSeconds(1f);
				SetIsStatic(stat);
			}
		}

		public virtual void SetIsStatic(bool stat)
		{
			isStatic = stat;
			if (isStatic)
			{
				rb.isKinematic = true;
			}
			else
			{
				rb.isKinematic = false;
			}
			foreach (Wheel wheel in wheels)
			{
				wheel.SetIsStatic(isStatic);
			}
		}

		public void AlignTo(Transform target, EParkingAlignment type)
		{
			Tuple<Vector3, Quaternion> alignmentTransform = GetAlignmentTransform(target, type);
			base.transform.rotation = alignmentTransform.Item2;
			base.transform.position = alignmentTransform.Item1;
		}

		public Tuple<Vector3, Quaternion> GetAlignmentTransform(Transform target, EParkingAlignment type)
		{
			Quaternion rotation = target.rotation;
			if (type == EParkingAlignment.FrontToKerb)
			{
				rotation *= Quaternion.Euler(0f, 180f, 0f);
			}
			Vector3 item = target.position + target.up * (boundingBoxDimensions.y / 2f - boundingBox.transform.localPosition.y);
			if (type == EParkingAlignment.FrontToKerb)
			{
				item += target.forward * (boundingBoxDimensions.z / 2f - boundingBox.transform.localPosition.y);
			}
			else
			{
				item += target.forward * (boundingBoxDimensions.z / 2f - boundingBox.transform.localPosition.y);
			}
			return new Tuple<Vector3, Quaternion>(item, rotation);
		}

		public float GetVehicleValue()
		{
			return VehiclePrice;
		}

		public void OverrideMaxSteerAngle(float maxAngle)
		{
			OverriddenMaxSteerAngle = maxAngle;
			MaxSteerAngleOverridden = true;
		}

		public void ResetMaxSteerAngle()
		{
			MaxSteerAngleOverridden = false;
		}

		public void SetObstaclesActive(bool active)
		{
			NavmeshCut.enabled = active;
			NavMeshObstacle.carving = active;
		}

		public VehicleSeat GetFirstFreeSeat()
		{
			for (int i = 0; i < Seats.Length; i++)
			{
				if (!Seats[i].isOccupied)
				{
					return Seats[i];
				}
			}
			return null;
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void SetSeatOccupant(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetSeatOccupant_3428404692(conn, seatIndex, occupant);
				RpcLogic___SetSeatOccupant_3428404692(conn, seatIndex, occupant);
			}
			else
			{
				RpcWriter___Target_SetSeatOccupant_3428404692(conn, seatIndex, occupant);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SetSeatOccupant_Server(int seatIndex, NetworkConnection conn)
		{
			RpcWriter___Server_SetSeatOccupant_Server_3266232555(seatIndex, conn);
			RpcLogic___SetSeatOccupant_Server_3266232555(seatIndex, conn);
		}

		private void Hovered()
		{
			if (!IsPlayerOwned)
			{
				intObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
			else if (CurrentPlayerOccupancy < Capacity)
			{
				intObj.SetMessage("Enter vehicle");
				intObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				intObj.SetMessage("Vehicle full");
				intObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
			}
		}

		private void Interacted()
		{
			if (!justExitedVehicle && IsPlayerOwned && CurrentPlayerOccupancy < Capacity)
			{
				EnterVehicle();
			}
		}

		private void EnterVehicle()
		{
			_ = justExitedVehicle;
		}

		public void ExitVehicle()
		{
			if (localPlayerIsDriver)
			{
				SetOwner(null);
			}
			localPlayerIsInVehicle = false;
			localPlayerIsDriver = false;
			if (localPlayerSeat != null)
			{
				SetSeatOccupant_Server(Array.IndexOf(Seats, localPlayerSeat), null);
				localPlayerSeat = null;
			}
			List<Transform> list = new List<Transform>();
			list.Add(closestExitPoint);
			list.AddRange(exitPoints);
			Transform validExitPoint = GetValidExitPoint(list);
			Player.Local.ExitVehicle(validExitPoint);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
			PlayerSingleton<PlayerCamera>.Instance.ResetRotation();
			PlayerSingleton<PlayerCamera>.Instance.SetCameraMode(PlayerCamera.ECameraMode.Default);
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			SetObstaclesActive(!isOccupied);
			justExitedVehicle = true;
			Invoke("EndJustExited", 0.05f);
		}

		private void EndJustExited()
		{
			justExitedVehicle = false;
		}

		public Transform GetExitPoint(int seatIndex = 0)
		{
			return exitPoints[seatIndex];
		}

		private Transform GetClosestExitPoint(Vector3 pos)
		{
			Transform transform = null;
			for (int i = 0; i < exitPoints.Count; i++)
			{
				if (transform == null || Vector3.Distance(exitPoints[i].position, pos) < Vector3.Distance(transform.transform.position, pos))
				{
					transform = exitPoints[i];
				}
			}
			return transform;
		}

		private Transform GetValidExitPoint(List<Transform> possibleExitPoints)
		{
			LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Default"));
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Vehicle"));
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Terrain"));
			for (int i = 0; i < possibleExitPoints.Count; i++)
			{
				if (Physics.OverlapSphere(possibleExitPoints[i].position, 0.35f, layerMask).Length == 0)
				{
					return possibleExitPoints[i];
				}
			}
			Console.LogWarning("Unable to find clear exit point for vehicle. Using first exit point.");
			return possibleExitPoints[0];
		}

		public void AddNPCOccupant(NPC npc)
		{
			int num = OccupantNPCs.Where((NPC x) => x != null).Count();
			if (!OccupantNPCs.Contains(npc))
			{
				for (int i = 0; i < OccupantNPCs.Length; i++)
				{
					if (OccupantNPCs[i] == null)
					{
						OccupantNPCs[i] = npc;
						break;
					}
				}
			}
			isOccupied = true;
			SetObstaclesActive(!isOccupied);
			if (num == 0 && onVehicleStart != null)
			{
				onVehicleStart.Invoke();
			}
		}

		public void RemoveNPCOccupant(NPC npc)
		{
			for (int i = 0; i < OccupantNPCs.Length; i++)
			{
				if (OccupantNPCs[i] == npc)
				{
					OccupantNPCs[i] = null;
				}
			}
			if (OccupantNPCs.Where((NPC x) => x != null).Count() == 0)
			{
				isOccupied = false;
				if (onVehicleStop != null)
				{
					onVehicleStop.Invoke();
				}
			}
			SetObstaclesActive(!isOccupied);
		}

		public virtual bool CanBeRecovered()
		{
			if (IsPlayerOwned && !isOccupied)
			{
				return !isStatic;
			}
			return false;
		}

		public virtual void RecoverVehicle()
		{
			VehicleRecoveryPoint closestRecoveryPoint = VehicleRecoveryPoint.GetClosestRecoveryPoint(base.transform.position);
			base.transform.position = closestRecoveryPoint.transform.position + Vector3.up * 2f;
			base.transform.up = Vector3.up;
		}

		public virtual void SetColor(EVehicleColor col)
		{
			color = col;
			ApplyColor(appliedColor);
		}

		public virtual void OverrideColor(EVehicleColor col)
		{
			overrideColor = col;
			colorOverridden = true;
			ApplyColor(overrideColor);
		}

		protected virtual void ApplyColor(EVehicleColor col)
		{
			if (col == EVehicleColor.Custom)
			{
				appliedColor = col;
				return;
			}
			appliedColor = col;
			Material material = Singleton<VehicleColors>.Instance.colorLibrary.Find((VehicleColors.VehicleColorData x) => x.color == appliedColor).material;
			for (int i = 0; i < BodyMeshes.Length; i++)
			{
				BodyMeshes[i].Renderer.materials[BodyMeshes[i].MaterialIndex].color = material.color;
			}
		}

		public virtual void StopColorOverride()
		{
			colorOverridden = false;
			ApplyColor(appliedColor);
		}

		public void ShowOutline(BuildableItem.EOutlineColor color)
		{
			if (outlineEffect == null)
			{
				outlineEffect = base.gameObject.AddComponent<Outlinable>();
				outlineEffect.OutlineParameters.BlurShift = 0f;
				outlineEffect.OutlineParameters.DilateShift = 0.5f;
				outlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
				foreach (GameObject outlineRenderer in outlineRenderers)
				{
					MeshRenderer[] componentsInChildren = outlineRenderer.GetComponentsInChildren<MeshRenderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						OutlineTarget target = new OutlineTarget(componentsInChildren[i]);
						outlineEffect.TryAddTarget(target);
					}
				}
			}
			outlineEffect.OutlineParameters.Color = BuildableItem.GetColorFromOutlineColorEnum(color);
			Color32 colorFromOutlineColorEnum = BuildableItem.GetColorFromOutlineColorEnum(color);
			colorFromOutlineColorEnum.a = 9;
			outlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", colorFromOutlineColorEnum);
			outlineEffect.enabled = true;
		}

		public void HideOutline()
		{
			if (outlineEffect != null)
			{
				outlineEffect.enabled = false;
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void Park_Networked(NetworkConnection conn, ParkData parkData)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Park_Networked_2633993806(conn, parkData);
				RpcLogic___Park_Networked_2633993806(conn, parkData);
			}
			else
			{
				RpcWriter___Target_Park_Networked_2633993806(conn, parkData);
			}
		}

		public void Park(NetworkConnection conn, ParkData parkData, bool network)
		{
			if (isParked)
			{
				ExitPark();
			}
			if (network)
			{
				Park_Networked(conn, parkData);
				return;
			}
			CurrentParkingLot = GUIDManager.GetObject<ParkingLot>(parkData.lotGUID);
			if (CurrentParkingLot == null)
			{
				Console.LogWarning("LandVehicle.Park: parking lot not found with the given GUID.");
				return;
			}
			CurrentParkData = parkData;
			if (parkData.spotIndex < 0 || parkData.spotIndex >= CurrentParkingLot.ParkingSpots.Count)
			{
				Console.Log("Hiding");
				SetVisible(vis: false);
			}
			else
			{
				CurrentParkingSpot = CurrentParkingLot.ParkingSpots[parkData.spotIndex];
				CurrentParkingSpot.SetOccupant(this);
				AlignTo(CurrentParkingSpot.AlignmentPoint, parkData.alignment);
			}
			SetIsStatic(stat: true);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void ExitPark_Networked(NetworkConnection conn, bool moveToExitPoint = true)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ExitPark_Networked_214505783(conn, moveToExitPoint);
				RpcLogic___ExitPark_Networked_214505783(conn, moveToExitPoint);
			}
			else
			{
				RpcWriter___Target_ExitPark_Networked_214505783(conn, moveToExitPoint);
			}
		}

		public void ExitPark(bool moveToExitPoint = true)
		{
			if (!(CurrentParkingLot == null))
			{
				if (CurrentParkingLot.ExitPoint != null && moveToExitPoint)
				{
					AlignTo(CurrentParkingLot.ExitPoint, CurrentParkingLot.ExitAlignment);
				}
				CurrentParkData = null;
				CurrentParkingLot = null;
				if (CurrentParkingSpot != null)
				{
					CurrentParkingSpot.SetOccupant(null);
					CurrentParkingSpot = null;
				}
				SetIsStatic(stat: false);
				SetVisible(vis: true);
				base.gameObject.SetActive(value: true);
			}
		}

		public void SetVisible(bool vis)
		{
			IsVisible = vis;
			vehicleModel.gameObject.SetActive(vis);
			HumanoidColliderContainer.gameObject.SetActive(vis);
		}

		public List<ItemInstance> GetContents()
		{
			List<ItemInstance> list = new List<ItemInstance>();
			if (Storage != null)
			{
				list.AddRange(Storage.GetAllItems());
			}
			return list;
		}

		public virtual string GetSaveString()
		{
			return new VehicleData(GUID, vehicleCode, base.transform.position, base.transform.rotation, color).GetJson();
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> result = new List<string>();
			if (Storage != null && Storage.ItemCount > 0)
			{
				string jSON = new ItemSet(Storage.ItemSlots).GetJSON();
				((ISaveable)this).WriteSubfile(parentFolderPath, "Contents", jSON);
			}
			return result;
		}

		public virtual void Load(VehicleData data, string containerPath)
		{
			SetGUID(new Guid(data.GUID));
			SetTransform(data.Position, data.Rotation);
			SetColor(Enum.Parse<EVehicleColor>(data.Color));
			if (Storage != null && File.Exists(System.IO.Path.Combine(containerPath, "Contents.json")) && Loader.TryLoadFile(containerPath, "Contents", out var contents))
			{
				ItemInstance[] array = ItemSet.Deserialize(contents);
				for (int i = 0; i < array.Length; i++)
				{
					Storage.ItemSlots[i].SetStoredItem(array[i]);
				}
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CisReversing_003Ek__BackingField = new SyncVar<bool>(this, 2u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, 0.1f, Channel.Unreliable, isReversing);
				syncVar____003CbrakesApplied_003Ek__BackingField = new SyncVar<bool>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, 0.1f, Channel.Unreliable, brakesApplied);
				syncVar___currentSteerAngle = new SyncVar<float>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, 0.05f, Channel.Unreliable, currentSteerAngle);
				RegisterObserversRpc(0u, RpcReader___Observers_SetIsPlayerOwned_214505783);
				RegisterTargetRpc(1u, RpcReader___Target_SetIsPlayerOwned_214505783);
				RegisterServerRpc(2u, RpcReader___Server_SetOwner_328543758);
				RegisterObserversRpc(3u, RpcReader___Observers_OnOwnerChanged_2166136261);
				RegisterServerRpc(4u, RpcReader___Server_SetTransform_Server_3848837105);
				RegisterObserversRpc(5u, RpcReader___Observers_SetTransform_3848837105);
				RegisterServerRpc(6u, RpcReader___Server_SetSteeringAngle_431000436);
				RegisterObserversRpc(7u, RpcReader___Observers_SetSeatOccupant_3428404692);
				RegisterTargetRpc(8u, RpcReader___Target_SetSeatOccupant_3428404692);
				RegisterServerRpc(9u, RpcReader___Server_SetSeatOccupant_Server_3266232555);
				RegisterObserversRpc(10u, RpcReader___Observers_Park_Networked_2633993806);
				RegisterTargetRpc(11u, RpcReader___Target_Park_Networked_2633993806);
				RegisterObserversRpc(12u, RpcReader___Observers_ExitPark_Networked_214505783);
				RegisterTargetRpc(13u, RpcReader___Target_ExitPark_Networked_214505783);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EVehicles_002ELandVehicle);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVehicles_002ELandVehicleAssembly_002DCSharp_002Edll_Excuted = true;
				syncVar____003CisReversing_003Ek__BackingField.SetRegistered();
				syncVar____003CbrakesApplied_003Ek__BackingField.SetRegistered();
				syncVar___currentSteerAngle.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_SetIsPlayerOwned_214505783(NetworkConnection conn, bool playerOwned)
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
				writer.WriteBoolean(playerOwned);
				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetIsPlayerOwned_214505783(NetworkConnection conn, bool playerOwned)
		{
			IsPlayerOwned = playerOwned;
			if (GetComponent<StorageEntity>() != null)
			{
				GetComponent<StorageEntity>().AccessSettings = (playerOwned ? StorageEntity.EAccessSettings.Full : StorageEntity.EAccessSettings.Closed);
			}
		}

		private void RpcReader___Observers_SetIsPlayerOwned_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool playerOwned = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetIsPlayerOwned_214505783(null, playerOwned);
			}
		}

		private void RpcWriter___Target_SetIsPlayerOwned_214505783(NetworkConnection conn, bool playerOwned)
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
				writer.WriteBoolean(playerOwned);
				SendTargetRpc(1u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetIsPlayerOwned_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool playerOwned = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetIsPlayerOwned_214505783(base.LocalConnection, playerOwned);
			}
		}

		private void RpcWriter___Server_SetOwner_328543758(NetworkConnection conn)
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
				writer.WriteNetworkConnection(conn);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___SetOwner_328543758(NetworkConnection conn)
		{
			base.NetworkObject.GiveOwnership(conn);
			OnOwnerChanged();
		}

		private void RpcReader___Server_SetOwner_328543758(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			if (base.IsServerInitialized)
			{
				RpcLogic___SetOwner_328543758(conn2);
			}
		}

		private void RpcWriter___Observers_OnOwnerChanged_2166136261()
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
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___OnOwnerChanged_2166136261()
		{
			if (base.NetworkObject.Owner == base.LocalConnection || (base.NetworkObject.OwnerId == -1 && InstanceFinder.IsHost))
			{
				Console.Log("Local client owns vehicle");
				rb.isKinematic = false;
				rb.interpolation = RigidbodyInterpolation.Interpolate;
				GetComponent<NetworkTransform>().ClearReplicateCache();
				GetComponent<NetworkTransform>().ForceSend();
				return;
			}
			Console.Log("Local client no longer owns vehicle");
			if (!InstanceFinder.IsHost || (InstanceFinder.IsHost && !localPlayerIsDriver && CurrentPlayerOccupancy > 0))
			{
				rb.interpolation = RigidbodyInterpolation.None;
				Debug.Log("No interpolation");
				rb.isKinematic = false;
				rb.isKinematic = true;
			}
		}

		private void RpcReader___Observers_OnOwnerChanged_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___OnOwnerChanged_2166136261();
			}
		}

		private void RpcWriter___Server_SetTransform_Server_3848837105(Vector3 pos, Quaternion rot)
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
				writer.WriteVector3(pos);
				writer.WriteQuaternion(rot);
				SendServerRpc(4u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetTransform_Server_3848837105(Vector3 pos, Quaternion rot)
		{
			SetTransform(pos, rot);
		}

		private void RpcReader___Server_SetTransform_Server_3848837105(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			Vector3 pos = PooledReader0.ReadVector3();
			Quaternion rot = PooledReader0.ReadQuaternion();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetTransform_Server_3848837105(pos, rot);
			}
		}

		private void RpcWriter___Observers_SetTransform_3848837105(Vector3 pos, Quaternion rot)
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
				writer.WriteVector3(pos);
				writer.WriteQuaternion(rot);
				SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetTransform_3848837105(Vector3 pos, Quaternion rot)
		{
			base.transform.position = pos;
			base.transform.rotation = rot;
		}

		private void RpcReader___Observers_SetTransform_3848837105(PooledReader PooledReader0, Channel channel)
		{
			Vector3 pos = PooledReader0.ReadVector3();
			Quaternion rot = PooledReader0.ReadQuaternion();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetTransform_3848837105(pos, rot);
			}
		}

		private void RpcWriter___Server_SetSteeringAngle_431000436(float sa)
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
				writer.WriteSingle(sa);
				SendServerRpc(6u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SetSteeringAngle_431000436(float sa)
		{
			syncVar___currentSteerAngle.SetValue(sa,  true);
		}

		private void RpcReader___Server_SetSteeringAngle_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float sa = PooledReader0.ReadSingle();
			if (base.IsServerInitialized)
			{
				RpcLogic___SetSteeringAngle_431000436(sa);
			}
		}

		private void RpcWriter___Observers_SetSeatOccupant_3428404692(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
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
				writer.WriteInt32(seatIndex);
				writer.WriteNetworkConnection(occupant);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetSeatOccupant_3428404692(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
		{
			Player occupant2 = Seats[seatIndex].Occupant;
			Seats[seatIndex].Occupant = Player.GetPlayer(occupant);
			_ = occupant != null;
			if (seatIndex == 0)
			{
				if (occupant != null)
				{
					if (onVehicleStart != null)
					{
						onVehicleStart.Invoke();
					}
				}
				else if (onVehicleStop != null)
				{
					onVehicleStop.Invoke();
				}
			}
			if (occupant != null)
			{
				if (onPlayerEnterVehicle != null)
				{
					onPlayerEnterVehicle(Seats[seatIndex].Occupant);
				}
			}
			else if (onPlayerExitVehicle != null)
			{
				onPlayerExitVehicle(occupant2);
			}
			isOccupied = Seats.Count((VehicleSeat s) => s.isOccupied) > 0;
		}

		private void RpcReader___Observers_SetSeatOccupant_3428404692(PooledReader PooledReader0, Channel channel)
		{
			int seatIndex = PooledReader0.ReadInt32();
			NetworkConnection occupant = PooledReader0.ReadNetworkConnection();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetSeatOccupant_3428404692(null, seatIndex, occupant);
			}
		}

		private void RpcWriter___Target_SetSeatOccupant_3428404692(NetworkConnection conn, int seatIndex, NetworkConnection occupant)
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
				writer.WriteInt32(seatIndex);
				writer.WriteNetworkConnection(occupant);
				SendTargetRpc(8u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetSeatOccupant_3428404692(PooledReader PooledReader0, Channel channel)
		{
			int seatIndex = PooledReader0.ReadInt32();
			NetworkConnection occupant = PooledReader0.ReadNetworkConnection();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetSeatOccupant_3428404692(base.LocalConnection, seatIndex, occupant);
			}
		}

		private void RpcWriter___Server_SetSeatOccupant_Server_3266232555(int seatIndex, NetworkConnection conn)
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
				writer.WriteInt32(seatIndex);
				writer.WriteNetworkConnection(conn);
				SendServerRpc(9u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SetSeatOccupant_Server_3266232555(int seatIndex, NetworkConnection conn)
		{
			SetSeatOccupant(null, seatIndex, conn);
		}

		private void RpcReader___Server_SetSeatOccupant_Server_3266232555(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int seatIndex = PooledReader0.ReadInt32();
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetSeatOccupant_Server_3266232555(seatIndex, conn2);
			}
		}

		private void RpcWriter___Observers_Park_Networked_2633993806(NetworkConnection conn, ParkData parkData)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated(writer, parkData);
				SendObserversRpc(10u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___Park_Networked_2633993806(NetworkConnection conn, ParkData parkData)
		{
			Park(conn, parkData, network: false);
		}

		private void RpcReader___Observers_Park_Networked_2633993806(PooledReader PooledReader0, Channel channel)
		{
			ParkData parkData = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Park_Networked_2633993806(null, parkData);
			}
		}

		private void RpcWriter___Target_Park_Networked_2633993806(NetworkConnection conn, ParkData parkData)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated(writer, parkData);
				SendTargetRpc(11u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Park_Networked_2633993806(PooledReader PooledReader0, Channel channel)
		{
			ParkData parkData = GeneratedReaders___Internal.Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___Park_Networked_2633993806(base.LocalConnection, parkData);
			}
		}

		private void RpcWriter___Observers_ExitPark_Networked_214505783(NetworkConnection conn, bool moveToExitPoint = true)
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
				writer.WriteBoolean(moveToExitPoint);
				SendObserversRpc(12u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ExitPark_Networked_214505783(NetworkConnection conn, bool moveToExitPoint = true)
		{
			ExitPark(moveToExitPoint);
		}

		private void RpcReader___Observers_ExitPark_Networked_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool moveToExitPoint = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ExitPark_Networked_214505783(null, moveToExitPoint);
			}
		}

		private void RpcWriter___Target_ExitPark_Networked_214505783(NetworkConnection conn, bool moveToExitPoint = true)
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
				writer.WriteBoolean(moveToExitPoint);
				SendTargetRpc(13u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ExitPark_Networked_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool moveToExitPoint = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___ExitPark_Networked_214505783(base.LocalConnection, moveToExitPoint);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EVehicles_002ELandVehicle(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CisReversing_003Ek__BackingField.SetValue(syncVar____003CisReversing_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				bool value3 = PooledReader0.ReadBoolean();
				syncVar____003CisReversing_003Ek__BackingField.SetValue(value3, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CbrakesApplied_003Ek__BackingField.SetValue(syncVar____003CbrakesApplied_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				bool value2 = PooledReader0.ReadBoolean();
				syncVar____003CbrakesApplied_003Ek__BackingField.SetValue(value2, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar___currentSteerAngle.SetValue(syncVar___currentSteerAngle.GetValue(calledByUser: true),  true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
				syncVar___currentSteerAngle.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EVehicles_002ELandVehicle_Assembly_002DCSharp_002Edll()
		{
			OccupantNPCs = new NPC[Seats.Length];
			boundingBox.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			for (int i = 0; i < driveWheels.Length; i++)
			{
				wheels.Add(driveWheels[i].GetComponent<Wheel>());
			}
			for (int j = 0; j < steerWheels.Length; j++)
			{
				if (!wheels.Contains(steerWheels[j].GetComponent<Wheel>()))
				{
					wheels.Add(steerWheels[j].GetComponent<Wheel>());
				}
			}
			InitializeSaveable();
			if (GetComponent<StorageEntity>() != null)
			{
				GetComponent<StorageEntity>().AccessSettings = StorageEntity.EAccessSettings.Closed;
			}
			SetObstaclesActive(active: true);
		}
	}
}
