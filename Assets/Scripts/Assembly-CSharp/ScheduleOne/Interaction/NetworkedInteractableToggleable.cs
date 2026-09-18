using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Interaction
{
	public class NetworkedInteractableToggleable : NetworkBehaviour
	{
		public string ActivateMessage = "Activate";

		public string DeactivateMessage = "Deactivate";

		public float CoolDown;

		[Header("References")]
		public InteractableObject IntObj;

		public UnityEvent onToggle = new UnityEvent();

		public UnityEvent onActivate = new UnityEvent();

		public UnityEvent onDeactivate = new UnityEvent();

		private float lastActivated;

		private bool NetworkInitialize___EarlyScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted;

		public bool IsActivated { get; private set; }

		public void Start()
		{
			IntObj.onHovered.AddListener(Hovered);
			IntObj.onInteractStart.AddListener(Interacted);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (IsActivated)
			{
				SetState(connection, activated: true);
			}
		}

		public void Hovered()
		{
			if (Time.time - lastActivated < CoolDown)
			{
				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
				return;
			}
			IntObj.SetMessage(IsActivated ? DeactivateMessage : ActivateMessage);
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}

		public void Interacted()
		{
			SendToggle();
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendToggle()
		{
			RpcWriter___Server_SendToggle_2166136261();
			RpcLogic___SendToggle_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void SetState(NetworkConnection conn, bool activated)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetState_214505783(conn, activated);
				RpcLogic___SetState_214505783(conn, activated);
			}
			else
			{
				RpcWriter___Target_SetState_214505783(conn, activated);
			}
		}

		public void PoliceDetected()
		{
			if (InstanceFinder.IsServer && !IsActivated)
			{
				SendToggle();
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendToggle_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_SetState_214505783);
				RegisterTargetRpc(2u, RpcReader___Target_SetState_214505783);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EInteraction_002ENetworkedInteractableToggleableAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendToggle_2166136261()
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
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendToggle_2166136261()
		{
			SetState(null, !IsActivated);
		}

		private void RpcReader___Server_SendToggle_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendToggle_2166136261();
			}
		}

		private void RpcWriter___Observers_SetState_214505783(NetworkConnection conn, bool activated)
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
				writer.WriteBoolean(activated);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___SetState_214505783(NetworkConnection conn, bool activated)
		{
			if (IsActivated != activated)
			{
				lastActivated = Time.time;
				IsActivated = !IsActivated;
				if (onToggle != null)
				{
					onToggle.Invoke();
				}
				if (IsActivated)
				{
					onActivate.Invoke();
				}
				else
				{
					onDeactivate.Invoke();
				}
			}
		}

		private void RpcReader___Observers_SetState_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool activated = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetState_214505783(null, activated);
			}
		}

		private void RpcWriter___Target_SetState_214505783(NetworkConnection conn, bool activated)
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
				writer.WriteBoolean(activated);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetState_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool activated = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetState_214505783(base.LocalConnection, activated);
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
