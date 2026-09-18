using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.Dragging
{
	public class DragManager : NetworkSingleton<DragManager>
	{
		public const float DRAGGABLE_OFFSET = 1.25f;

		public AudioSourceController ThrowSound;

		[Header("Settings")]
		public float DragForce = 10f;

		public float DampingFactor = 0.5f;

		public float TorqueForce = 10f;

		public float TorqueDampingFactor = 0.5f;

		public float ThrowForce = 10f;

		public float MassInfluence = 0.6f;

		private List<Draggable> AllDraggables = new List<Draggable>();

		private Draggable lastThrownDraggable;

		private Draggable lastHeldDraggable;

		private bool NetworkInitialize___EarlyScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted;

		public Draggable CurrentDraggable { get; protected set; }

		public bool IsDragging => CurrentDraggable != null;

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			foreach (Draggable allDraggable in AllDraggables)
			{
				if (allDraggable.InitialReplicationMode != 0 && (allDraggable.InitialReplicationMode == Draggable.EInitialReplicationMode.Full || Vector3.Distance(allDraggable.initialPosition, allDraggable.transform.position) > 1f))
				{
					SetDraggableTransformData(connection, allDraggable.GUID.ToString(), allDraggable.transform.position, allDraggable.transform.rotation, allDraggable.Rigidbody.velocity);
				}
			}
		}

		public void Update()
		{
			if (IsDragging)
			{
				bool flag = false;
				LayerMask layerMask = default(LayerMask);
				layerMask.value = 1 << LayerMask.NameToLayer("Default");
				layerMask.value |= 1 << LayerMask.NameToLayer("NPC");
				if (Physics.Raycast(PlayerSingleton<PlayerMovement>.Instance.transform.position - PlayerSingleton<PlayerMovement>.Instance.Controller.height * Vector3.up * 0.5f, Vector3.down, out var hitInfo, 0.5f, layerMask))
				{
					flag = hitInfo.collider.GetComponentInParent<Draggable>() == CurrentDraggable;
				}
				if (GameInput.GetButtonDown(GameInput.ButtonCode.Interact) || !IsDraggingAllowed() || Vector3.Distance(GetTargetPosition(), CurrentDraggable.transform.position) > 1.5f || flag)
				{
					StopDragging(CurrentDraggable.Rigidbody.velocity);
				}
				else if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
				{
					Vector3 vector = PlayerSingleton<PlayerCamera>.Instance.transform.forward * ThrowForce;
					float num = Mathf.Lerp(1f, Mathf.Sqrt(CurrentDraggable.Rigidbody.mass), MassInfluence);
					Vector3 velocity = CurrentDraggable.Rigidbody.velocity + vector / num;
					CurrentDraggable.Rigidbody.velocity = velocity;
					lastThrownDraggable = CurrentDraggable;
					ThrowSound.transform.position = lastThrownDraggable.transform.position;
					float value = Mathf.Sqrt(CurrentDraggable.Rigidbody.mass / 30f);
					ThrowSound.VolumeMultiplier = Mathf.Clamp(value, 0.4f, 1f);
					ThrowSound.PitchMultiplier = Mathf.Lerp(0.6f, 0.4f, Mathf.Clamp01(value));
					ThrowSound.Play();
					StopDragging(velocity);
				}
			}
		}

		public void FixedUpdate()
		{
			if (lastThrownDraggable != null)
			{
				ThrowSound.transform.position = lastThrownDraggable.transform.position;
			}
			if (IsDragging)
			{
				CurrentDraggable.ApplyDragForces(GetTargetPosition());
			}
		}

		public bool IsDraggingAllowed()
		{
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				return false;
			}
			if (!Player.Local.Health.IsAlive)
			{
				return false;
			}
			if (Player.Local.IsSkating)
			{
				return false;
			}
			if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
			{
				if (PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.ID == "trashgrabber")
				{
					return false;
				}
				if (PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.ID == "trashbag" && TrashBag_Equippable.IsHoveringTrash)
				{
					return false;
				}
			}
			return true;
		}

		public void RegisterDraggable(Draggable draggable)
		{
			if (!AllDraggables.Contains(draggable))
			{
				AllDraggables.Add(draggable);
			}
		}

		public void Deregister(Draggable draggable)
		{
			if (AllDraggables.Contains(draggable))
			{
				AllDraggables.Remove(draggable);
			}
		}

		public void StartDragging(Draggable draggable)
		{
			if (CurrentDraggable != null)
			{
				CurrentDraggable.StopDragging();
			}
			CurrentDraggable = draggable;
			lastHeldDraggable = draggable;
			draggable.StartDragging(Player.Local);
			SendDragger(draggable.GUID.ToString(), Player.Local.NetworkObject, draggable.transform.position);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SendDragger(string draggableGUID, NetworkObject dragger, Vector3 position)
		{
			RpcWriter___Server_SendDragger_807933219(draggableGUID, dragger, position);
		}

		[ObserversRpc]
		private void SetDragger(string draggableGUID, NetworkObject dragger, Vector3 position)
		{
			RpcWriter___Observers_SetDragger_807933219(draggableGUID, dragger, position);
		}

		public void StopDragging(Vector3 velocity)
		{
			if (CurrentDraggable != null)
			{
				CurrentDraggable.StopDragging();
				SendDragger(CurrentDraggable.GUID.ToString(), null, CurrentDraggable.transform.position);
				SendDraggableTransformData(CurrentDraggable.GUID.ToString(), CurrentDraggable.transform.position, CurrentDraggable.transform.rotation, velocity);
				CurrentDraggable = null;
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void SendDraggableTransformData(string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
		{
			RpcWriter___Server_SendDraggableTransformData_4062762274(guid, position, rotation, velocity);
			RpcLogic___SendDraggableTransformData_4062762274(guid, position, rotation, velocity);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void SetDraggableTransformData(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetDraggableTransformData_3831223955(conn, guid, position, rotation, velocity);
				RpcLogic___SetDraggableTransformData_3831223955(conn, guid, position, rotation, velocity);
			}
			else
			{
				RpcWriter___Target_SetDraggableTransformData_3831223955(conn, guid, position, rotation, velocity);
			}
		}

		private Vector3 GetTargetPosition()
		{
			return PlayerSingleton<PlayerCamera>.Instance.transform.position + PlayerSingleton<PlayerCamera>.Instance.transform.forward * 1.25f * CurrentDraggable.HoldDistanceMultiplier;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_SendDragger_807933219);
				RegisterObserversRpc(1u, RpcReader___Observers_SetDragger_807933219);
				RegisterServerRpc(2u, RpcReader___Server_SendDraggableTransformData_4062762274);
				RegisterObserversRpc(3u, RpcReader___Observers_SetDraggableTransformData_3831223955);
				RegisterTargetRpc(4u, RpcReader___Target_SetDraggableTransformData_3831223955);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EDragging_002EDragManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
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
				writer.WriteString(draggableGUID);
				writer.WriteNetworkObject(dragger);
				writer.WriteVector3(position);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
		{
			SetDragger(draggableGUID, dragger, position);
		}

		private void RpcReader___Server_SendDragger_807933219(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string draggableGUID = PooledReader0.ReadString();
			NetworkObject dragger = PooledReader0.ReadNetworkObject();
			Vector3 position = PooledReader0.ReadVector3();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendDragger_807933219(draggableGUID, dragger, position);
			}
		}

		private void RpcWriter___Observers_SetDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
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
				writer.WriteString(draggableGUID);
				writer.WriteNetworkObject(dragger);
				writer.WriteVector3(position);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetDragger_807933219(string draggableGUID, NetworkObject dragger, Vector3 position)
		{
			Draggable @object = GUIDManager.GetObject<Draggable>(new Guid(draggableGUID));
			Player player = ((dragger != null) ? dragger.GetComponent<Player>() : null);
			if (!(@object != null))
			{
				return;
			}
			if (CurrentDraggable != @object && lastHeldDraggable != @object)
			{
				@object.Rigidbody.position = position;
			}
			if (dragger != null)
			{
				if (player != null)
				{
					@object.StartDragging(dragger.GetComponent<Player>());
				}
			}
			else
			{
				@object.StopDragging();
			}
		}

		private void RpcReader___Observers_SetDragger_807933219(PooledReader PooledReader0, Channel channel)
		{
			string draggableGUID = PooledReader0.ReadString();
			NetworkObject dragger = PooledReader0.ReadNetworkObject();
			Vector3 position = PooledReader0.ReadVector3();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetDragger_807933219(draggableGUID, dragger, position);
			}
		}

		private void RpcWriter___Server_SendDraggableTransformData_4062762274(string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
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
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendDraggableTransformData_4062762274(string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
		{
			SetDraggableTransformData(null, guid, position, rotation, velocity);
		}

		private void RpcReader___Server_SendDraggableTransformData_4062762274(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string guid = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 velocity = PooledReader0.ReadVector3();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendDraggableTransformData_4062762274(guid, position, rotation, velocity);
			}
		}

		private void RpcWriter___Observers_SetDraggableTransformData_3831223955(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
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
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetDraggableTransformData_3831223955(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
		{
			Draggable @object = GUIDManager.GetObject<Draggable>(new Guid(guid));
			if (@object == null)
			{
				Console.LogWarning("Failed to find draggable with GUID " + guid);
			}
			if (!(@object == lastThrownDraggable) && !(@object == lastHeldDraggable) && @object != null)
			{
				@object.Rigidbody.position = position;
				@object.Rigidbody.rotation = rotation;
				@object.Rigidbody.velocity = velocity;
			}
		}

		private void RpcReader___Observers_SetDraggableTransformData_3831223955(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 velocity = PooledReader0.ReadVector3();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetDraggableTransformData_3831223955(null, guid, position, rotation, velocity);
			}
		}

		private void RpcWriter___Target_SetDraggableTransformData_3831223955(NetworkConnection conn, string guid, Vector3 position, Quaternion rotation, Vector3 velocity)
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
				SendTargetRpc(4u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetDraggableTransformData_3831223955(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			Vector3 position = PooledReader0.ReadVector3();
			Quaternion rotation = PooledReader0.ReadQuaternion();
			Vector3 velocity = PooledReader0.ReadVector3();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetDraggableTransformData_3831223955(base.LocalConnection, guid, position, rotation, velocity);
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
