using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Trash
{
	public class TrashContainer : NetworkBehaviour
	{
		[Header("Settings")]
		[Range(1f, 50f)]
		public int TrashCapacity = 10;

		[Header("Settings")]
		public Transform TrashBagDropLocation;

		public UnityEvent<string> onTrashAdded;

		public UnityEvent onTrashLevelChanged;

		private bool NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted;

		public TrashContent Content { get; protected set; } = new TrashContent();


		public int TrashLevel => Content.GetTotalSize();

		public float NormalizedTrashLevel => (float)Content.GetTotalSize() / (float)TrashCapacity;

		public virtual void AddTrash(TrashItem item)
		{
			SendTrash(item.ID, 1);
			item.DestroyTrash();
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ContainedTrashItems", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("ContainedTrashItems") + 1f).ToString());
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (Content.GetTotalSize() > 0)
			{
				LoadContent(connection, Content.GetData());
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendTrash(string trashID, int quantity)
		{
			RpcWriter___Server_SendTrash_3643459082(trashID, quantity);
			RpcLogic___SendTrash_3643459082(trashID, quantity);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void AddTrash(NetworkConnection conn, string trashID, int quantity)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_AddTrash_3905681115(conn, trashID, quantity);
				RpcLogic___AddTrash_3905681115(conn, trashID, quantity);
			}
			else
			{
				RpcWriter___Target_AddTrash_3905681115(conn, trashID, quantity);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendClear()
		{
			RpcWriter___Server_SendClear_2166136261();
			RpcLogic___SendClear_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		private void Clear()
		{
			RpcWriter___Observers_Clear_2166136261();
			RpcLogic___Clear_2166136261();
		}

		[TargetRpc]
		private void LoadContent(NetworkConnection conn, TrashContentData data)
		{
			RpcWriter___Target_LoadContent_189522235(conn, data);
		}

		public void TriggerEnter(Collider other)
		{
			if (InstanceFinder.IsServer && TrashLevel < TrashCapacity)
			{
				TrashItem componentInParent = other.GetComponentInParent<TrashItem>();
				if (!(componentInParent == null) && componentInParent.CanGoInContainer)
				{
					AddTrash(componentInParent);
				}
			}
		}

		public bool CanBeBagged()
		{
			return TrashLevel > 0;
		}

		public void BagTrash()
		{
			NetworkSingleton<TrashManager>.Instance.CreateTrashBag(NetworkSingleton<TrashManager>.Instance.TrashBagPrefab.ID, TrashBagDropLocation.position, TrashBagDropLocation.rotation, Content.GetData(), TrashBagDropLocation.forward * 3f);
			SendClear();
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("TrashContainersBagged", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("TrashContainersBagged") + 1f).ToString());
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendTrash_3643459082);
				RegisterObserversRpc(1u, RpcReader___Observers_AddTrash_3905681115);
				RegisterTargetRpc(2u, RpcReader___Target_AddTrash_3905681115);
				RegisterServerRpc(3u, RpcReader___Server_SendClear_2166136261);
				RegisterObserversRpc(4u, RpcReader___Observers_Clear_2166136261);
				RegisterTargetRpc(5u, RpcReader___Target_LoadContent_189522235);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ETrash_002ETrashContainerAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendTrash_3643459082(string trashID, int quantity)
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
				writer.WriteString(trashID);
				writer.WriteInt32(quantity);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendTrash_3643459082(string trashID, int quantity)
		{
			AddTrash(null, trashID, quantity);
		}

		private void RpcReader___Server_SendTrash_3643459082(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string trashID = PooledReader0.ReadString();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendTrash_3643459082(trashID, quantity);
			}
		}

		private void RpcWriter___Observers_AddTrash_3905681115(NetworkConnection conn, string trashID, int quantity)
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
				writer.WriteString(trashID);
				writer.WriteInt32(quantity);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___AddTrash_3905681115(NetworkConnection conn, string trashID, int quantity)
		{
			Content.AddTrash(trashID, quantity);
			if (onTrashAdded != null)
			{
				onTrashAdded.Invoke(trashID);
			}
			if (onTrashLevelChanged != null)
			{
				onTrashLevelChanged.Invoke();
			}
		}

		private void RpcReader___Observers_AddTrash_3905681115(PooledReader PooledReader0, Channel channel)
		{
			string trashID = PooledReader0.ReadString();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___AddTrash_3905681115(null, trashID, quantity);
			}
		}

		private void RpcWriter___Target_AddTrash_3905681115(NetworkConnection conn, string trashID, int quantity)
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
				writer.WriteString(trashID);
				writer.WriteInt32(quantity);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_AddTrash_3905681115(PooledReader PooledReader0, Channel channel)
		{
			string trashID = PooledReader0.ReadString();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddTrash_3905681115(base.LocalConnection, trashID, quantity);
			}
		}

		private void RpcWriter___Server_SendClear_2166136261()
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
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendClear_2166136261()
		{
			Clear();
		}

		private void RpcReader___Server_SendClear_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendClear_2166136261();
			}
		}

		private void RpcWriter___Observers_Clear_2166136261()
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

		private void RpcLogic___Clear_2166136261()
		{
			Content.Clear();
			if (onTrashLevelChanged != null)
			{
				onTrashLevelChanged.Invoke();
			}
		}

		private void RpcReader___Observers_Clear_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Clear_2166136261();
			}
		}

		private void RpcWriter___Target_LoadContent_189522235(NetworkConnection conn, TrashContentData data)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated(writer, data);
				SendTargetRpc(5u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcLogic___LoadContent_189522235(NetworkConnection conn, TrashContentData data)
		{
			Content.LoadFromData(data);
			if (onTrashLevelChanged != null)
			{
				onTrashLevelChanged.Invoke();
			}
		}

		private void RpcReader___Target_LoadContent_189522235(PooledReader PooledReader0, Channel channel)
		{
			TrashContentData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___LoadContent_189522235(base.LocalConnection, data);
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
