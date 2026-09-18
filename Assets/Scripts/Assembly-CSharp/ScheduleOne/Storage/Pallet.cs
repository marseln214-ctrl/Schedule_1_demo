using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Storage
{
	public class Pallet : NetworkBehaviour, IStorageEntity
	{
		public static List<Pallet> palletsOwnedByLocalPlayer = new List<Pallet>();

		public static int sizeX = 6;

		public static int sizeY = 6;

		[Header("Reference")]
		public Transform _storedItemContainer;

		public Rigidbody rb;

		public StorageGrid storageGrid;

		public NetworkTransform networkTransform;

		protected List<Forklift> forkliftsInContact = new List<Forklift>();

		public Guid currentSlotGUID;

		private PalletSlot currentSlot;

		private float timeSinceSlotCheck;

		private float timeBoundToSlot;

		private float rb_Mass;

		private float rb_Drag;

		private float rb_AngularDrag;

		protected Dictionary<StoredItem, Employee> _reservedItems = new Dictionary<StoredItem, Employee>();

		private List<string> completedJobs = new List<string>();

		private bool NetworkInitialize___EarlyScheduleOne_002EStorage_002EPalletAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EStorage_002EPalletAssembly_002DCSharp_002Edll_Excuted;

		public bool isEmpty => _storedItemContainer.childCount == 0;

		protected bool carriedByForklift => forkliftsInContact.Count > 0;

		public Transform storedItemContainer => _storedItemContainer;

		public Dictionary<StoredItem, Employee> reservedItems => _reservedItems;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EStorage_002EPallet_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnStartServer()
		{
			base.OnStartServer();
			if (currentSlot == null)
			{
				rb.isKinematic = false;
				rb.interpolation = RigidbodyInterpolation.Interpolate;
			}
		}

		[ServerRpc(RequireOwnership = false)]
		protected virtual void SetOwner(NetworkConnection conn)
		{
			RpcWriter___Server_SetOwner_328543758(conn);
		}

		public override void OnOwnershipClient(NetworkConnection prevOwner)
		{
			base.OnOwnershipClient(prevOwner);
			if (base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost))
			{
				if (rb != null)
				{
					rb.interpolation = RigidbodyInterpolation.Interpolate;
					rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
					rb.isKinematic = false;
				}
				if (!palletsOwnedByLocalPlayer.Contains(this))
				{
					palletsOwnedByLocalPlayer.Add(this);
				}
			}
			else
			{
				if (rb != null)
				{
					rb.interpolation = RigidbodyInterpolation.None;
					rb.isKinematic = true;
				}
				if (palletsOwnedByLocalPlayer.Contains(this))
				{
					palletsOwnedByLocalPlayer.Remove(this);
				}
			}
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			SendItemsToClient(connection);
			if (currentSlot != null)
			{
				BindToSlot(connection, currentSlot.GUID);
			}
		}

		private void SendItemsToClient(NetworkConnection connection)
		{
			StoredItem[] componentsInChildren = _storedItemContainer.GetComponentsInChildren<StoredItem>();
			List<StorageGrid> storageGrids = GetStorageGrids();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				CreateStoredItem(connection, componentsInChildren[i].item, storageGrids.IndexOf(componentsInChildren[i].parentGrid), componentsInChildren[i].CoordinatePairs[0].coord2, componentsInChildren[i].Rotation, "", network: false);
			}
		}

		public virtual void DestroyPallet()
		{
			Despawn();
		}

		protected virtual void Update()
		{
			timeSinceSlotCheck += Time.deltaTime;
			if (currentSlot == null)
			{
				timeBoundToSlot = 0f;
			}
			else
			{
				timeBoundToSlot += Time.deltaTime;
			}
		}

		protected virtual void FixedUpdate()
		{
			if (base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost))
			{
				if (carriedByForklift)
				{
					if (currentSlot != null && timeBoundToSlot > 1f)
					{
						Console.Log("Exiting");
						ExitSlot_Server();
					}
				}
				else if (currentSlot == null && timeSinceSlotCheck >= 0.5f)
				{
					timeSinceSlotCheck = 0f;
					Collider[] array = Physics.OverlapSphere(base.transform.position, 0.3f, 1 << LayerMask.NameToLayer("Pallet"), QueryTriggerInteraction.Collide);
					for (int i = 0; i < array.Length; i++)
					{
						PalletSlot componentInParent = array[i].gameObject.GetComponentInParent<PalletSlot>();
						if (componentInParent != null && componentInParent.occupant == null)
						{
							BindToSlot_Server(componentInParent.GUID);
							break;
						}
					}
				}
				if (base.transform.position.y < -20f && currentSlot == null)
				{
					if (rb != null)
					{
						rb.velocity = Vector3.zero;
						rb.angularVelocity = Vector3.zero;
					}
					float y = 0f;
					if (MapHeightSampler.Sample(base.transform.position.x, out y, base.transform.position.z))
					{
						SetPosition(new Vector3(base.transform.position.x, y + 3f, base.transform.position.z));
					}
					else
					{
						SetPosition(MapHeightSampler.ResetPosition);
					}
				}
			}
			UpdateOwnership();
			forkliftsInContact.Clear();
		}

		private void SetPosition(Vector3 position)
		{
			base.transform.position = position;
		}

		private void UpdateOwnership()
		{
			if (forkliftsInContact.Count == 0)
			{
				if (base.IsOwner && !InstanceFinder.IsHost)
				{
					base.NetworkObject.SetLocalOwnership(null);
					SetOwner(null);
				}
				return;
			}
			NetworkConnection owner = forkliftsInContact[0].Owner;
			if (base.Owner != owner && owner == Player.Local.Connection)
			{
				base.NetworkObject.SetLocalOwnership(Player.Local.Connection);
				SetOwner(Player.Local.Connection);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void BindToSlot_Server(Guid slotGuid)
		{
			RpcWriter___Server_BindToSlot_Server_1272046255(slotGuid);
			RpcLogic___BindToSlot_Server_1272046255(slotGuid);
		}

		[ObserversRpc]
		[TargetRpc]
		private void BindToSlot(NetworkConnection conn, Guid slotGuid)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_BindToSlot_454078614(conn, slotGuid);
			}
			else
			{
				RpcWriter___Target_BindToSlot_454078614(conn, slotGuid);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void ExitSlot_Server()
		{
			RpcWriter___Server_ExitSlot_Server_2166136261();
			RpcLogic___ExitSlot_Server_2166136261();
		}

		[ObserversRpc]
		private void ExitSlot()
		{
			RpcWriter___Observers_ExitSlot_2166136261();
		}

		public void TriggerStay(Collider other)
		{
			Forklift forklift = other.gameObject.GetComponentInParent<Forklift>();
			if (forklift == null)
			{
				ForkliftFork componentInParent = other.gameObject.GetComponentInParent<ForkliftFork>();
				if (componentInParent != null)
				{
					forklift = componentInParent.forklift;
				}
			}
			if (other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast") && forklift != null && !forkliftsInContact.Contains(forklift))
			{
				forkliftsInContact.Add(forklift);
			}
		}

		public List<StoredItem> GetStoredItems()
		{
			return new List<StoredItem>(storedItemContainer.GetComponentsInChildren<StoredItem>());
		}

		public List<StorageGrid> GetStorageGrids()
		{
			return new List<StorageGrid> { storageGrid };
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void CreateStoredItem(NetworkConnection conn, StorableItemInstance item, int gridIndex, Vector2 originCoord, float rotation, string jobID = "", bool network = true)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_CreateStoredItem_913707843(conn, item, gridIndex, originCoord, rotation, jobID, network);
				RpcLogic___CreateStoredItem_913707843(conn, item, gridIndex, originCoord, rotation, jobID, network);
			}
			else
			{
				RpcWriter___Target_CreateStoredItem_913707843(conn, item, gridIndex, originCoord, rotation, jobID, network);
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void CreateStoredItem_Server(StorableItemInstance data, int gridIndex, Vector2 originCoord, float rotation, string jobID)
		{
			RpcWriter___Server_CreateStoredItem_Server_1890711751(data, gridIndex, originCoord, rotation, jobID);
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

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EStorage_002EPalletAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EStorage_002EPalletAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SetOwner_328543758);
				RegisterServerRpc(1u, RpcReader___Server_BindToSlot_Server_1272046255);
				RegisterObserversRpc(2u, RpcReader___Observers_BindToSlot_454078614);
				RegisterTargetRpc(3u, RpcReader___Target_BindToSlot_454078614);
				RegisterServerRpc(4u, RpcReader___Server_ExitSlot_Server_2166136261);
				RegisterObserversRpc(5u, RpcReader___Observers_ExitSlot_2166136261);
				RegisterObserversRpc(6u, RpcReader___Observers_CreateStoredItem_913707843);
				RegisterTargetRpc(7u, RpcReader___Target_CreateStoredItem_913707843);
				RegisterServerRpc(8u, RpcReader___Server_CreateStoredItem_Server_1890711751);
				RegisterObserversRpc(9u, RpcReader___Observers_DestroyStoredItem_3261517793);
				RegisterServerRpc(10u, RpcReader___Server_DestroyStoredItem_Server_3952619116);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EStorage_002EPalletAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EStorage_002EPalletAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
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
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		protected virtual void RpcLogic___SetOwner_328543758(NetworkConnection conn)
		{
			Console.Log("Setting pallet owner to: " + conn.ClientId);
			if (base.Owner != null && Player.GetPlayer(base.Owner) != null)
			{
				Player.GetPlayer(base.Owner).objectsTemporarilyOwnedByPlayer.Remove(base.NetworkObject);
			}
			if (conn != null && Player.GetPlayer(conn) != null)
			{
				Player.GetPlayer(conn).objectsTemporarilyOwnedByPlayer.Add(base.NetworkObject);
			}
			base.NetworkObject.GiveOwnership(conn);
		}

		private void RpcReader___Server_SetOwner_328543758(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			if (base.IsServerInitialized)
			{
				RpcLogic___SetOwner_328543758(conn2);
			}
		}

		private void RpcWriter___Server_BindToSlot_Server_1272046255(Guid slotGuid)
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
				writer.WriteGuidAllocated(slotGuid);
				SendServerRpc(1u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___BindToSlot_Server_1272046255(Guid slotGuid)
		{
			BindToSlot(null, slotGuid);
		}

		private void RpcReader___Server_BindToSlot_Server_1272046255(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			Guid slotGuid = PooledReader0.ReadGuid();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___BindToSlot_Server_1272046255(slotGuid);
			}
		}

		private void RpcWriter___Observers_BindToSlot_454078614(NetworkConnection conn, Guid slotGuid)
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
				writer.WriteGuidAllocated(slotGuid);
				SendObserversRpc(2u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___BindToSlot_454078614(NetworkConnection conn, Guid slotGuid)
		{
			currentSlotGUID = slotGuid;
			currentSlot = GUIDManager.GetObject<PalletSlot>(slotGuid);
			if (currentSlot == null)
			{
				currentSlotGUID = Guid.Empty;
				Console.LogWarning("BindToSlot called but slotGuid is not valid");
				return;
			}
			currentSlot.SetOccupant(this);
			networkTransform.enabled = false;
			UnityEngine.Object.Destroy(rb);
			base.transform.SetParent(currentSlot.transform);
			base.transform.position = currentSlot.transform.position + currentSlot.transform.up * 0.1f;
			Vector3 vector = currentSlot.transform.forward;
			if (Vector3.Angle(base.transform.forward, -currentSlot.transform.forward) < Vector3.Angle(base.transform.forward, vector))
			{
				vector = -currentSlot.transform.forward;
			}
			if (Vector3.Angle(base.transform.forward, currentSlot.transform.right) < Vector3.Angle(base.transform.forward, vector))
			{
				vector = currentSlot.transform.right;
			}
			if (Vector3.Angle(base.transform.forward, -currentSlot.transform.right) < Vector3.Angle(base.transform.forward, vector))
			{
				vector = -currentSlot.transform.right;
			}
			base.transform.rotation = Quaternion.LookRotation(vector, Vector3.up);
			base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		}

		private void RpcReader___Observers_BindToSlot_454078614(PooledReader PooledReader0, Channel channel)
		{
			Guid slotGuid = PooledReader0.ReadGuid();
			if (base.IsClientInitialized)
			{
				RpcLogic___BindToSlot_454078614(null, slotGuid);
			}
		}

		private void RpcWriter___Target_BindToSlot_454078614(NetworkConnection conn, Guid slotGuid)
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
				writer.WriteGuidAllocated(slotGuid);
				SendTargetRpc(3u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_BindToSlot_454078614(PooledReader PooledReader0, Channel channel)
		{
			Guid slotGuid = PooledReader0.ReadGuid();
			if (base.IsClientInitialized)
			{
				RpcLogic___BindToSlot_454078614(base.LocalConnection, slotGuid);
			}
		}

		private void RpcWriter___Server_ExitSlot_Server_2166136261()
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
				SendServerRpc(4u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___ExitSlot_Server_2166136261()
		{
			ExitSlot();
		}

		private void RpcReader___Server_ExitSlot_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___ExitSlot_Server_2166136261();
			}
		}

		private void RpcWriter___Observers_ExitSlot_2166136261()
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
				SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ExitSlot_2166136261()
		{
			if (!(currentSlot == null))
			{
				currentSlot.SetOccupant(null);
				base.transform.SetParent(null);
				if (rb == null)
				{
					rb = base.gameObject.AddComponent<Rigidbody>();
				}
				rb.mass = rb_Mass;
				rb.drag = rb_Drag;
				rb.angularDrag = rb_AngularDrag;
				rb.interpolation = RigidbodyInterpolation.Interpolate;
				if (base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost))
				{
					Console.Log("Exit slot, owner");
					rb.isKinematic = false;
					rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
				}
				else
				{
					Console.Log("Exit slot, not owner");
					rb.isKinematic = true;
					rb.interpolation = RigidbodyInterpolation.None;
				}
				networkTransform.enabled = true;
				currentSlotGUID = default(Guid);
				currentSlot = null;
			}
		}

		private void RpcReader___Observers_ExitSlot_2166136261(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___ExitSlot_2166136261();
			}
		}

		private void RpcWriter___Observers_CreateStoredItem_913707843(NetworkConnection conn, StorableItemInstance item, int gridIndex, Vector2 originCoord, float rotation, string jobID = "", bool network = true)
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
				writer.WriteStorableItemInstance(item);
				writer.WriteInt32(gridIndex);
				writer.WriteVector2(originCoord);
				writer.WriteSingle(rotation);
				writer.WriteString(jobID);
				writer.WriteBoolean(network);
				SendObserversRpc(6u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___CreateStoredItem_913707843(NetworkConnection conn, StorableItemInstance item, int gridIndex, Vector2 originCoord, float rotation, string jobID = "", bool network = true)
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
			UnityEngine.Object.Instantiate(item.StoredItem, storedItemContainer).GetComponent<StoredItem>();
			if (network)
			{
				CreateStoredItem_Server(item, gridIndex, originCoord, rotation, jobID);
			}
		}

		private void RpcReader___Observers_CreateStoredItem_913707843(PooledReader PooledReader0, Channel channel)
		{
			StorableItemInstance item = PooledReader0.ReadStorableItemInstance();
			int gridIndex = PooledReader0.ReadInt32();
			Vector2 originCoord = PooledReader0.ReadVector2();
			float rotation = PooledReader0.ReadSingle();
			string jobID = PooledReader0.ReadString();
			bool network = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___CreateStoredItem_913707843(null, item, gridIndex, originCoord, rotation, jobID, network);
			}
		}

		private void RpcWriter___Target_CreateStoredItem_913707843(NetworkConnection conn, StorableItemInstance item, int gridIndex, Vector2 originCoord, float rotation, string jobID = "", bool network = true)
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
				writer.WriteStorableItemInstance(item);
				writer.WriteInt32(gridIndex);
				writer.WriteVector2(originCoord);
				writer.WriteSingle(rotation);
				writer.WriteString(jobID);
				writer.WriteBoolean(network);
				SendTargetRpc(7u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_CreateStoredItem_913707843(PooledReader PooledReader0, Channel channel)
		{
			StorableItemInstance item = PooledReader0.ReadStorableItemInstance();
			int gridIndex = PooledReader0.ReadInt32();
			Vector2 originCoord = PooledReader0.ReadVector2();
			float rotation = PooledReader0.ReadSingle();
			string jobID = PooledReader0.ReadString();
			bool network = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateStoredItem_913707843(base.LocalConnection, item, gridIndex, originCoord, rotation, jobID, network);
			}
		}

		private void RpcWriter___Server_CreateStoredItem_Server_1890711751(StorableItemInstance data, int gridIndex, Vector2 originCoord, float rotation, string jobID)
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
				writer.WriteStorableItemInstance(data);
				writer.WriteInt32(gridIndex);
				writer.WriteVector2(originCoord);
				writer.WriteSingle(rotation);
				writer.WriteString(jobID);
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___CreateStoredItem_Server_1890711751(StorableItemInstance data, int gridIndex, Vector2 originCoord, float rotation, string jobID)
		{
			CreateStoredItem(null, data, gridIndex, originCoord, rotation, jobID, network: false);
		}

		private void RpcReader___Server_CreateStoredItem_Server_1890711751(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			StorableItemInstance data = PooledReader0.ReadStorableItemInstance();
			int gridIndex = PooledReader0.ReadInt32();
			Vector2 originCoord = PooledReader0.ReadVector2();
			float rotation = PooledReader0.ReadSingle();
			string jobID = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___CreateStoredItem_Server_1890711751(data, gridIndex, originCoord, rotation, jobID);
			}
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
				SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
			List<StorageGrid> storageGrids = GetStorageGrids();
			if (gridIndex > storageGrids.Count)
			{
				Console.LogError("DestroyStoredItem: grid index out of range");
				return;
			}
			if (storageGrids[gridIndex].GetTile(coord) == null)
			{
				Console.LogError("DestroyStoredItem: no tile found at " + coord);
				return;
			}
			storageGrids[gridIndex].GetTile(coord).occupant.Destroy_Internal();
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
				SendServerRpc(10u, writer, channel, DataOrderType.Default);
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

		protected virtual void Awake_UserLogic_ScheduleOne_002EStorage_002EPallet_Assembly_002DCSharp_002Edll()
		{
			rb_Mass = rb.mass;
			rb_Drag = rb.drag;
			rb_AngularDrag = rb.angularDrag;
		}
	}
}
