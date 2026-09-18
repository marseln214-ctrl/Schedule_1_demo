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
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees
{
	public class Cleaner : Employee, IConfigurable
	{
		public const int MAX_ASSIGNED_BINS = 3;

		public TrashGrabberDefinition TrashGrabberDef;

		[Header("References")]
		public PickUpTrashBehaviour PickUpTrashBehaviour;

		public EmptyTrashGrabberBehaviour EmptyTrashGrabberBehaviour;

		public BagTrashCanBehaviour BagTrashCanBehaviour;

		public DisposeTrashBagBehaviour DisposeTrashBagBehaviour;

		public Sprite typeIcon;

		[SerializeField]
		protected ConfigurationReplicator configReplicator;

		[Header("UI")]
		public CleanerUIElement WorldspaceUIPrefab;

		public Transform uiPoint;

		[CompilerGenerated]
		[SyncVar]
		public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted;

		public TrashGrabberInstance trashGrabberInstance { get; private set; }

		public EntityConfiguration Configuration => configuration;

		protected CleanerConfiguration configuration { get; set; }

		public ConfigurationReplicator ConfigReplicator => configReplicator;

		public EConfigurableType ConfigurableType => EConfigurableType.Cleaner;

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

		protected override void AssignProperty(ScheduleOne.Property.Property prop)
		{
			base.AssignProperty(prop);
			prop.Configurables.Add(this);
			configuration = new CleanerConfiguration(configReplicator, this, this);
			CreateWorldspaceUI();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
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

		protected override void MinPass()
		{
			base.MinPass();
			if (Singleton<LoadManager>.Instance.IsLoading)
			{
				return;
			}
			if (AnyWorkInProgress())
			{
				MarkIsWorking();
			}
			else if (!(GetBed() == null) && base.PaidForToday)
			{
				if (configuration.binItems.Count == 0)
				{
					SubmitNoWorkReason("I haven't been assigned any trash cans", "You can use your management clipboards to assign trash cans to me.");
					SetIdle(idle: true);
				}
				else if (InstanceFinder.IsServer)
				{
					TryStartNewTask();
				}
			}
		}

		private void TryStartNewTask()
		{
		}

		private TrashContainerItem GetFirstNonFullBin(TrashContainerItem[] bins)
		{
			return bins.FirstOrDefault((TrashContainerItem bin) => bin.Container.NormalizedTrashLevel < 1f);
		}

		public override void SetIdle(bool idle)
		{
			base.SetIdle(idle);
			if (idle && Avatar.CurrentEquippable != null)
			{
				SetEquippable_Return(string.Empty);
			}
		}

		private TrashContainerItem[] GetTrashContainersOrderedByDistance()
		{
			TrashContainerItem[] array = configuration.binItems.ToArray();
			Array.Sort(array, delegate(TrashContainerItem x, TrashContainerItem y)
			{
				float num = Vector3.Distance(x.transform.position, base.transform.position);
				float value = Vector3.Distance(y.transform.position, base.transform.position);
				return num.CompareTo(value);
			});
			return array;
		}

		public override BedItem GetBed()
		{
			return configuration.bedItem;
		}

		private void EnsureTrashGrabberInInventory()
		{
			if (InstanceFinder.IsServer)
			{
				if (base.Inventory._GetItemAmount(TrashGrabberDef.ID) == 0)
				{
					base.Inventory.InsertItem(TrashGrabberDef.GetDefaultInstance());
				}
				trashGrabberInstance = base.Inventory.GetFirstItem(TrashGrabberDef.ID) as TrashGrabberInstance;
			}
		}

		private bool AnyWorkInProgress()
		{
			if (PickUpTrashBehaviour.Active)
			{
				return true;
			}
			if (EmptyTrashGrabberBehaviour.Active)
			{
				return true;
			}
			if (BagTrashCanBehaviour.Active)
			{
				return true;
			}
			if (DisposeTrashBagBehaviour.Active)
			{
				return true;
			}
			if (MoveItemBehaviour.Active)
			{
				return true;
			}
			return false;
		}

		private int GetTrashGrabberAmount()
		{
			return trashGrabberInstance.GetTotalSize();
		}

		public WorldspaceUIElement CreateWorldspaceUI()
		{
			if (WorldspaceUI != null)
			{
				Console.LogWarning(base.gameObject.name + " already has a worldspace UI element!");
			}
			ScheduleOne.Property.Property assignedProperty = base.AssignedProperty;
			if (assignedProperty == null)
			{
				Console.LogError(assignedProperty?.ToString() + " is not a child of a property!");
				return null;
			}
			CleanerUIElement component = UnityEngine.Object.Instantiate(WorldspaceUIPrefab, assignedProperty.WorldspaceUIContainer).GetComponent<CleanerUIElement>();
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
			return new CleanerData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, base.transform.position, base.transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData()).GetJson();
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
			if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, CurrentPlayerConfigurer);
				RegisterServerRpc(37u, RpcReader___Server_SetConfigurer_3323014238);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEmployees_002ECleaner);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEmployees_002ECleanerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetRegistered();
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
				SendServerRpc(37u, writer, channel, DataOrderType.Default);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EEmployees_002ECleaner(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			if (UInt321 == 2)
			{
				if (PooledReader0 == null)
				{
					syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				NetworkObject value = PooledReader0.ReadNetworkObject();
                syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			return false;
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
