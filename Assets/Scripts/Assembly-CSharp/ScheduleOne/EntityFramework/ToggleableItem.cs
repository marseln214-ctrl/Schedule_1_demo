using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Persistence.Datas;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.EntityFramework
{
	public class ToggleableItem : GridItem
	{
		public enum EStartupAction
		{
			None = 0,
			TurnOn = 1,
			TurnOff = 2,
			Toggle = 3
		}

		[Header("Settings")]
		public EStartupAction StartupAction;

		public UnityEvent onTurnedOn;

		public UnityEvent onTurnedOff;

		public UnityEvent onTurnOnOrOff;

		private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted;

		public bool IsOn { get; private set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EEntityFramework_002EToggleableItem_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (IsOn)
			{
				SetIsOn(connection, on: true);
			}
		}

		public void Toggle()
		{
			if (IsOn)
			{
				TurnOff();
			}
			else
			{
				TurnOn();
			}
		}

		public void TurnOn(bool network = true)
		{
			if (IsOn)
			{
				return;
			}
			if (network)
			{
				SendIsOn(on: true);
				return;
			}
			IsOn = true;
			if (onTurnedOn != null)
			{
				onTurnedOn.Invoke();
			}
			if (onTurnOnOrOff != null)
			{
				onTurnOnOrOff.Invoke();
			}
		}

		public void TurnOff(bool network = true)
		{
			if (!IsOn)
			{
				return;
			}
			if (network)
			{
				SendIsOn(on: false);
				return;
			}
			IsOn = false;
			if (onTurnedOff != null)
			{
				onTurnedOff.Invoke();
			}
			if (onTurnOnOrOff != null)
			{
				onTurnOnOrOff.Invoke();
			}
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		private void SendIsOn(bool on)
		{
			RpcWriter___Server_SendIsOn_1140765316(on);
			RpcLogic___SendIsOn_1140765316(on);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void SetIsOn(NetworkConnection conn, bool on)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetIsOn_214505783(conn, on);
				RpcLogic___SetIsOn_214505783(conn, on);
			}
			else
			{
				RpcWriter___Target_SetIsOn_214505783(conn, on);
			}
		}

		public override string GetSaveString()
		{
			return new ToggleableItemData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, OriginCoordinate, Rotation, IsOn).GetJson();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(8u, RpcReader___Server_SendIsOn_1140765316);
				RegisterObserversRpc(9u, RpcReader___Observers_SetIsOn_214505783);
				RegisterTargetRpc(10u, RpcReader___Target_SetIsOn_214505783);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEntityFramework_002EToggleableItemAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendIsOn_1140765316(bool on)
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
				writer.WriteBoolean(on);
				SendServerRpc(8u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SendIsOn_1140765316(bool on)
		{
			base.HasChanged = true;
			SetIsOn(null, on);
		}

		private void RpcReader___Server_SendIsOn_1140765316(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			bool on = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendIsOn_1140765316(on);
			}
		}

		private void RpcWriter___Observers_SetIsOn_214505783(NetworkConnection conn, bool on)
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
				writer.WriteBoolean(on);
				SendObserversRpc(9u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetIsOn_214505783(NetworkConnection conn, bool on)
		{
			if (on)
			{
				TurnOn(network: false);
			}
			else
			{
				TurnOff(network: false);
			}
		}

		private void RpcReader___Observers_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool on = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetIsOn_214505783(null, on);
			}
		}

		private void RpcWriter___Target_SetIsOn_214505783(NetworkConnection conn, bool on)
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
				writer.WriteBoolean(on);
				SendTargetRpc(10u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetIsOn_214505783(PooledReader PooledReader0, Channel channel)
		{
			bool on = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetIsOn_214505783(base.LocalConnection, on);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EToggleableItem_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			switch (StartupAction)
			{
			case EStartupAction.TurnOn:
				TurnOn();
				break;
			case EStartupAction.TurnOff:
				TurnOff();
				break;
			case EStartupAction.Toggle:
				Toggle();
				break;
			}
		}
	}
}
