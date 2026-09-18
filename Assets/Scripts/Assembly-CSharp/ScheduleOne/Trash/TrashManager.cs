using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Trash
{
	public class TrashManager : NetworkSingleton<TrashManager>, IBaseSaveable, ISaveable
	{
		[Serializable]
		public class TrashItemData
		{
			public TrashItem Item;

			[Range(0f, 1f)]
			public float GenerationChance = 0.5f;
		}

		public const int TRASH_ITEM_LIMIT = 1000;

		public TrashItem[] TrashPrefabs;

		public TrashItem TrashBagPrefab;

		public TrashItemData[] GenerateableTrashItems;

		private List<TrashItem> trashItems = new List<TrashItem>();

		public float TrashForceMultiplier = 0.3f;

		private TrashLoader loader = new TrashLoader();

		private List<string> writtenItemFiles = new List<string>();

		private bool NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted;

		public string SaveFolderName => "Trash";

		public string SaveFileName => "Trash";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string> { "Items", "Generators" };


		public bool HasChanged { get; set; }

		protected override void Start()
		{
			base.Start();
			InitializeSaveable();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			foreach (TrashItem trashItem in trashItems)
			{
				CreateTrashItem(connection, trashItem.ID, trashItem.transform.position, trashItem.transform.rotation, Vector3.zero, null, trashItem.GUID.ToString());
			}
		}

		public void ReplicateTransformData(TrashItem trash)
		{
			SendTransformData(trash.GUID.ToString(), trash.transform.position, trash.transform.rotation, trash.Rigidbody.velocity, Player.Local.LocalConnection);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendTransformData(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
		{
			RpcWriter___Server_SendTransformData_2990100769(guid, position, rotation, velocity, sender);
		}

		[ObserversRpc]
		private void ReceiveTransformData(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
		{
			RpcWriter___Observers_ReceiveTransformData_2990100769(guid, position, rotation, velocity, sender);
		}

		public TrashItem CreateTrashItem(string id, Vector3 posiiton, Quaternion rotation, Vector3 initialVelocity = default(Vector3), string guid = "", bool startKinematic = false)
		{
			if (guid == "")
			{
				guid = Guid.NewGuid().ToString();
			}
			SendTrashItem(id, posiiton, rotation, initialVelocity, Player.Local.LocalConnection, guid);
			return CreateAndReturnTrashItem(id, posiiton, rotation, initialVelocity, guid, startKinematic);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendTrashItem(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			RpcWriter___Server_SendTrashItem_478112418(id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}

		[ObserversRpc]
		[TargetRpc]
		private void CreateTrashItem(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_CreateTrashItem_2385526393(conn, id, position, rotation, initialVelocity, sender, guid, startKinematic);
			}
			else
			{
				RpcWriter___Target_CreateTrashItem_2385526393(conn, id, position, rotation, initialVelocity, sender, guid, startKinematic);
			}
		}

		private TrashItem CreateAndReturnTrashItem(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, string guid, bool startKinematic)
		{
			TrashItem trashPrefab = GetTrashPrefab(id);
			if (trashPrefab == null)
			{
				Debug.LogError("Trash item with ID " + id + " not found.");
				return null;
			}
			trashPrefab.Draggable.CreateCoM = false;
			trashPrefab.GetComponent<PhysicsDamageable>().ForceMultiplier = TrashForceMultiplier;
			TrashItem trashItem = UnityEngine.Object.Instantiate(trashPrefab, position, rotation, NetworkSingleton<GameManager>.Instance.Temp);
			trashItem.SetGUID(new Guid(guid));
			if (!startKinematic)
			{
				trashItem.SetContinuousCollisionDetection();
			}
			if (initialVelocity != default(Vector3))
			{
				trashItem.SetVelocity(initialVelocity);
			}
			trashItems.Add(trashItem);
			HasChanged = true;
			return trashItem;
		}

		public TrashItem CreateTrashBag(string id, Vector3 posiiton, Quaternion rotation, TrashContentData content, Vector3 initialVelocity = default(Vector3), string guid = "", bool startKinematic = false)
		{
			if (guid == "")
			{
				guid = Guid.NewGuid().ToString();
			}
			SendTrashBag(id, posiiton, rotation, content, initialVelocity, Player.Local.LocalConnection, guid);
			return CreateAndReturnTrashBag(id, posiiton, rotation, content, initialVelocity, guid, startKinematic);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendTrashBag(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			RpcWriter___Server_SendTrashBag_3965031115(id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}

		[ObserversRpc]
		[TargetRpc]
		private void CreateTrashBag(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_CreateTrashBag_680856992(conn, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
			}
			else
			{
				RpcWriter___Target_CreateTrashBag_680856992(conn, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
			}
		}

		private TrashItem CreateAndReturnTrashBag(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, string guid, bool startKinematic)
		{
			TrashBag trashBag = GetTrashPrefab(id) as TrashBag;
			if (trashBag == null)
			{
				Debug.LogError("Trash item with ID " + id + " not found.");
				return null;
			}
			TrashBag trashBag2 = UnityEngine.Object.Instantiate(trashBag, position, rotation, NetworkSingleton<GameManager>.Instance.Temp);
			trashBag2.SetGUID(new Guid(guid));
			trashBag2.LoadContent(content);
			if (!startKinematic)
			{
				trashBag2.SetContinuousCollisionDetection();
			}
			if (initialVelocity != default(Vector3))
			{
				trashBag2.SetVelocity(initialVelocity);
			}
			trashItems.Add(trashBag2);
			HasChanged = true;
			return trashBag2;
		}

		public void DestroyTrash(TrashItem trash)
		{
			SendDestroyTrash(trash.GUID.ToString());
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendDestroyTrash(string guid)
		{
			RpcWriter___Server_SendDestroyTrash_3615296227(guid);
			RpcLogic___SendDestroyTrash_3615296227(guid);
		}

		[ObserversRpc(RunLocally = true)]
		private void DestroyTrash(string guid)
		{
			RpcWriter___Observers_DestroyTrash_3615296227(guid);
			RpcLogic___DestroyTrash_3615296227(guid);
		}

		public TrashItem GetTrashPrefab(string id)
		{
			return TrashPrefabs.FirstOrDefault((TrashItem t) => t.ID == id);
		}

		public TrashItem GetRandomGeneratableTrashPrefab()
		{
			float maxInclusive = GenerateableTrashItems.Sum((TrashItemData t) => t.GenerationChance);
			float num = UnityEngine.Random.Range(0f, maxInclusive);
			TrashItemData[] generateableTrashItems = GenerateableTrashItems;
			foreach (TrashItemData trashItemData in generateableTrashItems)
			{
				if (num < trashItemData.GenerationChance)
				{
					return trashItemData.Item;
				}
				num -= trashItemData.GenerationChance;
			}
			return GenerateableTrashItems[GenerateableTrashItems.Length - 1].Item;
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> result = new List<string>();
			((ISaveable)this).GetContainerFolder(parentFolderPath);
			writtenItemFiles.Clear();
			string parentFolderPath2 = ((ISaveable)this).WriteFolder(parentFolderPath, "Items");
			for (int i = 0; i < trashItems.Count; i++)
			{
				if (trashItems[i].ShouldSave())
				{
					writtenItemFiles.Add(trashItems[i].SaveFileName + ".json");
					new SaveRequest(trashItems[i], parentFolderPath2);
				}
			}
			string parentFolderPath3 = ((ISaveable)this).WriteFolder(parentFolderPath, "Generators");
			foreach (TrashGenerator allGenerator in TrashGenerator.AllGenerators)
			{
				if (allGenerator.ShouldSave() && allGenerator.HasChanged)
				{
					new SaveRequest(allGenerator, parentFolderPath3);
				}
			}
			return result;
		}

		public virtual void DeleteUnapprovedFiles(string parentFolderPath)
		{
			((ISaveable)this).GetContainerFolder(parentFolderPath);
			string[] files = Directory.GetFiles(((ISaveable)this).WriteFolder(parentFolderPath, "Items"));
			for (int i = 0; i < files.Length; i++)
			{
				if (!writtenItemFiles.Contains(files[i]))
				{
					try
					{
						File.Delete(files[i]);
					}
					catch (Exception ex)
					{
						Console.LogError("Failed to delete unapproved file: " + files[i] + " - " + ex.Message);
					}
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_SendTransformData_2990100769);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveTransformData_2990100769);
				RegisterServerRpc(2u, RpcReader___Server_SendTrashItem_478112418);
				RegisterObserversRpc(3u, RpcReader___Observers_CreateTrashItem_2385526393);
				RegisterTargetRpc(4u, RpcReader___Target_CreateTrashItem_2385526393);
				RegisterServerRpc(5u, RpcReader___Server_SendTrashBag_3965031115);
				RegisterObserversRpc(6u, RpcReader___Observers_CreateTrashBag_680856992);
				RegisterTargetRpc(7u, RpcReader___Target_CreateTrashBag_680856992);
				RegisterServerRpc(8u, RpcReader___Server_SendDestroyTrash_3615296227);
				RegisterObserversRpc(9u, RpcReader___Observers_DestroyTrash_3615296227);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ETrash_002ETrashManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
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
				writer.WriteString(guid);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				writer.WriteVector3(velocity);
				writer.WriteNetworkConnection(sender);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
		{
			ReceiveTransformData(guid, position, rotation, velocity, sender);
		}

		private void RpcReader___Server_SendTransformData_2990100769(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string guid = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 velocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendTransformData_2990100769(guid, position, rotation, velocity, sender);
			}
		}

		private void RpcWriter___Observers_ReceiveTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
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
				writer.WriteString(guid);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				writer.WriteVector3(velocity);
				writer.WriteNetworkConnection(sender);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveTransformData_2990100769(string guid, Vector3 position, Quaternion rotation, Vector3 velocity, NetworkConnection sender)
		{
			if (!sender.IsLocalClient)
			{
				TrashItem @object = GUIDManager.GetObject<TrashItem>(new Guid(guid));
				if (!(@object == null))
				{
					@object.transform.position = position;
					@object.transform.rotation = rotation;
					@object.Rigidbody.velocity = velocity;
				}
			}
		}

		private void RpcReader___Observers_ReceiveTransformData_2990100769(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 velocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveTransformData_2990100769(guid, position, rotation, velocity, sender);
			}
		}

		private void RpcWriter___Server_SendTrashItem_478112418(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
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
				writer.WriteString(id);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				writer.WriteVector3(initialVelocity);
				writer.WriteNetworkConnection(sender);
				writer.WriteString(guid);
				writer.WriteBoolean(startKinematic);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendTrashItem_478112418(string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			if (trashItems.Count >= 1000)
			{
				trashItems[UnityEngine.Random.Range(0, trashItems.Count)].DestroyTrash();
			}
			CreateTrashItem(null, id, position, rotation, initialVelocity, sender, guid, startKinematic);
		}

		private void RpcReader___Server_SendTrashItem_478112418(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string id = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 initialVelocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			string guid = PooledReader0.ReadString();
			bool startKinematic = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendTrashItem_478112418(id, position, rotation, initialVelocity, sender, guid, startKinematic);
			}
		}

		private void RpcWriter___Observers_CreateTrashItem_2385526393(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
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
				writer.WriteString(id);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				writer.WriteVector3(initialVelocity);
				writer.WriteNetworkConnection(sender);
				writer.WriteString(guid);
				writer.WriteBoolean(startKinematic);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___CreateTrashItem_2385526393(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			if (!sender.IsLocalClient)
			{
				CreateAndReturnTrashItem(id, position, rotation, initialVelocity, guid, startKinematic);
			}
		}

		private void RpcReader___Observers_CreateTrashItem_2385526393(PooledReader PooledReader0, Channel channel)
		{
			string id = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 initialVelocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			string guid = PooledReader0.ReadString();
			bool startKinematic = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateTrashItem_2385526393(null, id, position, rotation, initialVelocity, sender, guid, startKinematic);
			}
		}

		private void RpcWriter___Target_CreateTrashItem_2385526393(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
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
				writer.WriteString(id);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				writer.WriteVector3(initialVelocity);
				writer.WriteNetworkConnection(sender);
				writer.WriteString(guid);
				writer.WriteBoolean(startKinematic);
				SendTargetRpc(4u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_CreateTrashItem_2385526393(PooledReader PooledReader0, Channel channel)
		{
			string id = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 initialVelocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			string guid = PooledReader0.ReadString();
			bool startKinematic = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateTrashItem_2385526393(base.LocalConnection, id, position, rotation, initialVelocity, sender, guid, startKinematic);
			}
		}

		private void RpcWriter___Server_SendTrashBag_3965031115(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
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
				writer.WriteString(id);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated(writer, content);
				writer.WriteVector3(initialVelocity);
				writer.WriteNetworkConnection(sender);
				writer.WriteString(guid);
				writer.WriteBoolean(startKinematic);
				SendServerRpc(5u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendTrashBag_3965031115(string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			if (trashItems.Count >= 1000)
			{
				trashItems[UnityEngine.Random.Range(0, trashItems.Count)].DestroyTrash();
			}
			CreateTrashBag(null, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
		}

		private void RpcReader___Server_SendTrashBag_3965031115(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string id = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			TrashContentData content = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			Vector3 initialVelocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			string guid = PooledReader0.ReadString();
			bool startKinematic = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendTrashBag_3965031115(id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
			}
		}

		private void RpcWriter___Observers_CreateTrashBag_680856992(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
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
				writer.WriteString(id);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated(writer, content);
				writer.WriteVector3(initialVelocity);
				writer.WriteNetworkConnection(sender);
				writer.WriteString(guid);
				writer.WriteBoolean(startKinematic);
				SendObserversRpc(6u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___CreateTrashBag_680856992(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
		{
			if (!sender.IsLocalClient)
			{
				CreateAndReturnTrashBag(id, position, rotation, content, initialVelocity, guid, startKinematic);
			}
		}

		private void RpcReader___Observers_CreateTrashBag_680856992(PooledReader PooledReader0, Channel channel)
		{
			string id = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			TrashContentData content = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			Vector3 initialVelocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			string guid = PooledReader0.ReadString();
			bool startKinematic = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateTrashBag_680856992(null, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
			}
		}

		private void RpcWriter___Target_CreateTrashBag_680856992(NetworkConnection conn, string id, Vector3 position, Quaternion rotation, TrashContentData content, Vector3 initialVelocity, NetworkConnection sender, string guid, bool startKinematic = false)
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
				writer.WriteString(id);
				writer.WriteVector3(position);
				writer.WriteQuaternion(rotation);
				GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated(writer, content);
				writer.WriteVector3(initialVelocity);
				writer.WriteNetworkConnection(sender);
				writer.WriteString(guid);
				writer.WriteBoolean(startKinematic);
				SendTargetRpc(7u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_CreateTrashBag_680856992(PooledReader PooledReader0, Channel channel)
		{
			string id = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			TrashContentData content = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			Vector3 initialVelocity = PooledReader0.ReadVector3();
			NetworkConnection sender = PooledReader0.ReadNetworkConnection();
			string guid = PooledReader0.ReadString();
			bool startKinematic = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateTrashBag_680856992(base.LocalConnection, id, position, rotation, content, initialVelocity, sender, guid, startKinematic);
			}
		}

		private void RpcWriter___Server_SendDestroyTrash_3615296227(string guid)
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
				writer.WriteString(guid);
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendDestroyTrash_3615296227(string guid)
		{
			DestroyTrash(guid);
		}

		private void RpcReader___Server_SendDestroyTrash_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string guid = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendDestroyTrash_3615296227(guid);
			}
		}

		private void RpcWriter___Observers_DestroyTrash_3615296227(string guid)
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
				writer.WriteString(guid);
				SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___DestroyTrash_3615296227(string guid)
		{
			TrashItem @object = GUIDManager.GetObject<TrashItem>(new Guid(guid));
			if (!(@object == null))
			{
				trashItems.Remove(@object);
				GUIDManager.DeregisterObject(@object);
				if (@object.onDestroyed != null)
				{
					@object.onDestroyed(@object);
				}
				UnityEngine.Object.Destroy(@object.gameObject);
				HasChanged = true;
			}
		}

		private void RpcReader___Observers_DestroyTrash_3615296227(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___DestroyTrash_3615296227(guid);
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
