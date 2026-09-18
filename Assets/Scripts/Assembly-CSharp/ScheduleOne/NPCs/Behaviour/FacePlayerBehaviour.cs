using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class FacePlayerBehaviour : Behaviour
	{
		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFacePlayerBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFacePlayerBehaviourAssembly_002DCSharp_002Edll_Excuted;

		public Player Player { get; private set; }

		public float Countdown { get; private set; }

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SetTarget(NetworkObject player, float countDown = 5f)
		{
			RpcWriter___Server_SetTarget_244313061(player, countDown);
			RpcLogic___SetTarget_244313061(player, countDown);
		}

		[ObserversRpc(RunLocally = true)]
		private void SetTargetLocal(NetworkObject player)
		{
			RpcWriter___Observers_SetTargetLocal_3323014238(player);
			RpcLogic___SetTargetLocal_3323014238(player);
		}

		protected override void Begin()
		{
			base.Begin();
			base.Npc.Movement.Stop();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!base.Active)
			{
				return;
			}
			if (Player != null)
			{
				base.Npc.Avatar.LookController.OverrideLookTarget(Player.EyePosition.position, 1, rotateBody: true);
			}
			if (InstanceFinder.IsServer)
			{
				Countdown -= Time.deltaTime;
				if (Countdown <= 0f)
				{
					Disable_Networked(null);
				}
			}
		}

		public override void Disable()
		{
			base.Disable();
			End();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFacePlayerBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EFacePlayerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(15u, RpcReader___Server_SetTarget_244313061);
				RegisterObserversRpc(16u, RpcReader___Observers_SetTargetLocal_3323014238);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFacePlayerBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EFacePlayerBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetTarget_244313061(NetworkObject player, float countDown = 5f)
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
				writer.WriteSingle(countDown);
				SendServerRpc(15u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetTarget_244313061(NetworkObject player, float countDown = 5f)
		{
			Console.Log("SetTarget: " + player);
			Countdown = countDown;
			Player = ((player != null) ? player.GetComponent<Player>() : null);
			SetTargetLocal(player);
		}

		private void RpcReader___Server_SetTarget_244313061(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject player = PooledReader0.ReadNetworkObject();
			float countDown = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetTarget_244313061(player, countDown);
			}
		}

		private void RpcWriter___Observers_SetTargetLocal_3323014238(NetworkObject player)
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
				writer.WriteNetworkObject(player);
				SendObserversRpc(16u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetTargetLocal_3323014238(NetworkObject player)
		{
			Player = ((player != null) ? player.GetComponent<Player>() : null);
		}

		private void RpcReader___Observers_SetTargetLocal_3323014238(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject player = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetTargetLocal_3323014238(player);
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
