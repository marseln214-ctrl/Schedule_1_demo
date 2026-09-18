using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Lighting;
using ScheduleOne.Management;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.UI.Management;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.ObjectScripts
{
	public class Pot : GridItem, IUsable, IConfigurable, ITransitEntity
	{
		public enum ECameraPosition
		{
			Closeup = 0,
			Midshot = 1,
			Fullshot = 2,
			BirdsEye = 3
		}

		public enum ESoilState
		{
			Flat = 0,
			Parted = 1,
			Packed = 2
		}

		public const float DryThreshold = 0f;

		public const float WaterloggedThreshold = 1f;

		public const float ROTATION_SPEED = 10f;

		public const float MAX_CAMERA_DISTANCE = 2.75f;

		public const float MIN_CAMERA_DISTANCE = 0.5f;

		[Header("References")]
		public Transform ModelTransform;

		public InteractableObject IntObj;

		public Transform PourableStartPoint;

		public Transform SeedStartPoint;

		public Transform SeedRestingPoint;

		public GameObject WaterLoggedVisuals;

		public Transform LookAtPoint;

		public Transform AdditivesContainer;

		public Transform PlantContainer;

		public Transform IntObjLabel_Low;

		public Transform IntObjLabel_High;

		public Transform uiPoint;

		[SerializeField]
		protected ConfigurationReplicator configReplicator;

		public Transform[] accessPoints;

		public Transform TaskBounds;

		public PotSoilCover SoilCover;

		public Transform LeafDropPoint;

		public ParticleSystem PoofParticles;

		public AudioSourceController PoofSound;

		[Header("UI")]
		public Transform WaterCanvasContainer;

		public Canvas WaterLevelCanvas;

		public CanvasGroup WaterLevelCanvasGroup;

		public Slider WaterLevelSlider;

		public GameObject NoWaterIcon;

		public PotUIElement WorldspaceUIPrefab;

		public Sprite typeIcon;

		[Header("Camera References")]
		public Transform CameraContainer;

		public Transform MidshotPosition;

		public Transform CloseupPosition;

		public Transform FullshotPosition;

		public Transform BirdsEyePosition;

		public bool AutoRotateCameraContainer = true;

		[Header("Dirt references")]
		public Transform Dirt_Flat;

		public Transform Dirt_Parted;

		public SoilChunk[] SoilChunks;

		public List<MeshRenderer> DirtRenderers = new List<MeshRenderer>();

		[Header("Pot Settings")]
		public float PotRadius = 0.2f;

		[Range(0.2f, 2f)]
		public float YieldMultiplier = 1f;

		[Range(0.2f, 2f)]
		public float GrowSpeedMultiplier = 1f;

		[Range(0.2f, 2f)]
		public float MoistureDrainMultiplier = 1f;

		public bool AlignLeafDropToPlayer = true;

		[Header("Capacity Settings")]
		public float SoilCapacity = 20f;

		public float WaterCapacity = 5f;

		public float WaterDrainPerHour = 2f;

		[Header("Dirt Settings")]
		[SerializeField]
		protected Vector3 DirtMinScale;

		[SerializeField]
		protected Vector3 DirtMaxScale = Vector3.one;

		[Header("Pour Target")]
		public Transform Target;

		[Header("Lighting")]
		public UsableLightSource LightSourceOverride;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized, OnChange = "SoilLevelChanged")]
		public float _003CSoilLevel_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public string _003CSoilID_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public int _003CRemainingSoilUses_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized, OnChange = "WaterLevelChanged")]
		public float _003CWaterLevel_003Ek__BackingField;

		public List<Additive> AppliedAdditives = new List<Additive>();

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CNPCUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool intObjSetThisFrame;

		private ItemSlot outputSlot;

		private float rotation;

		private bool rotationOverridden;

		public SyncVar<float> syncVar____003CSoilLevel_003Ek__BackingField;

		public SyncVar<string> syncVar____003CSoilID_003Ek__BackingField;

		public SyncVar<int> syncVar____003CRemainingSoilUses_003Ek__BackingField;

		public SyncVar<float> syncVar____003CWaterLevel_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted;

		public float SoilLevel
		{
			[CompilerGenerated]
			get
			{
				return _003CSoilLevel_003Ek__BackingField;
			}
			[CompilerGenerated]
			protected set
			{
				_003CSoilLevel_003Ek__BackingField = value;

                syncVar____003CSoilLevel_003Ek__BackingField.SetValue(value,  true);
			}
		}

        private string _soilID = string.Empty;

        public string SoilID
		{
			get
			{
				return _soilID;
			}
			[CompilerGenerated]
			protected set
			{
                _soilID = value;
			}
		}


		public int RemainingSoilUses
		{
			[CompilerGenerated]
			get
			{
				return _003CRemainingSoilUses_003Ek__BackingField;
			}
			[CompilerGenerated]
			protected set
			{
				_003CRemainingSoilUses_003Ek__BackingField = value;

                syncVar____003CRemainingSoilUses_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public float WaterLevel
		{
			[CompilerGenerated]
			get
			{
				return _003CWaterLevel_003Ek__BackingField;
			}
			[CompilerGenerated]
			protected set
			{
				_003CWaterLevel_003Ek__BackingField = value;

                syncVar____003CWaterLevel_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public float NormalizedWaterLevel => SyncAccessor__003CWaterLevel_003Ek__BackingField / WaterCapacity;

		public bool IsFilledWithSoil => SyncAccessor__003CSoilLevel_003Ek__BackingField >= SoilCapacity;

		public Plant Plant { get; protected set; }

		public NetworkObject NPCUserObject
		{
			[CompilerGenerated]
			get
			{
				return _003CNPCUserObject_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CNPCUserObject_003Ek__BackingField = value;

                syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public NetworkObject PlayerUserObject
		{
			[CompilerGenerated]
			get
			{
				return _003CPlayerUserObject_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CPlayerUserObject_003Ek__BackingField = value;

                syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public EntityConfiguration Configuration => potConfiguration;

		protected PotConfiguration potConfiguration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.Pot;

		public WorldspaceUIElement WorldspaceUI { get; set; }

		public NetworkObject CurrentPlayerConfigurer
		{
			[CompilerGenerated]
			get
			{
				return _003CCurrentPlayerConfigurer_003Ek__BackingField;
			}
			[CompilerGenerated]
			set
			{
				_003CCurrentPlayerConfigurer_003Ek__BackingField = value;

                syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value,  true);
			}
		}

		public Sprite TypeIcon => typeIcon;

		public Transform Transform => base.transform;

		public Transform UIPoint => uiPoint;

		public bool CanBeSelected => true;

		public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();


		public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();


		public Vector3 LinkOriginPoint => UIPoint.position;

		public Transform[] AccessPoints => accessPoints;

		public bool Selectable { get; }

		public bool IsAcceptingItems { get; set; } = true;


		public float SyncAccessor__003CSoilLevel_003Ek__BackingField
		{
			get
			{
				return SoilLevel;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					SoilLevel = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CSoilLevel_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public string SyncAccessor__003CSoilID_003Ek__BackingField
		{
			get
			{
				return SoilID;
			}
			set
			{
				if (value != null || !base.IsServerInitialized)
				{
					SoilID = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CSoilID_003Ek__BackingField.SetValue(value, value != null);
				}
			}
		}

		public int SyncAccessor__003CRemainingSoilUses_003Ek__BackingField
		{
			get
			{
				return RemainingSoilUses;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					RemainingSoilUses = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CRemainingSoilUses_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public float SyncAccessor__003CWaterLevel_003Ek__BackingField
		{
			get
			{
				return WaterLevel;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					WaterLevel = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CWaterLevel_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public NetworkObject SyncAccessor__003CNPCUserObject_003Ek__BackingField
		{
			get
			{
				return NPCUserObject;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					NPCUserObject = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public NetworkObject SyncAccessor__003CPlayerUserObject_003Ek__BackingField
		{
			get
			{
				return PlayerUserObject;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					PlayerUserObject = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		public NetworkObject SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField
		{
			get
			{
				return CurrentPlayerConfigurer;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					CurrentPlayerConfigurer = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, value);
				}
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetConfigurer(NetworkObject player)
		{
			RpcWriter___Server_SetConfigurer_3323014238(player);
			RpcLogic___SetConfigurer_3323014238(player);
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002EPot_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			WaterLoggedVisuals.gameObject.SetActive(value: false);
			SetSoilState(ESoilState.Flat);
			UpdateSoilScale();
			UpdateSoilMaterial();
			WaterLevelSlider.value = SyncAccessor__003CWaterLevel_003Ek__BackingField / WaterCapacity;
			NoWaterIcon.gameObject.SetActive(SyncAccessor__003CWaterLevel_003Ek__BackingField <= 0f);
			WaterLevelCanvas.gameObject.SetActive(value: false);
			TaskBounds.gameObject.SetActive(value: false);
			SoilChunk[] soilChunks = SoilChunks;
			for (int i = 0; i < soilChunks.Length; i++)
			{
				soilChunks[i].ClickableEnabled = false;
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			foreach (Additive appliedAdditive in AppliedAdditives)
			{
				ApplyAdditive(connection, appliedAdditive.AssetPath, initial: false);
			}
			if (Plant != null)
			{
				PlantSeed(connection, Plant.SeedDefinition.ID, Plant.NormalizedGrowthProgress, Plant.YieldLevel, Plant.QualityLevel);
				for (int i = 0; i < Plant.ActiveHarvestables.Count; i++)
				{
					SetHarvestableActive(connection, Plant.ActiveHarvestables[i], active: true);
				}
			}
			SendConfigurationToClient(connection);
		}

		public void SendConfigurationToClient(NetworkConnection conn)
		{
			if (!conn.IsHost)
			{
				Singleton<CoroutineService>.Instance.StartCoroutine(WaitForConfig());
			}
			IEnumerator WaitForConfig()
			{
				yield return new WaitUntil(() => Configuration != null);
				Configuration.ReplicateAllFields(conn);
			}
		}

		public override void InitializeGridItem(ItemInstance instance, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, string GUID)
		{
			bool initialized = base.Initialized;
			base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
			if (!initialized && !isGhost)
			{
				TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(OnMinPass));
				TimeManager instance3 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance3.onTimeSkip = (Action<int>)Delegate.Combine(instance3.onTimeSkip, new Action<int>(TimeSkipped));
				base.ParentProperty.Configurables.Add(this);
				potConfiguration = new PotConfiguration(configReplicator, this, this);
				CreateWorldspaceUI();
				outputSlot = new ItemSlot();
				OutputSlots.Add(outputSlot);
			}
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(OnMinPass));
			TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance2.onTimeSkip = (Action<int>)Delegate.Remove(instance2.onTimeSkip, new Action<int>(TimeSkipped));
			if (Plant != null)
			{
				Plant.Destroy();
			}
			Configuration.Destroy();
			DestroyWorldspaceUI();
			base.ParentProperty.Configurables.Remove(this);
			base.DestroyItem(callOnServer);
		}

		protected virtual void LateUpdate()
		{
			if (!intObjSetThisFrame)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				if (Plant != null && (!Singleton<ManagementClipboard>.InstanceExists || !Singleton<ManagementClipboard>.Instance.IsEquipped))
				{
					if (Plant.IsFullyGrown)
					{
						IntObj.SetMessage("Use trimmers to harvest");
					}
					else
					{
						IntObj.SetMessage(Mathf.FloorToInt(Plant.NormalizedGrowthProgress * 100f) + "% grown");
					}
					IntObj.SetInteractableState(InteractableObject.EInteractableState.Label);
				}
			}
			intObjSetThisFrame = false;
			if (rotationOverridden)
			{
				ModelTransform.localRotation = Quaternion.Lerp(ModelTransform.localRotation, Quaternion.Euler(0f, rotation, 0f), Time.deltaTime * 10f);
			}
			else if (Mathf.Abs(ModelTransform.localEulerAngles.y) > 0.1f)
			{
				ModelTransform.localRotation = Quaternion.Lerp(ModelTransform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * 10f);
			}
			UpdateCanvas();
			rotationOverridden = false;
		}

		protected void UpdateCanvas()
		{
			if (Player.Local == null)
			{
				return;
			}
			if (Player.Local.CurrentProperty != base.ParentProperty)
			{
				WaterLevelCanvas.gameObject.SetActive(value: false);
				return;
			}
			if (!IsFilledWithSoil)
			{
				WaterLevelCanvas.gameObject.SetActive(value: false);
				return;
			}
			float num = Vector3.Distance(WaterLevelCanvas.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
			if (num > 2.75f)
			{
				WaterLevelCanvas.gameObject.SetActive(value: false);
				return;
			}
			Vector3 normalized = Vector3.ProjectOnPlane(PlayerSingleton<PlayerCamera>.Instance.transform.position - WaterCanvasContainer.position, Vector3.up).normalized;
			WaterCanvasContainer.forward = normalized;
			WaterLevelCanvas.transform.rotation = Quaternion.LookRotation((PlayerSingleton<PlayerCamera>.Instance.transform.position - WaterLevelCanvas.transform.position).normalized, PlayerSingleton<PlayerCamera>.Instance.transform.up);
			float num2 = 0.5f;
			float a = 1f - Mathf.Clamp01(Mathf.InverseLerp(2.75f - num2, 2.75f, num));
			float b = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 0.75f, num));
			WaterLevelCanvasGroup.alpha = Mathf.Min(a, b);
			WaterLevelCanvas.gameObject.SetActive(value: true);
		}

		private void OnMinPass()
		{
			float num = WaterDrainPerHour * WaterCapacity / 60f * MoistureDrainMultiplier;
			WaterLevel = Mathf.Clamp(SyncAccessor__003CWaterLevel_003Ek__BackingField - num, 0f, WaterCapacity);
			UpdateSoilMaterial();
			if (Plant != null)
			{
				Plant.MinPass();
			}
		}

		private void TimeSkipped(int minsSkippped)
		{
			if (InstanceFinder.IsServer)
			{
				for (int i = 0; i < minsSkippped; i++)
				{
					OnMinPass();
				}
				if (Plant != null)
				{
					SetGrowProgress(Plant.NormalizedGrowthProgress);
				}
			}
		}

		public void ConfigureInteraction(string message, InteractableObject.EInteractableState state, bool useHighLabelPos = false)
		{
			intObjSetThisFrame = true;
			IntObj.SetMessage(message);
			IntObj.SetInteractableState(state);
			IntObj.displayLocationPoint = (useHighLabelPos ? IntObjLabel_High : IntObjLabel_Low);
		}

		public void PositionCameraContainer()
		{
			if (AutoRotateCameraContainer)
			{
				Vector3 vector = CameraContainer.parent.TransformPoint(new Vector3(0f, 0.75f, 0f));
				Vector3 vector2 = PlayerSingleton<PlayerCamera>.Instance.transform.position - vector;
				vector2.y = 0f;
				vector2 = vector2.normalized;
				CameraContainer.localPosition = new Vector3(0f, 0.75f, 0f);
				CameraContainer.position += vector2 * 0.7f;
				Vector3 normalized = (vector - PlayerSingleton<PlayerCamera>.Instance.transform.position).normalized;
				normalized.y = 0f;
				CameraContainer.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetPlayerUser(NetworkObject playerObject)
		{
			RpcWriter___Server_SetPlayerUser_3323014238(playerObject);
			RpcLogic___SetPlayerUser_3323014238(playerObject);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetNPCUser(NetworkObject npcObject)
		{
			RpcWriter___Server_SetNPCUser_3323014238(npcObject);
			RpcLogic___SetNPCUser_3323014238(npcObject);
		}

		[ObserversRpc(RunLocally = true)]
		public virtual void ResetPot()
		{
			RpcWriter___Observers_ResetPot_2166136261();
			RpcLogic___ResetPot_2166136261();
		}

		public float GetAverageLightExposure(out float growSpeedMultiplier)
		{
			growSpeedMultiplier = 1f;
			float num = 0f;
			if (LightSourceOverride != null)
			{
				return LightSourceOverride.GrowSpeedMultiplier;
			}
			for (int i = 0; i < CoordinatePairs.Count; i++)
			{
				num += base.OwnerGrid.GetTile(CoordinatePairs[i].coord2).LightExposureNode.GetTotalExposure(out var growSpeedMultiplier2);
				growSpeedMultiplier += growSpeedMultiplier2;
			}
			growSpeedMultiplier /= CoordinatePairs.Count;
			return num / (float)CoordinatePairs.Count;
		}

		public bool CanAcceptSeed(out string reason)
		{
			if (SyncAccessor__003CSoilLevel_003Ek__BackingField < SoilCapacity)
			{
				reason = "No soil";
				return false;
			}
			if (NormalizedWaterLevel >= 1f)
			{
				reason = "Waterlogged";
				return false;
			}
			if (Plant != null)
			{
				reason = "Already contains seed";
				return false;
			}
			reason = string.Empty;
			return SyncAccessor__003CSoilLevel_003Ek__BackingField >= SoilCapacity;
		}

		public bool IsReadyForHarvest(out string reason)
		{
			if (Plant == null)
			{
				reason = "No plant in this pot";
				return false;
			}
			if (!Plant.IsFullyGrown)
			{
				reason = Mathf.Floor(Plant.NormalizedGrowthProgress * 100f) + "% grown";
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public override bool CanBeDestroyed(out string reason)
		{
			if (Plant != null)
			{
				reason = "Contains plant";
				return false;
			}
			if (((IUsable)this).IsInUse)
			{
				reason = "In use by other player";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public void OverrideRotation(float angle)
		{
			rotationOverridden = true;
			rotation = angle;
		}

		public Transform GetCameraPosition(ECameraPosition pos)
		{
			return pos switch
			{
				ECameraPosition.Closeup => CloseupPosition, 
				ECameraPosition.Midshot => MidshotPosition, 
				ECameraPosition.Fullshot => FullshotPosition, 
				ECameraPosition.BirdsEye => BirdsEyePosition, 
				_ => null, 
			};
		}

		public virtual void AddSoil(float amount)
		{
			SoilLevel = Mathf.Clamp(SyncAccessor__003CSoilLevel_003Ek__BackingField + amount, 0f, SoilCapacity);
			UpdateSoilScale();
		}

		private void SoilLevelChanged(float _prev, float _new, bool asServer)
		{
			UpdateSoilScale();
		}

		protected virtual void UpdateSoilScale()
		{
			Vector3 localScale = Vector3.Lerp(DirtMinScale, DirtMaxScale, SyncAccessor__003CSoilLevel_003Ek__BackingField / SoilCapacity);
			Dirt_Flat.localScale = localScale;
		}

		public virtual void SetSoilID(string id)
		{
			SoilID = id;
			UpdateSoilMaterial();
		}

		public virtual void SetSoilUses(int uses)
		{
			RemainingSoilUses = uses;
		}

		public void PushSoilDataToServer()
		{
			SendSoilData(SyncAccessor__003CSoilID_003Ek__BackingField, SyncAccessor__003CSoilLevel_003Ek__BackingField, SyncAccessor__003CRemainingSoilUses_003Ek__BackingField);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendSoilData(string soilID, float soilLevel, int soilUses)
		{
			RpcWriter___Server_SendSoilData_3104499779(soilID, soilLevel, soilUses);
		}

		public void SetSoilState(ESoilState state)
		{
			if (state == ESoilState.Flat && Plant == null)
			{
				Dirt_Parted.gameObject.SetActive(value: false);
				Dirt_Flat.gameObject.SetActive(value: true);
			}
			else
			{
				if (state != ESoilState.Parted && state != ESoilState.Packed)
				{
					return;
				}
				Dirt_Parted.gameObject.SetActive(value: true);
				Dirt_Flat.gameObject.SetActive(value: false);
				if (state == ESoilState.Packed)
				{
					for (int i = 0; i < SoilChunks.Length; i++)
					{
						SoilChunks[i].SetLerpedTransform(1f);
					}
				}
				else
				{
					for (int j = 0; j < SoilChunks.Length; j++)
					{
						SoilChunks[j].SetLerpedTransform(0f);
					}
				}
			}
		}

		protected virtual void UpdateSoilMaterial()
		{
			if (!(SyncAccessor__003CSoilID_003Ek__BackingField == string.Empty))
			{
				SoilDefinition soilDefinition = Registry.GetItem(SyncAccessor__003CSoilID_003Ek__BackingField) as SoilDefinition;
				Material material = soilDefinition.WetSoilMat;
				if (NormalizedWaterLevel <= 0f)
				{
					material = soilDefinition.DrySoilMat;
				}
				for (int i = 0; i < DirtRenderers.Count; i++)
				{
					DirtRenderers[i].material = material;
				}
				WaterLoggedVisuals.SetActive(NormalizedWaterLevel > 1f);
			}
		}

		public void ChangeWaterAmount(float change)
		{
			WaterLevel = Mathf.Clamp(SyncAccessor__003CWaterLevel_003Ek__BackingField + change, 0f, WaterCapacity);
			UpdateSoilMaterial();
			WaterLevelSlider.value = SyncAccessor__003CWaterLevel_003Ek__BackingField / WaterCapacity;
			NoWaterIcon.gameObject.SetActive(SyncAccessor__003CWaterLevel_003Ek__BackingField <= 0f);
		}

		public void PushWaterDataToServer()
		{
			SendWaterData(SyncAccessor__003CWaterLevel_003Ek__BackingField);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendWaterData(float waterLevel)
		{
			RpcWriter___Server_SendWaterData_431000436(waterLevel);
		}

		private void WaterLevelChanged(float _prev, float _new, bool asServer)
		{
			UpdateSoilMaterial();
			WaterLevelSlider.value = SyncAccessor__003CWaterLevel_003Ek__BackingField / WaterCapacity;
			NoWaterIcon.gameObject.SetActive(SyncAccessor__003CWaterLevel_003Ek__BackingField <= 0f);
		}

		public void SetTargetActive(bool active)
		{
			Target.gameObject.SetActive(active);
		}

		public void RandomizeTarget()
		{
			int num = 0;
			Vector3 vector;
			do
			{
				Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
				insideUnitSphere.y = 0f;
				vector = base.transform.position + insideUnitSphere * (PotRadius * 0.85f);
				vector.y = Target.position.y;
				num++;
			}
			while (Vector3.Distance(Target.position, vector) < 0.15f && num < 100);
			Target.position = vector;
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendAdditive(string additiveAssetPath, bool initial)
		{
			RpcWriter___Server_SendAdditive_310431262(additiveAssetPath, initial);
			RpcLogic___SendAdditive_310431262(additiveAssetPath, initial);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void ApplyAdditive(NetworkConnection conn, string additiveAssetPath, bool initial)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ApplyAdditive_619441887(conn, additiveAssetPath, initial);
				RpcLogic___ApplyAdditive_619441887(conn, additiveAssetPath, initial);
			}
			else
			{
				RpcWriter___Target_ApplyAdditive_619441887(conn, additiveAssetPath, initial);
			}
		}

		public float GetAdditiveGrowthMultiplier()
		{
			float num = 1f;
			foreach (Additive appliedAdditive in AppliedAdditives)
			{
				num *= appliedAdditive.GrowSpeedMultiplier;
			}
			return num;
		}

		public float GetNetYieldChange()
		{
			float num = 0f;
			foreach (Additive appliedAdditive in AppliedAdditives)
			{
				num += appliedAdditive.YieldChange;
			}
			return num;
		}

		public float GetNetQualityChange()
		{
			float num = 0f;
			foreach (Additive appliedAdditive in AppliedAdditives)
			{
				num += appliedAdditive.QualityChange;
			}
			return num;
		}

		public Additive GetAdditive(string additiveName)
		{
			return AppliedAdditives.Find((Additive x) => x.AdditiveName.ToLower() == additiveName.ToLower());
		}

		[ObserversRpc(RunLocally = true)]
		public void FullyGrowPlant()
		{
			RpcWriter___Observers_FullyGrowPlant_2166136261();
			RpcLogic___FullyGrowPlant_2166136261();
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendPlantSeed(string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
		{
			RpcWriter___Server_SendPlantSeed_2530605204(seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
			RpcLogic___SendPlantSeed_2530605204(seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void PlantSeed(NetworkConnection conn, string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_PlantSeed_709433087(conn, seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
				RpcLogic___PlantSeed_709433087(conn, seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
			}
			else
			{
				RpcWriter___Target_PlantSeed_709433087(conn, seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
			}
		}

		[ObserversRpc]
		private void SetGrowProgress(float progress)
		{
			RpcWriter___Observers_SetGrowProgress_431000436(progress);
		}

		private void PlantSeed(string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
		{
			if (Plant != null)
			{
				return;
			}
			if (SyncAccessor__003CSoilLevel_003Ek__BackingField < SoilCapacity)
			{
				Console.LogWarning("Pot not full of soil!");
				return;
			}
			SeedDefinition seedDefinition = Registry.GetItem(seedID) as SeedDefinition;
			if (seedDefinition == null)
			{
				Console.LogWarning("PlantSeed: seed not found with ID '" + seedDefinition?.ToString() + "'");
				return;
			}
			SetSoilState(ESoilState.Packed);
			Plant = UnityEngine.Object.Instantiate(seedDefinition.PlantPrefab.gameObject, PlantContainer).GetComponent<Plant>();
			Plant.transform.localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			Plant.Initialize(base.NetworkObject, normalizedSeedProgress, yieldLevel, qualityLevel);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void SetHarvestableActive(NetworkConnection conn, int harvestableIndex, bool active)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetHarvestableActive_338960014(conn, harvestableIndex, active);
				RpcLogic___SetHarvestableActive_338960014(conn, harvestableIndex, active);
			}
			else
			{
				RpcWriter___Target_SetHarvestableActive_338960014(conn, harvestableIndex, active);
			}
		}

		public void SetHarvestableActive_Local(int harvestableIndex, bool active)
		{
			if (Plant == null)
			{
				Console.LogWarning("SetHarvestableActive called but plant is null!");
			}
			else
			{
				if (Plant.IsHarvestableActive(harvestableIndex) == active)
				{
					return;
				}
				int count = Plant.ActiveHarvestables.Count;
				Plant.SetHarvestableActive(harvestableIndex, active);
				if (count > 0 && Plant.ActiveHarvestables.Count == 0)
				{
					if (InstanceFinder.IsServer)
					{
						float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("HarvestedPlantCount");
						NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("HarvestedPlantCount", (value + 1f).ToString());
						NetworkSingleton<LevelManager>.Instance.AddXP(5);
					}
					ResetPot();
				}
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendHarvestableActive(int harvestableIndex, bool active)
		{
			RpcWriter___Server_SendHarvestableActive_3658436649(harvestableIndex, active);
			RpcLogic___SendHarvestableActive_3658436649(harvestableIndex, active);
		}

		public void SendHarvestableActive_Local(int harvestableIndex, bool active)
		{
			SetHarvestableActive_Local(harvestableIndex, active);
		}

		public WorldspaceUIElement CreateWorldspaceUI()
		{
			if (WorldspaceUI != null)
			{
				Console.LogWarning(base.gameObject.name + " already has a worldspace UI element!");
			}
			if (base.ParentProperty == null)
			{
				Console.LogError(base.ParentProperty?.ToString() + " is not a child of a property!");
				return null;
			}
			PotUIElement component = UnityEngine.Object.Instantiate(WorldspaceUIPrefab, base.ParentProperty.WorldspaceUIContainer).GetComponent<PotUIElement>();
			component.Initialize(this);
			WorldspaceUI = component;
			return component;
		}

		public void DestroyWorldspaceUI()
		{
			if (WorldspaceUI != null)
			{
				WorldspaceUI.Destroy();
			}
		}

		public override string GetSaveString()
		{
			PlantData plantData = null;
			if (Plant != null)
			{
				plantData = Plant.GetPlantData();
			}
			return new PotData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, SyncAccessor__003CSoilID_003Ek__BackingField, SyncAccessor__003CSoilLevel_003Ek__BackingField, SyncAccessor__003CRemainingSoilUses_003Ek__BackingField, SyncAccessor__003CWaterLevel_003Ek__BackingField, AppliedAdditives.ConvertAll((Additive x) => x.AssetPath).ToArray(), plantData).GetJson();
		}

		public override List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			if (Configuration.ShouldSave())
			{
				list.Add("Configuration.json");
				((ISaveable)this).WriteSubfile(parentFolderPath, "Configuration", Configuration.GetSaveString());
			}
			list.AddRange(base.WriteData(parentFolderPath));
			return list;
		}

		public virtual void LoadPlant(PlantData data)
		{
			if (!string.IsNullOrEmpty(data.SeedID))
			{
				StartCoroutine(Wait());
			}
			IEnumerator Wait()
			{
				yield return new WaitUntil(() => base.NetworkObject.IsSpawned);
				SendPlantSeed(data.SeedID, data.GrowthProgress, data.YieldLevel, data.QualityLevel);
				if (Plant != null && data.ActiveBuds != null)
				{
					List<int> list = new List<int>(data.ActiveBuds);
					Plant.ActiveHarvestables.ToArray();
					for (int i = 0; i < Plant.FinalGrowthStage.GrowthSites.Length; i++)
					{
						Plant.SetHarvestableActive(i, list.Contains(i));
					}
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 6u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 5u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, PlayerUserObject);
				syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 4u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, NPCUserObject);
				syncVar____003CWaterLevel_003Ek__BackingField = new SyncVar<float>(this, 3u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, WaterLevel);
				syncVar____003CWaterLevel_003Ek__BackingField.OnChange += WaterLevelChanged;
				syncVar____003CRemainingSoilUses_003Ek__BackingField = new SyncVar<int>(this, 2u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, RemainingSoilUses);
				syncVar____003CSoilID_003Ek__BackingField = new SyncVar<string>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, SoilID);
				syncVar____003CSoilLevel_003Ek__BackingField = new SyncVar<float>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, SoilLevel);
				syncVar____003CSoilLevel_003Ek__BackingField.OnChange += SoilLevelChanged;
				RegisterServerRpc(8u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterServerRpc(9u, RpcReader___Server_SetPlayerUser_3323014238);
				RegisterServerRpc(10u, RpcReader___Server_SetNPCUser_3323014238);
				RegisterObserversRpc(11u, RpcReader___Observers_ResetPot_2166136261);
				RegisterServerRpc(12u, RpcReader___Server_SendSoilData_3104499779);
				RegisterServerRpc(13u, RpcReader___Server_SendWaterData_431000436);
				RegisterServerRpc(14u, RpcReader___Server_SendAdditive_310431262);
				RegisterObserversRpc(15u, RpcReader___Observers_ApplyAdditive_619441887);
				RegisterTargetRpc(16u, RpcReader___Target_ApplyAdditive_619441887);
				RegisterObserversRpc(17u, RpcReader___Observers_FullyGrowPlant_2166136261);
				RegisterServerRpc(18u, RpcReader___Server_SendPlantSeed_2530605204);
				RegisterObserversRpc(19u, RpcReader___Observers_PlantSeed_709433087);
				RegisterTargetRpc(20u, RpcReader___Target_PlantSeed_709433087);
				RegisterObserversRpc(21u, RpcReader___Observers_SetGrowProgress_431000436);
				RegisterObserversRpc(22u, RpcReader___Observers_SetHarvestableActive_338960014);
				RegisterTargetRpc(23u, RpcReader___Target_SetHarvestableActive_338960014);
				RegisterServerRpc(24u, RpcReader___Server_SendHarvestableActive_3658436649);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EObjectScripts_002EPot);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPotAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetRegistered();
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetRegistered();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetRegistered();
				syncVar____003CWaterLevel_003Ek__BackingField.SetRegistered();
				syncVar____003CRemainingSoilUses_003Ek__BackingField.SetRegistered();
				syncVar____003CSoilID_003Ek__BackingField.SetRegistered();
				syncVar____003CSoilLevel_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetConfigurer_3323014238(NetworkObject player)
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
				writer.WriteNetworkObject(player);
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetConfigurer_3323014238(NetworkObject player)
		{
			CurrentPlayerConfigurer = player;
		}

		private void RpcReader___Server_SetConfigurer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject player = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetConfigurer_3323014238(player);
			}
		}

		private void RpcWriter___Server_SetPlayerUser_3323014238(NetworkObject playerObject)
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
				writer.WriteNetworkObject(playerObject);
				SendServerRpc(9u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetPlayerUser_3323014238(NetworkObject playerObject)
		{
			if (SyncAccessor__003CPlayerUserObject_003Ek__BackingField != null && SyncAccessor__003CPlayerUserObject_003Ek__BackingField.Owner.IsLocalClient && playerObject != null && !playerObject.Owner.IsLocalClient)
			{
				Singleton<GameInput>.Instance.ExitAll();
			}
			PlayerUserObject = playerObject;
		}

		private void RpcReader___Server_SetPlayerUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject playerObject = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetPlayerUser_3323014238(playerObject);
			}
		}

		private void RpcWriter___Server_SetNPCUser_3323014238(NetworkObject npcObject)
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
				writer.WriteNetworkObject(npcObject);
				SendServerRpc(10u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetNPCUser_3323014238(NetworkObject npcObject)
		{
			NPCUserObject = npcObject;
		}

		private void RpcReader___Server_SetNPCUser_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject npcObject = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetNPCUser_3323014238(npcObject);
			}
		}

		private void RpcWriter___Observers_ResetPot_2166136261()
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
				SendObserversRpc(11u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___ResetPot_2166136261()
		{
			if (Plant != null)
			{
				Plant.Destroy(dropScraps: true);
			}
			Plant = null;
			if (InstanceFinder.IsServer)
			{
				SyncAccessor__003CRemainingSoilUses_003Ek__BackingField--;
			}
			if (SyncAccessor__003CRemainingSoilUses_003Ek__BackingField <= 0)
			{
				WaterLevel = 0f;
				SoilID = string.Empty;
				SoilLevel = 0f;
			}
			foreach (Additive appliedAdditive in AppliedAdditives)
			{
				UnityEngine.Object.Destroy(appliedAdditive.gameObject);
			}
			AppliedAdditives.Clear();
			SetSoilState(ESoilState.Flat);
			UpdateSoilScale();
			UpdateSoilMaterial();
			base.HasChanged = true;
		}

		private void RpcReader___Observers_ResetPot_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ResetPot_2166136261();
			}
		}

		private void RpcWriter___Server_SendSoilData_3104499779(string soilID, float soilLevel, int soilUses)
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
				writer.WriteString(soilID);
				writer.WriteSingle(soilLevel);
				writer.WriteInt32(soilUses);
				SendServerRpc(12u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendSoilData_3104499779(string soilID, float soilLevel, int soilUses)
		{
			SoilID = soilID;
			SoilLevel = soilLevel;
			RemainingSoilUses = soilUses;
		}

		private void RpcReader___Server_SendSoilData_3104499779(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string soilID = PooledReader0.ReadString();
			float soilLevel = PooledReader0.ReadSingle();
			int soilUses = PooledReader0.ReadInt32();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendSoilData_3104499779(soilID, soilLevel, soilUses);
			}
		}

		private void RpcWriter___Server_SendWaterData_431000436(float waterLevel)
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
				writer.WriteSingle(waterLevel);
				SendServerRpc(13u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendWaterData_431000436(float waterLevel)
		{
			WaterLevel = waterLevel;
		}

		private void RpcReader___Server_SendWaterData_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float waterLevel = PooledReader0.ReadSingle();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendWaterData_431000436(waterLevel);
			}
		}

		private void RpcWriter___Server_SendAdditive_310431262(string additiveAssetPath, bool initial)
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
				writer.WriteString(additiveAssetPath);
				writer.WriteBoolean(initial);
				SendServerRpc(14u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendAdditive_310431262(string additiveAssetPath, bool initial)
		{
			ApplyAdditive(null, additiveAssetPath, initial);
		}

		private void RpcReader___Server_SendAdditive_310431262(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string additiveAssetPath = PooledReader0.ReadString();
			bool initial = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendAdditive_310431262(additiveAssetPath, initial);
			}
		}

		private void RpcWriter___Observers_ApplyAdditive_619441887(NetworkConnection conn, string additiveAssetPath, bool initial)
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
				writer.WriteString(additiveAssetPath);
				writer.WriteBoolean(initial);
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___ApplyAdditive_619441887(NetworkConnection conn, string additiveAssetPath, bool initial)
		{
			if ((bool)AppliedAdditives.Find((Additive x) => x.AssetPath == additiveAssetPath))
			{
				Console.Log("Pot already contains additive at " + additiveAssetPath);
				return;
			}
			GameObject gameObject = Resources.Load(additiveAssetPath) as GameObject;
			if (gameObject == null)
			{
				Console.LogWarning("Failed to load additive at path: " + additiveAssetPath);
				return;
			}
			Additive component = UnityEngine.Object.Instantiate(gameObject, AdditivesContainer).GetComponent<Additive>();
			component.transform.localPosition = Vector3.zero;
			component.transform.localRotation = Quaternion.identity;
			if (Plant != null)
			{
				Plant.QualityLevel += component.QualityChange;
				Plant.YieldLevel += component.YieldChange;
				if (initial)
				{
					Plant.SetNormalizedGrowthProgress(Plant.NormalizedGrowthProgress + component.InstantGrowth);
					if (component.InstantGrowth > 0f)
					{
						PoofParticles.Play();
						PoofSound.Play();
					}
				}
			}
			AppliedAdditives.Add(component);
		}

		private void RpcReader___Observers_ApplyAdditive_619441887(PooledReader PooledReader0, Channel channel)
		{
			string additiveAssetPath = PooledReader0.ReadString();
			bool initial = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ApplyAdditive_619441887(null, additiveAssetPath, initial);
			}
		}

		private void RpcWriter___Target_ApplyAdditive_619441887(NetworkConnection conn, string additiveAssetPath, bool initial)
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
				writer.WriteString(additiveAssetPath);
				writer.WriteBoolean(initial);
				SendTargetRpc(16u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ApplyAdditive_619441887(PooledReader PooledReader0, Channel channel)
		{
			string additiveAssetPath = PooledReader0.ReadString();
			bool initial = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___ApplyAdditive_619441887(base.LocalConnection, additiveAssetPath, initial);
			}
		}

		private void RpcWriter___Observers_FullyGrowPlant_2166136261()
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
				SendObserversRpc(17u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___FullyGrowPlant_2166136261()
		{
			if (Plant == null)
			{
				Console.LogWarning("FullyGrowPlant called but plant is null!");
			}
			else
			{
				Plant.SetNormalizedGrowthProgress(1f);
			}
		}

		private void RpcReader___Observers_FullyGrowPlant_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___FullyGrowPlant_2166136261();
			}
		}

		private void RpcWriter___Server_SendPlantSeed_2530605204(string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
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
				writer.WriteString(seedID);
				writer.WriteSingle(normalizedSeedProgress);
				writer.WriteSingle(yieldLevel);
				writer.WriteSingle(qualityLevel);
				SendServerRpc(18u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendPlantSeed_2530605204(string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
		{
			PlantSeed(null, seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
		}

		private void RpcReader___Server_SendPlantSeed_2530605204(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string seedID = PooledReader0.ReadString();
			float normalizedSeedProgress = PooledReader0.ReadSingle();
			float yieldLevel = PooledReader0.ReadSingle();
			float qualityLevel = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendPlantSeed_2530605204(seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
			}
		}

		private void RpcWriter___Observers_PlantSeed_709433087(NetworkConnection conn, string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
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
				writer.WriteString(seedID);
				writer.WriteSingle(normalizedSeedProgress);
				writer.WriteSingle(yieldLevel);
				writer.WriteSingle(qualityLevel);
				SendObserversRpc(19u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___PlantSeed_709433087(NetworkConnection conn, string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
		{
			PlantSeed(seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
		}

		private void RpcReader___Observers_PlantSeed_709433087(PooledReader PooledReader0, Channel channel)
		{
			string seedID = PooledReader0.ReadString();
			float normalizedSeedProgress = PooledReader0.ReadSingle();
			float yieldLevel = PooledReader0.ReadSingle();
			float qualityLevel = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___PlantSeed_709433087(null, seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
			}
		}

		private void RpcWriter___Target_PlantSeed_709433087(NetworkConnection conn, string seedID, float normalizedSeedProgress, float yieldLevel, float qualityLevel)
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
				writer.WriteString(seedID);
				writer.WriteSingle(normalizedSeedProgress);
				writer.WriteSingle(yieldLevel);
				writer.WriteSingle(qualityLevel);
				SendTargetRpc(20u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_PlantSeed_709433087(PooledReader PooledReader0, Channel channel)
		{
			string seedID = PooledReader0.ReadString();
			float normalizedSeedProgress = PooledReader0.ReadSingle();
			float yieldLevel = PooledReader0.ReadSingle();
			float qualityLevel = PooledReader0.ReadSingle();
			if (base.IsClientInitialized)
			{
				RpcLogic___PlantSeed_709433087(base.LocalConnection, seedID, normalizedSeedProgress, yieldLevel, qualityLevel);
			}
		}

		private void RpcWriter___Observers_SetGrowProgress_431000436(float progress)
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
				writer.WriteSingle(progress);
				SendObserversRpc(21u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetGrowProgress_431000436(float progress)
		{
			if (Plant == null)
			{
				Console.LogWarning("SetGrowProgress called but plant is null!");
			}
			else
			{
				Plant.SetNormalizedGrowthProgress(progress);
			}
		}

		private void RpcReader___Observers_SetGrowProgress_431000436(PooledReader PooledReader0, Channel channel)
		{
			float progress = PooledReader0.ReadSingle();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetGrowProgress_431000436(progress);
			}
		}

		private void RpcWriter___Observers_SetHarvestableActive_338960014(NetworkConnection conn, int harvestableIndex, bool active)
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
				writer.WriteInt32(harvestableIndex);
				writer.WriteBoolean(active);
				SendObserversRpc(22u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetHarvestableActive_338960014(NetworkConnection conn, int harvestableIndex, bool active)
		{
			SetHarvestableActive_Local(harvestableIndex, active);
		}

		private void RpcReader___Observers_SetHarvestableActive_338960014(PooledReader PooledReader0, Channel channel)
		{
			int harvestableIndex = PooledReader0.ReadInt32();
			bool active = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetHarvestableActive_338960014(null, harvestableIndex, active);
			}
		}

		private void RpcWriter___Target_SetHarvestableActive_338960014(NetworkConnection conn, int harvestableIndex, bool active)
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
				writer.WriteInt32(harvestableIndex);
				writer.WriteBoolean(active);
				SendTargetRpc(23u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetHarvestableActive_338960014(PooledReader PooledReader0, Channel channel)
		{
			int harvestableIndex = PooledReader0.ReadInt32();
			bool active = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetHarvestableActive_338960014(base.LocalConnection, harvestableIndex, active);
			}
		}

		private void RpcWriter___Server_SendHarvestableActive_3658436649(int harvestableIndex, bool active)
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
				writer.WriteInt32(harvestableIndex);
				writer.WriteBoolean(active);
				SendServerRpc(24u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendHarvestableActive_3658436649(int harvestableIndex, bool active)
		{
			SetHarvestableActive(null, harvestableIndex, active);
		}

		private void RpcReader___Server_SendHarvestableActive_3658436649(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int harvestableIndex = PooledReader0.ReadInt32();
			bool active = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendHarvestableActive_3658436649(harvestableIndex, active);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EPot(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 6:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value2 = PooledReader0.ReadNetworkObject();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value2, Boolean2);
				return true;
			}
			case 5:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value6 = PooledReader0.ReadNetworkObject();
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value6, Boolean2);
				return true;
			}
			case 4:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value3 = PooledReader0.ReadNetworkObject();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value3, Boolean2);
				return true;
			}
			case 3:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CWaterLevel_003Ek__BackingField.SetValue(syncVar____003CWaterLevel_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				float value5 = PooledReader0.ReadSingle();
				syncVar____003CWaterLevel_003Ek__BackingField.SetValue(value5, Boolean2);
				return true;
			}
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CRemainingSoilUses_003Ek__BackingField.SetValue(syncVar____003CRemainingSoilUses_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				int value7 = PooledReader0.ReadInt32();
				syncVar____003CRemainingSoilUses_003Ek__BackingField.SetValue(value7, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CSoilID_003Ek__BackingField.SetValue(syncVar____003CSoilID_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				string value4 = PooledReader0.ReadString();
				syncVar____003CSoilID_003Ek__BackingField.SetValue(value4, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CSoilLevel_003Ek__BackingField.SetValue(syncVar____003CSoilLevel_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
				syncVar____003CSoilLevel_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EPot_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			SoilCover.gameObject.SetActive(value: false);
			SetTargetActive(active: false);
		}
	}
}
