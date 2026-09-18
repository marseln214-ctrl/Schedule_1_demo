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
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class ChemistryStation : GridItem, IUsable, IItemSlotOwner, ITransitEntity, IConfigurable
	{
		public enum EStep
		{
			CombineIngredients = 0,
			Stir = 1,
			LowerBoilingFlask = 2,
			PourIntoBoilingFlask = 3,
			RaiseBoilingFlask = 4,
			StartHeat = 5,
			Cook = 6,
			LowerBoilingFlaskAgain = 7,
			PourThroughFilter = 8
		}

		public const float FOV_OVERRIDE = 65f;

		public const int INPUT_SLOT_COUNT = 3;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CNPCUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

		public ItemSlot[] IngredientSlots;

		public ItemSlot OutputSlot;

		[Header("References")]
		public InteractableObject IntObj;

		public Transform CameraPosition_Default;

		public Transform CameraPosition_Stirring;

		public Transform StaticBeaker;

		public Transform StaticFunnel;

		public Transform StaticStirringRod;

		public Transform ItemContainer;

		public LabStand LabStand;

		public StorageVisualizer InputVisuals;

		public StorageVisualizer OutputVisuals;

		public Rigidbody AnchorRb;

		public BunsenBurner Burner;

		public BoilingFlask BoilingFlask;

		public DigitalAlarm Alarm;

		public Transform uiPoint;

		public Transform[] accessPoints;

		public ConfigurationReplicator configReplicator;

		[Header("Slot Display Points")]
		public Transform InputSlotsPosition;

		public Transform OutputSlotPosition;

		[Header("Transforms")]
		public Transform[] IngredientTransforms;

		public Transform BeakerAlignmentTransform;

		[Header("Prefabs")]
		public GameObject BeakerPrefab;

		public StirringRod StirringRodPrefab;

		[Header("UI")]
		public ChemistryStationUIElement WorldspaceUIPrefab;

		public Sprite typeIcon;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EChemistryStationAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EChemistryStationAssembly_002DCSharp_002Edll_Excuted;

		public bool isOpen => SyncAccessor__003CPlayerUserObject_003Ek__BackingField == Player.Local.NetworkObject;

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

		public ChemistryCookOperation CurrentCookOperation { get; set; }

		public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();


		public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();


		public Vector3 LinkOriginPoint => UIPoint.position;

		public Transform[] AccessPoints => accessPoints;

		public bool Selectable { get; } = true;


		public bool IsAcceptingItems { get; set; } = true;


		public EntityConfiguration Configuration => stationConfiguration;

		protected ChemistryStationConfiguration stationConfiguration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.ChemistryStation;

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
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002EChemistryStation_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void InitializeGridItem(ItemInstance instance, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, string GUID)
		{
			bool initialized = base.Initialized;
			base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
			if (!initialized && !isGhost)
			{
				GameInput.RegisterExitListener(Exit, 4);
				TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance2.onMinutePass = (Action)Delegate.Combine(instance2.onMinutePass, new Action(MinPass));
				base.ParentProperty.Configurables.Add(this);
				stationConfiguration = new ChemistryStationConfiguration(configReplicator, this, this);
				CreateWorldspaceUI();
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			((IItemSlotOwner)this).SendItemsToClient(connection);
			if (CurrentCookOperation != null)
			{
				SetCookOperation(connection, CurrentCookOperation);
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

		private void OnDestroy()
		{
			if (!isGhost)
			{
				TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
				Configuration.Destroy();
				DestroyWorldspaceUI();
				base.ParentProperty.Configurables.Remove(this);
			}
		}

		protected virtual void MinPass()
		{
			Alarm.FlashScreen = false;
			if (CurrentCookOperation != null)
			{
				CurrentCookOperation.Progress(1);
				base.HasChanged = true;
				float t = Mathf.Clamp01((float)CurrentCookOperation.CurrentTime / (float)CurrentCookOperation.Recipe.CookTime_Mins);
				BoilingFlask.LiquidContainer.SetLiquidColor(Color.Lerp(CurrentCookOperation.StartLiquidColor, CurrentCookOperation.Recipe.FinalLiquidColor, t));
				if (InstanceFinder.IsServer && CurrentCookOperation.CurrentTime >= CurrentCookOperation.Recipe.CookTime_Mins)
				{
					FinalizeOperation();
				}
			}
			UpdateClock();
		}

		private void UpdateClock()
		{
			if (CurrentCookOperation != null)
			{
				int b = CurrentCookOperation.Recipe.CookTime_Mins - CurrentCookOperation.CurrentTime;
				b = Mathf.Max(0, b);
				Alarm.DisplayMinutes(b);
				if (CurrentCookOperation.CurrentTime >= CurrentCookOperation.Recipe.CookTime_Mins)
				{
					Alarm.FlashScreen = true;
					Burner.SetDialPosition(0f);
				}
				else
				{
					Alarm.SetScreenLit(lit: true);
				}
			}
			else
			{
				Alarm.SetScreenLit(lit: false);
				Alarm.DisplayText(string.Empty);
			}
		}

		protected virtual void Update()
		{
			StaticFunnel.gameObject.SetActive(!LabStand.Funnel.gameObject.activeSelf);
		}

		public Beaker CreateBeaker()
		{
			Beaker component = UnityEngine.Object.Instantiate(BeakerPrefab, BeakerAlignmentTransform.position, BeakerAlignmentTransform.rotation).GetComponent<Beaker>();
			component.Anchor = AnchorRb;
			component.transform.SetParent(ItemContainer);
			component.Constraint.Container = ItemContainer;
			return component;
		}

		public StirringRod CreateStirringRod()
		{
			StirringRod component = UnityEngine.Object.Instantiate(StirringRodPrefab.gameObject, BeakerAlignmentTransform).GetComponent<StirringRod>();
			component.transform.localPosition = Vector3.zero;
			component.transform.localRotation = Quaternion.identity;
			return component;
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendCookOperation(ChemistryCookOperation op)
		{
			RpcWriter___Server_SendCookOperation_3552222198(op);
			RpcLogic___SendCookOperation_3552222198(op);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void SetCookOperation(NetworkConnection conn, ChemistryCookOperation operation)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetCookOperation_1024887225(conn, operation);
				RpcLogic___SetCookOperation_1024887225(conn, operation);
			}
			else
			{
				RpcWriter___Target_SetCookOperation_1024887225(conn, operation);
			}
		}

		[ObserversRpc]
		public void FinalizeOperation()
		{
			RpcWriter___Observers_FinalizeOperation_2166136261();
		}

		public void ResetStation()
		{
			BoilingFlask.SetRecipe(null);
			BoilingFlask.ResetContents();
			BoilingFlask.SetTemperature(0f);
			BoilingFlask.LockTemperature = false;
			Burner.SetDialPosition(0f);
			Burner.LockDial = false;
			LabStand.SetPosition(1f);
		}

		public bool DoesOutputHaveSpace(StationRecipe recipe)
		{
			StorableItemInstance productInstance = recipe.GetProductInstance(GetIngredients());
			return OutputSlot.GetCapacityForItem(productInstance) >= recipe.Product.Quantity;
		}

		public List<ItemInstance> GetIngredients()
		{
			List<ItemInstance> list = new List<ItemInstance>();
			ItemSlot[] ingredientSlots = IngredientSlots;
			foreach (ItemSlot itemSlot in ingredientSlots)
			{
				if (itemSlot.ItemInstance != null)
				{
					list.Add(itemSlot.ItemInstance);
				}
			}
			return list;
		}

		public bool HasIngredientsForRecipe(StationRecipe recipe)
		{
			List<ItemInstance> ingredients = GetIngredients();
			return recipe.DoIngredientsSuffice(ingredients);
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

		private void Exit(ExitAction action)
		{
			if (!action.used && isOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public void Open()
		{
		}

		public void Close()
		{
			Singleton<ChemistryStationCanvas>.Instance.Close(removeUI: true);
			LabStand.SetPosition(1f);
			SetPlayerUser(null);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
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

		public WorldspaceUIElement CreateWorldspaceUI()
		{
			if (WorldspaceUI != null)
			{
				Console.LogWarning(base.gameObject.name + " already has a worldspace UI element!");
			}
			if (base.ParentProperty == null)
			{
				Console.LogError(base.gameObject.name + " is not a child of a property!");
				return null;
			}
			ChemistryStationUIElement component = UnityEngine.Object.Instantiate(WorldspaceUIPrefab, base.ParentProperty.WorldspaceUIContainer).GetComponent<ChemistryStationUIElement>();
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
			string currentRecipeID = string.Empty;
			EQuality productQuality = EQuality.Standard;
			Color startLiquidColor = Color.clear;
			float liquidLevel = 0f;
			int currentTime = 0;
			if (CurrentCookOperation != null)
			{
				currentRecipeID = CurrentCookOperation.RecipeID;
				productQuality = CurrentCookOperation.ProductQuality;
				startLiquidColor = CurrentCookOperation.StartLiquidColor;
				liquidLevel = CurrentCookOperation.LiquidLevel;
				currentTime = CurrentCookOperation.CurrentTime;
			}
			return new ChemistryStationData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, new ItemSet(IngredientSlots), new ItemSet(new List<ItemSlot> { OutputSlot }), currentRecipeID, productQuality, startLiquidColor, liquidLevel, currentTime).GetJson();
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
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EChemistryStationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EChemistryStationAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, PlayerUserObject);
				syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, NPCUserObject);
				RegisterServerRpc(8u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterServerRpc(9u, RpcReader___Server_SendCookOperation_3552222198);
				RegisterObserversRpc(10u, RpcReader___Observers_SetCookOperation_1024887225);
				RegisterTargetRpc(11u, RpcReader___Target_SetCookOperation_1024887225);
				RegisterObserversRpc(12u, RpcReader___Observers_FinalizeOperation_2166136261);
				RegisterServerRpc(13u, RpcReader___Server_SetPlayerUser_3323014238);
				RegisterServerRpc(14u, RpcReader___Server_SetNPCUser_3323014238);
				RegisterServerRpc(15u, RpcReader___Server_SetStoredInstance_2652194801);
				RegisterObserversRpc(16u, RpcReader___Observers_SetStoredInstance_Internal_2652194801);
				RegisterTargetRpc(17u, RpcReader___Target_SetStoredInstance_Internal_2652194801);
				RegisterServerRpc(18u, RpcReader___Server_SetItemSlotQuantity_1692629761);
				RegisterObserversRpc(19u, RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761);
				RegisterServerRpc(20u, RpcReader___Server_SetSlotLocked_3170825843);
				RegisterTargetRpc(21u, RpcReader___Target_SetSlotLocked_Internal_3170825843);
				RegisterObserversRpc(22u, RpcReader___Observers_SetSlotLocked_Internal_3170825843);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EObjectScripts_002EChemistryStation);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EChemistryStationAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EChemistryStationAssembly_002DCSharp_002Edll_Excuted = true;
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

		private void RpcWriter___Server_SendCookOperation_3552222198(ChemistryCookOperation op)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated(writer, op);
				SendServerRpc(9u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendCookOperation_3552222198(ChemistryCookOperation op)
		{
			SetCookOperation(null, op);
		}

		private void RpcReader___Server_SendCookOperation_3552222198(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			ChemistryCookOperation op = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendCookOperation_3552222198(op);
			}
		}

		private void RpcWriter___Observers_SetCookOperation_1024887225(NetworkConnection conn, ChemistryCookOperation operation)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated(writer, operation);
				SendObserversRpc(10u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetCookOperation_1024887225(NetworkConnection conn, ChemistryCookOperation operation)
		{
			CurrentCookOperation = operation;
			BoilingFlask.LiquidContainer.SetLiquidLevel(operation.LiquidLevel);
			BoilingFlask.LiquidContainer.LiquidVolume.liquidColor1 = operation.StartLiquidColor;
			BoilingFlask.LiquidContainer.LiquidVolume.liquidColor2 = operation.StartLiquidColor;
			BoilingFlask.SetTemperature(operation.Recipe.CookTemperature);
			BoilingFlask.LockTemperature = true;
			Burner.SetDialPosition(CurrentCookOperation.Recipe.CookTemperature / 500f);
			Burner.LockDial = true;
			base.HasChanged = true;
			UpdateClock();
		}

		private void RpcReader___Observers_SetCookOperation_1024887225(PooledReader PooledReader0, Channel channel)
		{
			ChemistryCookOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetCookOperation_1024887225(null, operation);
			}
		}

		private void RpcWriter___Target_SetCookOperation_1024887225(NetworkConnection conn, ChemistryCookOperation operation)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated(writer, operation);
				SendTargetRpc(11u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetCookOperation_1024887225(PooledReader PooledReader0, Channel channel)
		{
			ChemistryCookOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___SetCookOperation_1024887225(base.LocalConnection, operation);
			}
		}

		private void RpcWriter___Observers_FinalizeOperation_2166136261()
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

		public void RpcLogic___FinalizeOperation_2166136261()
		{
			if (CurrentCookOperation == null)
			{
				Console.LogWarning("No cook operation to finalize");
				return;
			}
			if (InstanceFinder.IsServer)
			{
				StorableItemInstance productInstance = CurrentCookOperation.Recipe.GetProductInstance(CurrentCookOperation.ProductQuality);
				OutputSlot.AddItem(productInstance);
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Chemical_Operations_Completed", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Chemical_Operations_Completed") + 1f).ToString());
			}
			CurrentCookOperation = null;
			ResetStation();
		}

		private void RpcReader___Observers_FinalizeOperation_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___FinalizeOperation_2166136261();
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
				SendServerRpc(13u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetPlayerUser_3323014238(NetworkObject playerObject)
		{
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
				SendServerRpc(14u, writer, channel, DataOrderType.Default);
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
				SendServerRpc(15u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendTargetRpc(17u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendServerRpc(18u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(19u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendServerRpc(20u, writer, channel, DataOrderType.Default);
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
				SendTargetRpc(21u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendObserversRpc(22u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EChemistryStation(PooledReader PooledReader0, byte UInt321, bool Boolean2)
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

		protected virtual void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EChemistryStation_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			if (isGhost)
			{
				return;
			}
			IngredientSlots = new ItemSlot[3];
			for (int i = 0; i < 3; i++)
			{
				IngredientSlots[i] = new ItemSlot();
				IngredientSlots[i].SetSlotOwner(this);
				InputVisuals.AddSlot(IngredientSlots[i]);
				ItemSlot obj = IngredientSlots[i];
				obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, (Action)delegate
				{
					base.HasChanged = true;
				});
			}
			OutputSlot.SetIsAddLocked(locked: true);
			OutputSlot.SetSlotOwner(this);
			OutputVisuals.AddSlot(OutputSlot);
			ItemSlot outputSlot = OutputSlot;
			outputSlot.onItemDataChanged = (Action)Delegate.Combine(outputSlot.onItemDataChanged, (Action)delegate
			{
				base.HasChanged = true;
			});
			InputSlots.AddRange(IngredientSlots);
			OutputSlots.Add(OutputSlot);
		}
	}
}
