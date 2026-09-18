using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour
{
	public class GenericDialogueBehaviour : Behaviour
	{
		private Player targetPlayer;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted;

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendTargetPlayer(NetworkObject player)
		{
			RpcWriter___Server_SendTargetPlayer_3323014238(player);
			RpcLogic___SendTargetPlayer_3323014238(player);
		}

		[ObserversRpc(RunLocally = true)]
		private void SetTargetPlayer(NetworkObject player)
		{
			RpcWriter___Observers_SetTargetPlayer_3323014238(player);
			RpcLogic___SetTargetPlayer_3323014238(player);
		}

		public override void Enable()
		{
			base.Enable();
			base.beh.Update();
		}

		public override void Disable()
		{
			base.Disable();
			End();
		}

		protected override void Begin()
		{
			base.Begin();
			if (base.Npc.Movement.IsMoving)
			{
				base.Npc.Movement.Stop();
			}
		}

		protected override void Resume()
		{
			base.Resume();
			if (base.Npc.Movement.IsMoving)
			{
				base.Npc.Movement.Stop();
			}
		}

		protected override void End()
		{
			base.End();
			base.Npc.Movement.ResumeMovement();
		}

		public override void BehaviourUpdate()
		{
			base.BehaviourUpdate();
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (base.Npc.Movement.IsMoving)
			{
				base.Npc.Movement.Stop();
			}
			if (base.Npc.Movement.FaceDirectionInProgress || !(base.Npc.Avatar.Anim.TimeSinceSitEnd >= 0.5f))
			{
				return;
			}
			float distance;
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, out distance);
			if (!(closestPlayer == null))
			{
				Vector3 position = closestPlayer.EyePosition.position;
				if (Vector3.Angle(base.Npc.transform.forward, position - base.Npc.transform.position) > 10f)
				{
					base.Npc.Movement.FacePoint(position);
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(15u, RpcReader___Server_SendTargetPlayer_3323014238);
				RegisterObserversRpc(16u, RpcReader___Observers_SetTargetPlayer_3323014238);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EGenericDialogueBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendTargetPlayer_3323014238(NetworkObject player)
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
				SendServerRpc(15u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendTargetPlayer_3323014238(NetworkObject player)
		{
			SetTargetPlayer(player);
		}

		private void RpcReader___Server_SendTargetPlayer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject player = PooledReader0.ReadNetworkObject();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendTargetPlayer_3323014238(player);
			}
		}

		private void RpcWriter___Observers_SetTargetPlayer_3323014238(NetworkObject player)
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

		private void RpcLogic___SetTargetPlayer_3323014238(NetworkObject player)
		{
			if (Singleton<DialogueCanvas>.Instance.isActive && targetPlayer != null && targetPlayer.Owner.IsLocalClient && player != null && !player.Owner.IsLocalClient)
			{
				Singleton<GameInput>.Instance.ExitAll();
			}
			if (player != null)
			{
				targetPlayer = player.GetComponent<Player>();
			}
			else
			{
				targetPlayer = null;
			}
		}

		private void RpcReader___Observers_SetTargetPlayer_3323014238(PooledReader PooledReader0, Channel channel)
		{
			NetworkObject player = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetTargetPlayer_3323014238(player);
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
