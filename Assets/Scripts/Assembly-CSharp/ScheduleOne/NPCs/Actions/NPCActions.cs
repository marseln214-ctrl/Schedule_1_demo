using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.NPCs.Actions
{
	public class NPCActions : NetworkBehaviour
	{
		private NPC npc;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted;

		protected NPCBehaviour behaviour => npc.behaviour;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002EActions_002ENPCActions_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public void Cower()
		{
			behaviour.GetBehaviour("Cowering").Enable_Networked(null);
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				yield return new WaitForSeconds(10f);
				behaviour.GetBehaviour("Cowering").Disable_Networked(null);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void CallPolice_Networked(Player player)
		{
			RpcWriter___Server_CallPolice_Networked_1385486242(player);
			RpcLogic___CallPolice_Networked_1385486242(player);
		}

		public void SetCallPoliceBehaviourCrime(Crime crime)
		{
			npc.behaviour.CallPoliceBehaviour.ReportedCrime = crime;
		}

		public void FacePlayer(Player player)
		{
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_CallPolice_Networked_1385486242);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002EActions_002ENPCActionsAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_CallPolice_Networked_1385486242(Player player)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EPlayerScripts_002EPlayerFishNet_002ESerializing_002EGenerated(writer, player);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___CallPolice_Networked_1385486242(Player player)
		{
			if (NetworkSingleton<GameManager>.Instance.IsTutorial || !npc.IsConscious)
			{
				return;
			}
			Console.Log(npc.fullName + " is calling the police on " + player.PlayerName);
			if (player.CrimeData.CurrentPursuitLevel != 0)
			{
				Console.LogWarning("Player is already being pursued, ignoring call police request.");
				return;
			}
			npc.behaviour.CallPoliceBehaviour.Target = player;
			if (InstanceFinder.IsServer)
			{
				npc.behaviour.CallPoliceBehaviour.Enable_Networked(null);
			}
		}

		private void RpcReader___Server_CallPolice_Networked_1385486242(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			Player player = GeneratedReaders___Internal.Read___ScheduleOne_002EPlayerScripts_002EPlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___CallPolice_Networked_1385486242(player);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002EActions_002ENPCActions_Assembly_002DCSharp_002Edll()
		{
			npc = GetComponentInParent<NPC>();
		}
	}
}
