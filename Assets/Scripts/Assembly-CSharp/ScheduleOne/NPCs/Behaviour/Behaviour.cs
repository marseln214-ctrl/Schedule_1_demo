using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs.Behaviour
{
	public class Behaviour : NetworkBehaviour
	{
		public bool EnabledOnAwake;

		[Header("Settings")]
		public string Name = "Behaviour";

		[Tooltip("Behaviour priority; higher = takes priority over lower number behaviour")]
		public int Priority;

		public UnityEvent onEnable = new UnityEvent();

		public UnityEvent onDisable = new UnityEvent();

		public UnityEvent onBegin;

		public UnityEvent onEnd;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public bool Enabled { get; protected set; }

		public bool Started { get; private set; }

		public bool Active { get; private set; }

		public NPCBehaviour beh { get; private set; }

		public NPC Npc => beh.Npc;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EBehaviour_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsHost)
			{
				if (Enabled)
				{
					Enable_Networked(connection);
				}
				else
				{
					Disable_Networked(connection);
				}
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			UpdateGameObjectName();
		}

		public virtual void Enable()
		{
			if (Npc.behaviour.DEBUG_MODE)
			{
				Debug.Log(Name + " enabled");
			}
			Enabled = true;
			if (onEnable != null)
			{
				onEnable.Invoke();
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendEnable()
		{
			RpcWriter___Server_SendEnable_2166136261();
			RpcLogic___SendEnable_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void Enable_Networked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Enable_Networked_328543758(conn);
				RpcLogic___Enable_Networked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Enable_Networked_328543758(conn);
			}
		}

		public virtual void Disable()
		{
			if (Npc.behaviour.DEBUG_MODE)
			{
				Debug.Log(Name + " disabled");
			}
			Enabled = false;
			Started = false;
			if (Active)
			{
				End();
			}
			if (onDisable != null)
			{
				onDisable.Invoke();
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendDisable()
		{
			RpcWriter___Server_SendDisable_2166136261();
			RpcLogic___SendDisable_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void Disable_Networked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Disable_Networked_328543758(conn);
				RpcLogic___Disable_Networked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Disable_Networked_328543758(conn);
			}
		}

		private void UpdateGameObjectName()
		{
			if (!(base.gameObject == null))
			{
				base.gameObject.name = Name + (Active ? " (Active)" : " (Inactive)");
				if (!Active)
				{
					base.gameObject.name = base.gameObject.name + (Enabled ? " (Enabled)" : " (Disabled)");
				}
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void Begin_Networked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Begin_Networked_328543758(conn);
				RpcLogic___Begin_Networked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Begin_Networked_328543758(conn);
			}
		}

		protected virtual void Begin()
		{
			if (beh.DEBUG_MODE)
			{
				Console.Log("Behaviour (" + Name + ") started");
			}
			Started = true;
			Active = true;
			beh.activeBehaviour = this;
			UpdateGameObjectName();
			if (onBegin != null)
			{
				onBegin.Invoke();
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendEnd()
		{
			RpcWriter___Server_SendEnd_2166136261();
			RpcLogic___SendEnd_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void End_Networked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_End_Networked_328543758(conn);
				RpcLogic___End_Networked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_End_Networked_328543758(conn);
			}
		}

		protected virtual void End()
		{
			if (beh.DEBUG_MODE)
			{
				Console.Log("Behaviour (" + Name + ") ended");
			}
			Active = false;
			beh.activeBehaviour = null;
			UpdateGameObjectName();
			if (onEnd != null)
			{
				onEnd.Invoke();
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void Pause_Networked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Pause_Networked_328543758(conn);
				RpcLogic___Pause_Networked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Pause_Networked_328543758(conn);
			}
		}

		protected virtual void Pause()
		{
			if (beh.DEBUG_MODE)
			{
				Console.Log("Behaviour (" + Name + ") paused");
			}
			Active = false;
			UpdateGameObjectName();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void Resume_Networked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_Resume_Networked_328543758(conn);
				RpcLogic___Resume_Networked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_Resume_Networked_328543758(conn);
			}
		}

		protected virtual void Resume()
		{
			if (beh.DEBUG_MODE)
			{
				Console.Log("Behaviour (" + Name + ") resumed");
			}
			Active = true;
			beh.activeBehaviour = this;
			UpdateGameObjectName();
		}

		public virtual void BehaviourUpdate()
		{
		}

		public virtual void ActiveMinPass()
		{
		}

		protected void SetPriority(int p)
		{
			Priority = p;
			beh.SortBehaviourStack();
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SendEnable_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_Enable_Networked_328543758);
				RegisterTargetRpc(2u, RpcReader___Target_Enable_Networked_328543758);
				RegisterServerRpc(3u, RpcReader___Server_SendDisable_2166136261);
				RegisterObserversRpc(4u, RpcReader___Observers_Disable_Networked_328543758);
				RegisterTargetRpc(5u, RpcReader___Target_Disable_Networked_328543758);
				RegisterObserversRpc(6u, RpcReader___Observers_Begin_Networked_328543758);
				RegisterTargetRpc(7u, RpcReader___Target_Begin_Networked_328543758);
				RegisterServerRpc(8u, RpcReader___Server_SendEnd_2166136261);
				RegisterObserversRpc(9u, RpcReader___Observers_End_Networked_328543758);
				RegisterTargetRpc(10u, RpcReader___Target_End_Networked_328543758);
				RegisterObserversRpc(11u, RpcReader___Observers_Pause_Networked_328543758);
				RegisterTargetRpc(12u, RpcReader___Target_Pause_Networked_328543758);
				RegisterObserversRpc(13u, RpcReader___Observers_Resume_Networked_328543758);
				RegisterTargetRpc(14u, RpcReader___Target_Resume_Networked_328543758);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendEnable_2166136261()
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

		public void RpcLogic___SendEnable_2166136261()
		{
			Enable_Networked(null);
		}

		private void RpcReader___Server_SendEnable_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendEnable_2166136261();
			}
		}

		private void RpcWriter___Observers_Enable_Networked_328543758(NetworkConnection conn)
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
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Enable_Networked_328543758(NetworkConnection conn)
		{
			Enable();
		}

		private void RpcReader___Observers_Enable_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Enable_Networked_328543758(null);
			}
		}

		private void RpcWriter___Target_Enable_Networked_328543758(NetworkConnection conn)
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
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Enable_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Enable_Networked_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Server_SendDisable_2166136261()
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

		public void RpcLogic___SendDisable_2166136261()
		{
			Disable_Networked(null);
		}

		private void RpcReader___Server_SendDisable_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendDisable_2166136261();
			}
		}

		private void RpcWriter___Observers_Disable_Networked_328543758(NetworkConnection conn)
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

		public void RpcLogic___Disable_Networked_328543758(NetworkConnection conn)
		{
			Disable();
		}

		private void RpcReader___Observers_Disable_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Disable_Networked_328543758(null);
			}
		}

		private void RpcWriter___Target_Disable_Networked_328543758(NetworkConnection conn)
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
				SendTargetRpc(5u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Disable_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Disable_Networked_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_Begin_Networked_328543758(NetworkConnection conn)
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
				SendObserversRpc(6u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Begin_Networked_328543758(NetworkConnection conn)
		{
			Begin();
		}

		private void RpcReader___Observers_Begin_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Begin_Networked_328543758(null);
			}
		}

		private void RpcWriter___Target_Begin_Networked_328543758(NetworkConnection conn)
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
				SendTargetRpc(7u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Begin_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Begin_Networked_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Server_SendEnd_2166136261()
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
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendEnd_2166136261()
		{
			End_Networked(null);
		}

		private void RpcReader___Server_SendEnd_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendEnd_2166136261();
			}
		}

		private void RpcWriter___Observers_End_Networked_328543758(NetworkConnection conn)
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
				SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___End_Networked_328543758(NetworkConnection conn)
		{
			End();
		}

		private void RpcReader___Observers_End_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___End_Networked_328543758(null);
			}
		}

		private void RpcWriter___Target_End_Networked_328543758(NetworkConnection conn)
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
				SendTargetRpc(10u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_End_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___End_Networked_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_Pause_Networked_328543758(NetworkConnection conn)
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
				SendObserversRpc(11u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Pause_Networked_328543758(NetworkConnection conn)
		{
			Pause();
		}

		private void RpcReader___Observers_Pause_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Pause_Networked_328543758(null);
			}
		}

		private void RpcWriter___Target_Pause_Networked_328543758(NetworkConnection conn)
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
				SendTargetRpc(12u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Pause_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Pause_Networked_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Observers_Resume_Networked_328543758(NetworkConnection conn)
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
				SendObserversRpc(13u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___Resume_Networked_328543758(NetworkConnection conn)
		{
			Resume();
		}

		private void RpcReader___Observers_Resume_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___Resume_Networked_328543758(null);
			}
		}

		private void RpcWriter___Target_Resume_Networked_328543758(NetworkConnection conn)
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
				SendTargetRpc(14u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_Resume_Networked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___Resume_Networked_328543758(base.LocalConnection);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EBehaviour_Assembly_002DCSharp_002Edll()
		{
			beh = GetComponentInParent<NPCBehaviour>();
			Enabled = EnabledOnAwake;
		}
	}
}
