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
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Misc;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts
{
	public class MixingStation : GridItem, IUsable, IItemSlotOwner, ITransitEntity, IConfigurable
	{
		public ItemSlot ProductSlot;

		public ItemSlot MixerSlot;

		public ItemSlot OutputSlot;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CNPCUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

		public bool RequiresIngredientInsertion = true;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		[Header("Settings")]
		public int MixTimePerItem = 15;

		public int MaxMixQuantity = 10;

		[Header("Prefabs")]
		public GameObject JugPrefab;

		[Header("References")]
		public InteractableObject IntObj;

		public Transform CameraPosition;

		public Transform CameraPosition_CombineIngredients;

		public Transform CameraPosition_StartMachine;

		public StorageVisualizer InputVisuals;

		public StorageVisualizer OutputVisuals;

		public DigitalAlarm Clock;

		public ToggleableLight Light;

		public NewMixDiscoveryBox DiscoveryBox;

		public Transform ItemContainer;

		public Transform[] IngredientTransforms;

		public Fillable BowlFillable;

		public Clickable StartButton;

		public Transform JugAlignment;

		public Rigidbody Anchor;

		public BoxCollider TrashSpawnVolume;

		public Transform uiPoint;

		public Transform[] accessPoints;

		public ConfigurationReplicator configReplicator;

		[Header("Sounds")]
		public StartLoopStopAudio MachineSound;

		public AudioSourceController StartSound;

		public AudioSourceController StopSound;

		[Header("Mix Timing")]
		[Header("UI")]
		public MixingStationUIElement WorldspaceUIPrefab;

		public Sprite typeIcon;

		public UnityEvent onMixStart;

		public UnityEvent onMixDone;

		public UnityEvent onOutputCollected;

		public UnityEvent onStartButtonClicked;

		public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted;

		public bool IsOpen { get; private set; }

		public MixOperation CurrentMixOperation { get; set; }

		public bool IsMixingDone
		{
			get
			{
				if (CurrentMixOperation != null)
				{
					return CurrentMixTime >= GetMixTimeForCurrentOperation();
				}
				return false;
			}
		}

		public int CurrentMixTime { get; protected set; }

		public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();


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

		public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();


		public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();


		public Vector3 LinkOriginPoint => uiPoint.position;

		public Transform[] AccessPoints => accessPoints;

		public bool Selectable { get; } = true;


		public bool IsAcceptingItems { get; set; } = true;


		public EntityConfiguration Configuration => stationConfiguration;

		protected MixingStationConfiguration stationConfiguration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.MixingStation;

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

		public Vector3 DiscoveryBoxOffset { get; private set; }

		public Quaternion DiscoveryBoxRotation { get; private set; }

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
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002EMixingStation_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			if (!isGhost)
			{
				TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
				if (StartButton != null)
				{
					StartButton.onClickStart.AddListener(StartButtonClicked);
				}
			}
		}

		public override void InitializeGridItem(ItemInstance instance, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, string GUID)
		{
			bool initialized = base.Initialized;
			base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
			if (!initialized && !isGhost)
			{
				base.ParentProperty.Configurables.Add(this);
				stationConfiguration = new MixingStationConfiguration(configReplicator, this, this);
				CreateWorldspaceUI();
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			((IItemSlotOwner)this).SendItemsToClient(connection);
			if (CurrentMixOperation != null)
			{
				SetMixOperation(connection, CurrentMixOperation, CurrentMixTime);
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

		public override bool CanBeDestroyed(out string reason)
		{
			if (((IItemSlotOwner)this).GetTotalItemCount() > 0)
			{
				reason = "Contains items";
				return false;
			}
			if (CurrentMixOperation != null && IsMixingDone)
			{
				reason = "Contains items";
				return false;
			}
			if (CurrentMixOperation != null)
			{
				reason = "Mixing in progress";
				return false;
			}
			if (((IUsable)this).IsInUse)
			{
				reason = "Currently in use";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			Configuration.Destroy();
			DestroyWorldspaceUI();
			base.ParentProperty.Configurables.Remove(this);
			base.DestroyItem(callOnServer);
		}

		protected virtual void MinPass()
		{
			if (CurrentMixOperation != null || OutputSlot.Quantity > 0)
			{
				int num = 0;
				if (CurrentMixOperation != null)
				{
					int currentMixTime = CurrentMixTime;
					CurrentMixTime++;
					num = GetMixTimeForCurrentOperation();
					if (CurrentMixTime >= num && currentMixTime < num && InstanceFinder.IsServer)
					{
						NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Completed", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Completed") + 1f).ToString());
						MixingDone_Networked();
					}
				}
				if (Clock != null)
				{
					Clock.SetScreenLit(lit: true);
					Clock.DisplayMinutes(Mathf.Max(num - CurrentMixTime, 0));
				}
				if (Light != null)
				{
					if (IsMixingDone)
					{
						Light.isOn = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.DailyMinTotal % 2 == 0;
					}
					else
					{
						Light.isOn = true;
					}
				}
			}
			else
			{
				if (Clock != null)
				{
					Clock.SetScreenLit(lit: false);
					Clock.DisplayText(string.Empty);
				}
				if (Light != null && IsMixingDone)
				{
					Light.isOn = false;
				}
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendMixingOperation(MixOperation operation, int mixTime)
		{
			RpcWriter___Server_SendMixingOperation_2669582547(operation, mixTime);
			RpcLogic___SendMixingOperation_2669582547(operation, mixTime);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public virtual void SetMixOperation(NetworkConnection conn, MixOperation operation, int mixTIme)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetMixOperation_1073078804(conn, operation, mixTIme);
				RpcLogic___SetMixOperation_1073078804(conn, operation, mixTIme);
			}
			else
			{
				RpcWriter___Target_SetMixOperation_1073078804(conn, operation, mixTIme);
			}
		}

		public virtual void MixingStart()
		{
			StartSound.Play();
			MachineSound.StartAudio();
			if (onMixStart != null)
			{
				onMixStart.Invoke();
			}
		}

		[ObserversRpc]
		public void MixingDone_Networked()
		{
			RpcWriter___Observers_MixingDone_Networked_2166136261();
		}

		public virtual void MixingDone()
		{
			MachineSound.StopAudio();
			StopSound.Play();
			TryCreateOutputItems();
			if (onMixDone != null)
			{
				onMixDone.Invoke();
			}
		}

		public bool DoesOutputHaveSpace(StationRecipe recipe)
		{
			StorableItemInstance productInstance = recipe.GetProductInstance(GetIngredients());
			return OutputSlot.GetCapacityForItem(productInstance) >= 1;
		}

		public List<ItemInstance> GetIngredients()
		{
			List<ItemInstance> list = new List<ItemInstance>();
			if (ProductSlot.ItemInstance != null)
			{
				list.Add(ProductSlot.ItemInstance);
			}
			if (MixerSlot.ItemInstance != null)
			{
				list.Add(MixerSlot.ItemInstance);
			}
			return list;
		}

		public int GetMixQuantity()
		{
			if (GetProduct() == null || GetMixer() == null)
			{
				return 0;
			}
			return Mathf.Min(Mathf.Min(ProductSlot.Quantity, MixerSlot.Quantity), MaxMixQuantity);
		}

		public bool CanStartMix()
		{
			if (GetMixQuantity() > 0)
			{
				return OutputSlot.Quantity == 0;
			}
			return false;
		}

		public ProductDefinition GetProduct()
		{
			if (ProductSlot.ItemInstance != null)
			{
				return ProductSlot.ItemInstance.Definition as ProductDefinition;
			}
			return null;
		}

		public PropertyItemDefinition GetMixer()
		{
			if (MixerSlot.ItemInstance != null)
			{
				PropertyItemDefinition propertyItemDefinition = MixerSlot.ItemInstance.Definition as PropertyItemDefinition;
				if (propertyItemDefinition != null && NetworkSingleton<ProductManager>.Instance.ValidMixIngredients.Contains(propertyItemDefinition))
				{
					return propertyItemDefinition;
				}
			}
			return null;
		}

		public int GetMixTimeForCurrentOperation()
		{
			if (CurrentMixOperation == null)
			{
				return 0;
			}
			return MixTimePerItem * CurrentMixOperation.Quantity;
		}

		[ServerRpc(RequireOwnership = false)]
		public void TryCreateOutputItems()
		{
			RpcWriter___Server_TryCreateOutputItems_2166136261();
		}

		public void SetStartButtonClickable(bool clickable)
		{
			StartButton.ClickableEnabled = clickable;
		}

		private void OutputChanged()
		{
			if (OutputSlot.Quantity == 0)
			{
				if (onOutputCollected != null)
				{
					onOutputCollected.Invoke();
				}
				if (InstanceFinder.IsServer)
				{
					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Collected", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Collected") + 1f).ToString());
				}
			}
		}

		private void StartButtonClicked(RaycastHit hit)
		{
			SetStartButtonClickable(clickable: false);
			if (onStartButtonClicked != null)
			{
				onStartButtonClicked.Invoke();
			}
		}

		public void Open()
		{
			IsOpen = true;
			if (CurrentMixOperation != null && IsMixingDone)
			{
				TryCreateOutputItems();
			}
			SetPlayerUser(Player.Local.NetworkObject);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			Singleton<MixingStationCanvas>.Instance.Open(this);
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
		}

		public void Close()
		{
			IsOpen = false;
			SetPlayerUser(null);
			if (DiscoveryBox != null)
			{
				DiscoveryBox.transform.SetParent(CameraPosition.transform);
				DiscoveryBox.gameObject.SetActive(value: false);
			}
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
				PlayerSingleton<PlayerCamera>.Instance.LockMouse();
				PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				Singleton<CompassManager>.Instance.SetVisible(visible: true);
			}
		}

		public void Hovered()
		{
			if (((IUsable)this).IsInUse || Singleton<ManagementClipboard>.Instance.IsEquipped)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				return;
			}
			IntObj.SetMessage("Use " + base.ItemInstance.Name);
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}

		public void Interacted()
		{
			if (!((IUsable)this).IsInUse && !Singleton<ManagementClipboard>.Instance.IsEquipped)
			{
				Open();
			}
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
			MixingStationUIElement component = UnityEngine.Object.Instantiate(WorldspaceUIPrefab, base.ParentProperty.WorldspaceUIContainer).GetComponent<MixingStationUIElement>();
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

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
			RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc(RunLocally = true)]
		private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
				RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			}
			else
			{
				RpcWriter___Target_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
				RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			}
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetItemSlotQuantity(int itemSlotIndex, int quantity)
		{
			RpcWriter___Server_SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
			RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		}

		[ObserversRpc(RunLocally = true)]
		private void SetItemSlotQuantity_Internal(int itemSlotIndex, int quantity)
		{
			RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
			RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			RpcWriter___Server_SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			RpcLogic___SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}

		[TargetRpc(RunLocally = true)]
		[ObserversRpc(RunLocally = true)]
		private void SetSlotLocked_Internal(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
				RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			}
			else
			{
				RpcWriter___Target_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
				RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
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

		public override string GetSaveString()
		{
			return new MixingStationData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, new ItemSet(new List<ItemSlot> { ProductSlot }), new ItemSet(new List<ItemSlot> { MixerSlot }), new ItemSet(new List<ItemSlot> { OutputSlot }), CurrentMixOperation, CurrentMixTime).GetJson();
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

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, PlayerUserObject);
				syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, NPCUserObject);
				RegisterServerRpc(8u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterServerRpc(9u, RpcReader___Server_SendMixingOperation_2669582547);
				RegisterObserversRpc(10u, RpcReader___Observers_SetMixOperation_1073078804);
				RegisterTargetRpc(11u, RpcReader___Target_SetMixOperation_1073078804);
				RegisterObserversRpc(12u, RpcReader___Observers_MixingDone_Networked_2166136261);
				RegisterServerRpc(13u, RpcReader___Server_TryCreateOutputItems_2166136261);
				RegisterServerRpc(14u, RpcReader___Server_SetStoredInstance_2652194801);
				RegisterObserversRpc(15u, RpcReader___Observers_SetStoredInstance_Internal_2652194801);
				RegisterTargetRpc(16u, RpcReader___Target_SetStoredInstance_Internal_2652194801);
				RegisterServerRpc(17u, RpcReader___Server_SetItemSlotQuantity_1692629761);
				RegisterObserversRpc(18u, RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761);
				RegisterServerRpc(19u, RpcReader___Server_SetSlotLocked_3170825843);
				RegisterTargetRpc(20u, RpcReader___Target_SetSlotLocked_Internal_3170825843);
				RegisterObserversRpc(21u, RpcReader___Observers_SetSlotLocked_Internal_3170825843);
				RegisterServerRpc(22u, RpcReader___Server_SetPlayerUser_3323014238);
				RegisterServerRpc(23u, RpcReader___Server_SetNPCUser_3323014238);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EObjectScripts_002EMixingStation);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EMixingStationAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetRegistered();
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetRegistered();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetRegistered();
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

		private void RpcWriter___Server_SendMixingOperation_2669582547(MixOperation operation, int mixTime)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated(writer, operation);
				writer.WriteInt32(mixTime);
				SendServerRpc(9u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendMixingOperation_2669582547(MixOperation operation, int mixTime)
		{
			SetMixOperation(null, operation, mixTime);
		}

		private void RpcReader___Server_SendMixingOperation_2669582547(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			MixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			int mixTime = PooledReader0.ReadInt32();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendMixingOperation_2669582547(operation, mixTime);
			}
		}

		private void RpcWriter___Observers_SetMixOperation_1073078804(NetworkConnection conn, MixOperation operation, int mixTIme)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated(writer, operation);
				writer.WriteInt32(mixTIme);
				SendObserversRpc(10u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___SetMixOperation_1073078804(NetworkConnection conn, MixOperation operation, int mixTIme)
		{
			if (operation != null && string.IsNullOrEmpty(operation.ProductID))
			{
				operation = null;
			}
			MixOperation currentMixOperation = CurrentMixOperation;
			CurrentMixOperation = operation;
			CurrentMixTime = mixTIme;
			if (operation != null)
			{
				if (currentMixOperation == null)
				{
					MixingStart();
				}
			}
			else if (currentMixOperation != null && onMixDone != null)
			{
				onMixDone.Invoke();
			}
		}

		private void RpcReader___Observers_SetMixOperation_1073078804(PooledReader PooledReader0, Channel channel)
		{
			MixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			int mixTIme = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetMixOperation_1073078804(null, operation, mixTIme);
			}
		}

		private void RpcWriter___Target_SetMixOperation_1073078804(NetworkConnection conn, MixOperation operation, int mixTIme)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated(writer, operation);
				writer.WriteInt32(mixTIme);
				SendTargetRpc(11u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetMixOperation_1073078804(PooledReader PooledReader0, Channel channel)
		{
			MixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			int mixTIme = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetMixOperation_1073078804(base.LocalConnection, operation, mixTIme);
			}
		}

		private void RpcWriter___Observers_MixingDone_Networked_2166136261()
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
				SendObserversRpc(12u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___MixingDone_Networked_2166136261()
		{
			MixingDone();
		}

		private void RpcReader___Observers_MixingDone_Networked_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___MixingDone_Networked_2166136261();
			}
		}

		private void RpcWriter___Server_TryCreateOutputItems_2166136261()
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
				SendServerRpc(13u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___TryCreateOutputItems_2166136261()
		{
			if (CurrentMixOperation != null && CurrentMixOperation.IsOutputKnown(out var knownProduct))
			{
				QualityItemInstance qualityItemInstance = knownProduct.GetDefaultInstance(CurrentMixOperation.Quantity) as QualityItemInstance;
				qualityItemInstance.SetQuality(CurrentMixOperation.ProductQuality);
				OutputSlot.AddItem(qualityItemInstance);
				if (NetworkSingleton<ProductManager>.Instance.GetRecipe(CurrentMixOperation.ProductID, CurrentMixOperation.IngredientID) == null)
				{
					NetworkSingleton<ProductManager>.Instance.SendMixRecipe(CurrentMixOperation.ProductID, CurrentMixOperation.IngredientID, qualityItemInstance.ID);
				}
				SetMixOperation(null, null, 0);
			}
		}

		private void RpcReader___Server_TryCreateOutputItems_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___TryCreateOutputItems_2166136261();
			}
		}

		private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteItemInstance(instance);
				SendServerRpc(14u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			if (conn == null || conn.ClientId == -1)
			{
				SetStoredInstance_Internal(null, itemSlotIndex, instance);
			}
			else
			{
				SetStoredInstance_Internal(conn, itemSlotIndex, instance);
			}
		}

		private void RpcReader___Server_SetStoredInstance_2652194801(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			int itemSlotIndex = PooledReader0.ReadInt32();
			ItemInstance instance = PooledReader0.ReadItemInstance();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetStoredInstance_2652194801(conn2, itemSlotIndex, instance);
			}
		}

		private void RpcWriter___Observers_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteItemInstance(instance);
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			if (instance != null)
			{
				ItemSlots[itemSlotIndex].SetStoredItem(instance, _internal: true);
			}
			else
			{
				ItemSlots[itemSlotIndex].ClearStoredInstance(_internal: true);
			}
		}

		private void RpcReader___Observers_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			ItemInstance instance = PooledReader0.ReadItemInstance();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetStoredInstance_Internal_2652194801(null, itemSlotIndex, instance);
			}
		}

		private void RpcWriter___Target_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteItemInstance(instance);
				SendTargetRpc(16u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			ItemInstance instance = PooledReader0.ReadItemInstance();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetStoredInstance_Internal_2652194801(base.LocalConnection, itemSlotIndex, instance);
			}
		}

		private void RpcWriter___Server_SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteInt32(quantity);
				SendServerRpc(17u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
		{
			SetItemSlotQuantity_Internal(itemSlotIndex, quantity);
		}

		private void RpcReader___Server_SetItemSlotQuantity_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
			}
		}

		private void RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteInt32(quantity);
				SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
		{
			ItemSlots[itemSlotIndex].SetQuantity(quantity, _internal: true);
		}

		private void RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
			}
		}

		private void RpcWriter___Server_SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteBoolean(locked);
				writer.WriteNetworkObject(lockOwner);
				writer.WriteString(lockReason);
				SendServerRpc(19u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			if (conn == null || conn.ClientId == -1)
			{
				SetSlotLocked_Internal(null, itemSlotIndex, locked, lockOwner, lockReason);
			}
			else
			{
				SetSlotLocked_Internal(conn, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		private void RpcReader___Server_SetSlotLocked_3170825843(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			int itemSlotIndex = PooledReader0.ReadInt32();
			bool locked = PooledReader0.ReadBoolean();
			NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
			string lockReason = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetSlotLocked_3170825843(conn2, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		private void RpcWriter___Target_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteBoolean(locked);
				writer.WriteNetworkObject(lockOwner);
				writer.WriteString(lockReason);
				SendTargetRpc(20u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			if (locked)
			{
				ItemSlots[itemSlotIndex].ApplyLock(lockOwner, lockReason, _internal: true);
			}
			else
			{
				ItemSlots[itemSlotIndex].RemoveLock(_internal: true);
			}
		}

		private void RpcReader___Target_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			bool locked = PooledReader0.ReadBoolean();
			NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
			string lockReason = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetSlotLocked_Internal_3170825843(base.LocalConnection, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		private void RpcWriter___Observers_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteBoolean(locked);
				writer.WriteNetworkObject(lockOwner);
				writer.WriteString(lockReason);
				SendObserversRpc(21u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcReader___Observers_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			bool locked = PooledReader0.ReadBoolean();
			NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
			string lockReason = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetSlotLocked_Internal_3170825843(null, itemSlotIndex, locked, lockOwner, lockReason);
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
				SendServerRpc(22u, writer, channel, DataOrderType.Default);
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
				SendServerRpc(23u, writer, channel, DataOrderType.Default);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EMixingStation(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value3 = PooledReader0.ReadNetworkObject();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value3, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value2 = PooledReader0.ReadNetworkObject();
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value2, Boolean2);
				return true;
			}
			case 0:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(calledByUser: true),  true);
					return true;
				}
				NetworkObject value = PooledReader0.ReadNetworkObject();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EMixingStation_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			if (!isGhost)
			{
				ProductSlot.AddFilter(new ItemFilter_UnpackagedProduct());
				ProductSlot.SetSlotOwner(this);
				InputVisuals.AddSlot(ProductSlot);
				ItemSlot productSlot = ProductSlot;
				productSlot.onItemDataChanged = (Action)Delegate.Combine(productSlot.onItemDataChanged, (Action)delegate
				{
					base.HasChanged = true;
				});
				MixerSlot.SetSlotOwner(this);
				InputVisuals.AddSlot(MixerSlot);
				ItemSlot mixerSlot = MixerSlot;
				mixerSlot.onItemDataChanged = (Action)Delegate.Combine(mixerSlot.onItemDataChanged, (Action)delegate
				{
					base.HasChanged = true;
				});
				OutputSlot.SetIsAddLocked(locked: true);
				OutputSlot.SetSlotOwner(this);
				OutputVisuals.AddSlot(OutputSlot);
				ItemSlot outputSlot = OutputSlot;
				outputSlot.onItemDataChanged = (Action)Delegate.Combine(outputSlot.onItemDataChanged, (Action)delegate
				{
					base.HasChanged = true;
				});
				ItemSlot outputSlot2 = OutputSlot;
				outputSlot2.onItemDataChanged = (Action)Delegate.Combine(outputSlot2.onItemDataChanged, new Action(OutputChanged));
				DiscoveryBoxOffset = DiscoveryBox.transform.localPosition;
				DiscoveryBoxRotation = DiscoveryBox.transform.localRotation;
				InputSlots.AddRange(new List<ItemSlot> { ProductSlot, MixerSlot });
				OutputSlots.Add(OutputSlot);
			}
		}
	}
}
