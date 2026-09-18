using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using ScheduleOne.Tools;
using ScheduleOne.UI.Compass;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Stations;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class BrickPress : GridItem, IUsable, IItemSlotOwner, ITransitEntity, IConfigurable
	{
		public const int INPUT_SLOT_COUNT = 5;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CNPCUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

		[Header("References")]
		public Transform CameraPosition;

		public Transform CameraPosition_Pouring;

		public Transform CameraPosition_Raising;

		public InteractableObject IntObj;

		public Transform uiPoint;

		public Transform StandPoint;

		public Transform[] accessPoints;

		public StorageVisualizer OutputVisuals;

		public BrickPressContainer Container1;

		public BrickPressContainer Container2;

		public Transform ContainerSpawnPoint;

		public PackagingDefinition BrickPackaging;

		public BoxCollider MouldDetection;

		public BrickPressHandle Handle;

		public Transform PressTransform;

		public Transform PressTransform_Raised;

		public Transform PressTransform_Lowered;

		public Transform PressTransform_Compressed;

		public AudioSourceController SlamSound;

		public ConfigurationReplicator configReplicator;

		[Header("Prefabs")]
		public Draggable FunctionalContainerPrefab;

		[Header("UI")]
		public BrickPressUIElement WorldspaceUIPrefab;

		public Sprite typeIcon;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBrickPressAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBrickPressAssembly_002DCSharp_002Edll_Excuted;

		public bool isOpen => SyncAccessor__003CPlayerUserObject_003Ek__BackingField == Player.Local.NetworkObject;

		public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();


		public NetworkObject NPCUserObject
		{
			get
			{
				return _003CNPCUserObject_003Ek__BackingField;
			}
			set
			{
                _003CNPCUserObject_003Ek__BackingField = value;

                syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(value, true);
            }
		}

		public NetworkObject PlayerUserObject
		{
			get
			{
				return _003CPlayerUserObject_003Ek__BackingField;
			}
			set
			{
                _003CPlayerUserObject_003Ek__BackingField = value;
                syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(value, true);
            }
		}

		public ItemSlot[] ProductSlots { get; private set; }

		public ItemSlot OutputSlot { get; private set; }

		public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();


		public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();


		public Vector3 LinkOriginPoint => uiPoint.position;

		public Transform[] AccessPoints => accessPoints;

		public bool Selectable { get; } = true;


		public bool IsAcceptingItems { get; set; } = true;


		public EntityConfiguration Configuration => stationConfiguration;

		protected BrickPressConfiguration stationConfiguration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.BrickPress;

		public WorldspaceUIElement WorldspaceUI { get; set; }

		public NetworkObject CurrentPlayerConfigurer
		{
			get
			{
				return _003CCurrentPlayerConfigurer_003Ek__BackingField;
			}
			set
			{
                _003CCurrentPlayerConfigurer_003Ek__BackingField = value;
                syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, true);
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
			Awake_UserLogic_ScheduleOne_002EObjectScripts_002EBrickPress_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void InitializeGridItem(ItemInstance instance, ScheduleOne.Tiles.Grid grid, Vector2 originCoordinate, int rotation, string GUID)
		{
			bool initialized = base.Initialized;
			base.InitializeGridItem(instance, grid, originCoordinate, rotation, GUID);
			if (!initialized && !isGhost)
			{
				base.ParentProperty.Configurables.Add(this);
				stationConfiguration = new BrickPressConfiguration(configReplicator, this, this);
				CreateWorldspaceUI();
				GameInput.RegisterExitListener(Exit, 4);
			}
		}

		protected virtual void LateUpdate()
		{
			PressTransform.localPosition = Vector3.Lerp(PressTransform_Raised.localPosition, PressTransform_Lowered.localPosition, Handle.CurrentPosition);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			((IItemSlotOwner)this).SendItemsToClient(connection);
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

		private void Exit(ExitAction action)
		{
			if (!action.used && isOpen && action.exitType == ExitType.Escape)
			{
				action.used = true;
				Close();
			}
		}

		public override bool CanBeDestroyed(out string reason)
		{
			if (((IUsable)this).IsInUse)
			{
				reason = "Brick press is in use";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override void DestroyItem(bool callOnServer = true)
		{
			GameInput.DeregisterExitListener(Exit);
			Configuration.Destroy();
			DestroyWorldspaceUI();
			base.ParentProperty.Configurables.Remove(this);
			base.DestroyItem(callOnServer);
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

		public PackagingStation.EState GetState()
		{
			if (!HasSufficientProduct(out var product))
			{
				return PackagingStation.EState.InsufficentProduct;
			}
			if (OutputSlot.ItemInstance != null)
			{
				if ((OutputSlot.ItemInstance as QualityItemInstance).Quality != product.Quality)
				{
					return PackagingStation.EState.Mismatch;
				}
				if (OutputSlot.ItemInstance.ID != product.ID)
				{
					return PackagingStation.EState.Mismatch;
				}
				if (OutputSlot.ItemInstance.ID != product.ID)
				{
					return PackagingStation.EState.Mismatch;
				}
				if (OutputSlot.ItemInstance.Quantity >= OutputSlot.ItemInstance.StackLimit)
				{
					return PackagingStation.EState.OutputSlotFull;
				}
			}
			return PackagingStation.EState.CanBegin;
		}

		private void UpdateInputVisuals()
		{
			GetMainInputs(out var primaryItem, out var primaryItemQuantity, out var secondaryItem, out var secondaryItemQuantity);
			if (primaryItem != null)
			{
				Container1.SetContents(primaryItem as ProductItemInstance, (float)primaryItemQuantity / 20f);
			}
			else
			{
				Container1.SetContents(null, 0f);
			}
			if (secondaryItem != null)
			{
				Container2.SetContents(secondaryItem as ProductItemInstance, (float)secondaryItemQuantity / 20f);
			}
			else
			{
				Container2.SetContents(null, 0f);
			}
		}

		public bool HasSufficientProduct(out ProductItemInstance product)
		{
			GetMainInputs(out var primaryItem, out var primaryItemQuantity, out var _, out var _);
			if (primaryItem == null)
			{
				product = null;
				return false;
			}
			product = primaryItem as ProductItemInstance;
			return primaryItemQuantity >= 20;
		}

		public void GetMainInputs(out ItemInstance primaryItem, out int primaryItemQuantity, out ItemInstance secondaryItem, out int secondaryItemQuantity)
		{
			List<ItemInstance> list = new List<ItemInstance>();
			Dictionary<ItemInstance, int> itemQuantities = new Dictionary<ItemInstance, int>();
			int i;
			for (i = 0; i < InputSlots.Count; i++)
			{
				if (InputSlots[i].ItemInstance == null)
				{
					continue;
				}
				ItemInstance itemInstance = list.Find((ItemInstance x) => x.ID == InputSlots[i].ItemInstance.ID);
				if (itemInstance == null || !itemInstance.CanStackWith(InputSlots[i].ItemInstance, checkQuantities: false))
				{
					itemInstance = InputSlots[i].ItemInstance;
					list.Add(itemInstance);
					if (!itemQuantities.ContainsKey(InputSlots[i].ItemInstance))
					{
						itemQuantities.Add(InputSlots[i].ItemInstance, 0);
					}
				}
				itemQuantities[itemInstance] += InputSlots[i].Quantity;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (itemQuantities[list[j]] > 20)
				{
					int num = itemQuantities[list[j]] - 20;
					itemQuantities[list[j]] = 20;
					ItemInstance copy = list[j].GetCopy(num);
					list.Add(copy);
					itemQuantities.Add(copy, num);
				}
			}
			list = list.OrderByDescending((ItemInstance x) => itemQuantities[x]).ToList();
			if (list.Count > 0)
			{
				primaryItem = list[0];
				primaryItemQuantity = itemQuantities[list[0]];
			}
			else
			{
				primaryItem = null;
				primaryItemQuantity = 0;
			}
			if (list.Count > 1)
			{
				secondaryItem = list[1];
				secondaryItemQuantity = itemQuantities[list[1]];
			}
			else
			{
				secondaryItem = null;
				secondaryItemQuantity = 0;
			}
		}

		public Draggable CreateFunctionalContainer(ProductItemInstance instance, float productScale, out List<FunctionalProduct> products)
		{
			Draggable draggable = UnityEngine.Object.Instantiate(FunctionalContainerPrefab, NetworkSingleton<GameManager>.Instance.Temp);
			draggable.transform.position = ContainerSpawnPoint.position;
			draggable.transform.rotation = ContainerSpawnPoint.rotation;
			draggable.GetComponent<DraggableConstraint>().SetContainer(base.transform);
			Transform transform = draggable.transform.Find("ProductSpawnPoints");
			ProductDefinition productDefinition = instance.Definition as ProductDefinition;
			products = new List<FunctionalProduct>();
			for (int i = 0; i < 20; i++)
			{
				Transform child = transform.GetChild(i);
				FunctionalProduct functionalProduct = UnityEngine.Object.Instantiate(productDefinition.FunctionalProduct, NetworkSingleton<GameManager>.Instance.Temp);
				functionalProduct.transform.position = child.position;
				functionalProduct.transform.rotation = child.rotation;
				functionalProduct.transform.localScale = Vector3.one * productScale;
				functionalProduct.Initialize(instance);
				products.Add(functionalProduct);
			}
			return draggable;
		}

		public void PlayPressAnim()
		{
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				Handle.Locked = true;
				Handle.SetPosition(1f);
				yield return new WaitForSeconds(0.5f);
				SlamSound.Play();
				yield return new WaitForSeconds(0.5f);
				Handle.Locked = false;
			}
		}

		public void CompletePress(ProductItemInstance product)
		{
			ProductItemInstance productItemInstance = product.GetCopy(1) as ProductItemInstance;
			productItemInstance.SetPackaging(BrickPackaging);
			OutputSlot.AddItem(productItemInstance);
			int num = 20;
			for (int i = 0; i < InputSlots.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				if (InputSlots[i].ItemInstance != null && InputSlots[i].ItemInstance.CanStackWith(product, checkQuantities: false))
				{
					int num2 = Mathf.Min(num, InputSlots[i].Quantity);
					InputSlots[i].ChangeQuantity(-num2);
					num -= num2;
				}
			}
		}

		public List<FunctionalProduct> GetProductInMould()
		{
			Collider[] array = Physics.OverlapBox(MouldDetection.bounds.center, MouldDetection.bounds.extents, MouldDetection.transform.rotation, LayerMask.GetMask("Task"));
			List<FunctionalProduct> list = new List<FunctionalProduct>();
			for (int i = 0; i < array.Length; i++)
			{
				FunctionalProduct componentInParent = array[i].GetComponentInParent<FunctionalProduct>();
				if (componentInParent != null && !list.Contains(componentInParent))
				{
					list.Add(componentInParent);
				}
			}
			return list;
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
			BrickPressUIElement component = UnityEngine.Object.Instantiate(WorldspaceUIPrefab, base.ParentProperty.WorldspaceUIContainer).GetComponent<BrickPressUIElement>();
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

		public void Open()
		{
			SetPlayerUser(Player.Local.NetworkObject);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(65f, 0.2f);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
			Singleton<BrickPressCanvas>.Instance.SetIsOpen(this, open: true);
		}

		public void Close()
		{
			Singleton<BrickPressCanvas>.Instance.SetIsOpen(null, open: false);
			SetPlayerUser(null);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.2f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
			PlayerSingleton<PlayerMovement>.Instance.canMove = true;
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

		public override string GetSaveString()
		{
			return new BrickPressData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, new ItemSet(ItemSlots)).GetJson();
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
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBrickPressAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EBrickPressAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, PlayerUserObject);
				syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, NPCUserObject);
				RegisterServerRpc(8u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterServerRpc(9u, RpcReader___Server_SetPlayerUser_3323014238);
				RegisterServerRpc(10u, RpcReader___Server_SetNPCUser_3323014238);
				RegisterServerRpc(11u, RpcReader___Server_SetStoredInstance_2652194801);
				RegisterObserversRpc(12u, RpcReader___Observers_SetStoredInstance_Internal_2652194801);
				RegisterTargetRpc(13u, RpcReader___Target_SetStoredInstance_Internal_2652194801);
				RegisterServerRpc(14u, RpcReader___Server_SetItemSlotQuantity_1692629761);
				RegisterObserversRpc(15u, RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761);
				RegisterServerRpc(16u, RpcReader___Server_SetSlotLocked_3170825843);
				RegisterTargetRpc(17u, RpcReader___Target_SetSlotLocked_Internal_3170825843);
				RegisterObserversRpc(18u, RpcReader___Observers_SetSlotLocked_Internal_3170825843);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EObjectScripts_002EBrickPress);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBrickPressAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EBrickPressAssembly_002DCSharp_002Edll_Excuted = true;
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
				SendServerRpc(11u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(12u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendTargetRpc(13u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendServerRpc(14u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(15u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendServerRpc(16u, writer, channel, DataOrderType.Default);
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
				SendTargetRpc(17u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendObserversRpc(18u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EBrickPress(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(calledByUser: true), true);
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
					syncVar____003CPlayerUserObject_003Ek__BackingField.SetValue(syncVar____003CPlayerUserObject_003Ek__BackingField.GetValue(calledByUser: true), true);
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
					syncVar____003CNPCUserObject_003Ek__BackingField.SetValue(syncVar____003CNPCUserObject_003Ek__BackingField.GetValue(calledByUser: true), true);
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

		protected virtual void Awake_UserLogic_ScheduleOne_002EObjectScripts_002EBrickPress_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			if (!isGhost)
			{
				ProductSlots = new ItemSlot[5];
				for (int i = 0; i < 5; i++)
				{
					ProductSlots[i] = new ItemSlot();
					ProductSlots[i].SetSlotOwner(this);
					ProductSlots[i].AddFilter(new ItemFilter_UnpackagedProduct());
					ItemSlot obj = ProductSlots[i];
					obj.onItemDataChanged = (Action)Delegate.Combine(obj.onItemDataChanged, new Action(UpdateInputVisuals));
				}
				OutputSlot = new ItemSlot();
				OutputSlot.SetSlotOwner(this);
				OutputSlot.SetIsAddLocked(locked: true);
				OutputVisuals.AddSlot(OutputSlot);
				InputSlots.AddRange(ProductSlots);
				OutputSlots.Add(OutputSlot);
			}
		}
	}
}
