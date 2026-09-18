using System;
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
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Storage;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.ObjectScripts
{
	public class PlaceableStorageEntity : GridItem, ITransitEntity, IStorageEntity, IUsable
	{
		[Header("Reference")]
		[SerializeField]
		protected Transform _storedItemContainer;

		public StorageEntity StorageEntity;

		[SerializeField]
		protected List<StorageGrid> storageGrids = new List<StorageGrid>();

		public Transform[] accessPoints;

		protected Dictionary<StoredItem, Employee> _reservedItems = new Dictionary<StoredItem, Employee>();

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CNPCUserObject_003Ek__BackingField;

		[CompilerGenerated]
		[HideInInspector]
		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
		public NetworkObject _003CPlayerUserObject_003Ek__BackingField;

		private List<string> completedJobs = new List<string>();

		public SyncVar<NetworkObject> syncVar____003CNPCUserObject_003Ek__BackingField;

		public SyncVar<NetworkObject> syncVar____003CPlayerUserObject_003Ek__BackingField;

		private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPlaceableStorageEntityAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPlaceableStorageEntityAssembly_002DCSharp_002Edll_Excuted;

		public Transform storedItemContainer => _storedItemContainer;

		public Dictionary<StoredItem, Employee> reservedItems
		{
			get
			{
				return _reservedItems;
			}
			set
			{
				_reservedItems = value;
			}
		}

		public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();


		public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();


		public Vector3 LinkOriginPoint => base.transform.position;

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

		public Transform[] AccessPoints => accessPoints;

		public bool Selectable { get; } = true;


		public bool IsAcceptingItems { get; set; } = true;


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

		protected override void Start()
		{
			base.Start();
			for (int i = 0; i < StorageEntity.ItemSlots.Count; i++)
			{
				InputSlots.Add(StorageEntity.ItemSlots[i]);
				OutputSlots.Add(StorageEntity.ItemSlots[i]);
			}
		}

		public List<StoredItem> GetStoredItems()
		{
			return new List<StoredItem>(storedItemContainer.GetComponentsInChildren<StoredItem>());
		}

		public List<StorageGrid> GetStorageGrids()
		{
			return storageGrids;
		}

		[ObserversRpc(RunLocally = true)]
		public void DestroyStoredItem(int gridIndex, Coordinate coord, string jobID = "", bool network = true)
		{
			RpcWriter___Observers_DestroyStoredItem_3261517793(gridIndex, coord, jobID, network);
			RpcLogic___DestroyStoredItem_3261517793(gridIndex, coord, jobID, network);
		}

		[ServerRpc(RequireOwnership = false)]
		private void DestroyStoredItem_Server(int gridIndex, Coordinate coord, string jobID)
		{
			RpcWriter___Server_DestroyStoredItem_Server_3952619116(gridIndex, coord, jobID);
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

		public override bool CanBeDestroyed(out string reason)
		{
			if (StorageEntity.CurrentAccessor != null)
			{
				reason = "In use by other player";
				return false;
			}
			if (StorageEntity.ItemCount > 0)
			{
				reason = "Contains items";
				return false;
			}
			return base.CanBeDestroyed(out reason);
		}

		public override string GetSaveString()
		{
			return new PlaceableStorageData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, new ItemSet(StorageEntity.ItemSlots)).GetJson();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPlaceableStorageEntityAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002EPlaceableStorageEntityAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar____003CPlayerUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, PlayerUserObject);
				syncVar____003CNPCUserObject_003Ek__BackingField = new SyncVar<NetworkObject>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, NPCUserObject);
				RegisterObserversRpc(8u, RpcReader___Observers_DestroyStoredItem_3261517793);
				RegisterServerRpc(9u, RpcReader___Server_DestroyStoredItem_Server_3952619116);
				RegisterServerRpc(10u, RpcReader___Server_SetPlayerUser_3323014238);
				RegisterServerRpc(11u, RpcReader___Server_SetNPCUser_3323014238);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EObjectScripts_002EPlaceableStorageEntity);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPlaceableStorageEntityAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EObjectScripts_002EPlaceableStorageEntityAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar____003CPlayerUserObject_003Ek__BackingField.SetRegistered();
				syncVar____003CNPCUserObject_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Observers_DestroyStoredItem_3261517793(int gridIndex, Coordinate coord, string jobID = "", bool network = true)
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
				writer.WriteInt32(gridIndex);
				GeneratedWriters___Internal.Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated(writer, coord);
				writer.WriteString(jobID);
				writer.WriteBoolean(network);
				SendObserversRpc(8u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___DestroyStoredItem_3261517793(int gridIndex, Coordinate coord, string jobID = "", bool network = true)
		{
			if (jobID != "")
			{
				if (completedJobs.Contains(jobID))
				{
					return;
				}
			}
			else
			{
				jobID = Guid.NewGuid().ToString();
			}
			completedJobs.Add(jobID);
			List<StorageGrid> list = GetStorageGrids();
			if (gridIndex > list.Count)
			{
				Console.LogError("DestroyStoredItem: grid index out of range");
				return;
			}
			if (list[gridIndex].GetTile(coord) == null)
			{
				Console.LogError("DestroyStoredItem: no tile found at " + coord);
				return;
			}
			list[gridIndex].GetTile(coord).occupant.Destroy_Internal();
			if (network)
			{
				DestroyStoredItem_Server(gridIndex, coord, jobID);
			}
		}

		private void RpcReader___Observers_DestroyStoredItem_3261517793(PooledReader PooledReader0, Channel channel)
		{
			int gridIndex = PooledReader0.ReadInt32();
			Coordinate coord = GeneratedReaders___Internal.Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string jobID = PooledReader0.ReadString();
			bool network = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___DestroyStoredItem_3261517793(gridIndex, coord, jobID, network);
			}
		}

		private void RpcWriter___Server_DestroyStoredItem_Server_3952619116(int gridIndex, Coordinate coord, string jobID)
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
				writer.WriteInt32(gridIndex);
				GeneratedWriters___Internal.Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated(writer, coord);
				writer.WriteString(jobID);
				SendServerRpc(9u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___DestroyStoredItem_Server_3952619116(int gridIndex, Coordinate coord, string jobID)
		{
			DestroyStoredItem(gridIndex, coord, jobID, network: false);
		}

		private void RpcReader___Server_DestroyStoredItem_Server_3952619116(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int gridIndex = PooledReader0.ReadInt32();
			Coordinate coord = GeneratedReaders___Internal.Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string jobID = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___DestroyStoredItem_Server_3952619116(gridIndex, coord, jobID);
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
				SendServerRpc(10u, writer, channel, DataOrderType.Default);
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
				SendServerRpc(11u, writer, channel, DataOrderType.Default);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EObjectScripts_002EPlaceableStorageEntity(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
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

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
