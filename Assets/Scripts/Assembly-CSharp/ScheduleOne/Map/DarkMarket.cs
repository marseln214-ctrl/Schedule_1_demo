using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Persistence;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class DarkMarket : NetworkSingleton<DarkMarket>
	{
		public DarkMarketAccessZone AccessZone;

		public DarkMarketMainDoor MainDoor;

		public FullRank UnlockRank;

		private bool NetworkInitialize___EarlyScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted;

		public bool IsOpen { get; protected set; } = true;


		public bool Unlocked { get; protected set; }

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(OnLoad);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (Unlocked)
			{
				SetUnlocked(connection);
			}
		}

		private void Update()
		{
			IsOpen = ShouldBeOpen();
		}

		private bool ShouldBeOpen()
		{
			return false;
		}

		private void OnLoad()
		{
			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(OnLoad);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendUnlocked()
		{
			RpcWriter___Server_SendUnlocked_2166136261();
			RpcLogic___SendUnlocked_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void SetUnlocked(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetUnlocked_328543758(conn);
				RpcLogic___SetUnlocked_328543758(conn);
			}
			else
			{
				RpcWriter___Target_SetUnlocked_328543758(conn);
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_SendUnlocked_2166136261);
				RegisterObserversRpc(1u, RpcReader___Observers_SetUnlocked_328543758);
				RegisterTargetRpc(2u, RpcReader___Target_SetUnlocked_328543758);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EMap_002EDarkMarketAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendUnlocked_2166136261()
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

		public void RpcLogic___SendUnlocked_2166136261()
		{
			SetUnlocked(null);
		}

		private void RpcReader___Server_SendUnlocked_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendUnlocked_2166136261();
			}
		}

		private void RpcWriter___Observers_SetUnlocked_328543758(NetworkConnection conn)
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

		private void RpcLogic___SetUnlocked_328543758(NetworkConnection conn)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("WarehouseUnlocked", true.ToString());
			MainDoor.SetKnockingEnabled(enabled: false);
			Unlocked = true;
		}

		private void RpcReader___Observers_SetUnlocked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetUnlocked_328543758(null);
			}
		}

		private void RpcWriter___Target_SetUnlocked_328543758(NetworkConnection conn)
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

		private void RpcReader___Target_SetUnlocked_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___SetUnlocked_328543758(base.LocalConnection);
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
