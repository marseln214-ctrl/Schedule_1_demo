using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Phone.Messages;
using UnityEngine;

namespace ScheduleOne.Messaging
{
	public class MessagingManager : NetworkSingleton<MessagingManager>
	{
		protected Dictionary<NPC, MSGConversation> ConversationMap = new Dictionary<NPC, MSGConversation>();

		private bool NetworkInitialize___EarlyScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted;

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EMessaging_002EMessagingManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsLocalClient)
			{
				StartCoroutine(SendMessages());
			}
			IEnumerator SendMessages()
			{
				yield return new WaitUntil(() => Player.GetPlayer(connection) != null);
				foreach (NPC key in ConversationMap.Keys)
				{
					if (ConversationMap[key].messageHistory.Count > 0 || ConversationMap[key].messageChainHistory.Count > 0)
					{
						MSGConversationData saveData = ConversationMap[key].GetSaveData();
						ReceiveMSGConversationData(connection, key.ID, saveData);
					}
				}
			}
		}

		public MSGConversation GetConversation(NPC npc)
		{
			if (!ConversationMap.ContainsKey(npc))
			{
				Console.LogError("No conversation found for " + npc.fullName);
				return null;
			}
			return ConversationMap[npc];
		}

		public void Register(NPC npc, MSGConversation convs)
		{
			if (ConversationMap.ContainsKey(npc))
			{
				Console.LogError("Conversation already registered for " + npc.fullName);
			}
			else
			{
				ConversationMap.Add(npc, convs);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendMessage(Message m, bool notify, string npcID)
		{
			RpcWriter___Server_SendMessage_2134336246(m, notify, npcID);
			RpcLogic___SendMessage_2134336246(m, notify, npcID);
		}

		[ObserversRpc(RunLocally = true)]
		private void ReceiveMessage(Message m, bool notify, string npcID)
		{
			RpcWriter___Observers_ReceiveMessage_2134336246(m, notify, npcID);
			RpcLogic___ReceiveMessage_2134336246(m, notify, npcID);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendMessageChain(MessageChain m, string npcID, float initialDelay, bool notify)
		{
			RpcWriter___Server_SendMessageChain_3949292778(m, npcID, initialDelay, notify);
			RpcLogic___SendMessageChain_3949292778(m, npcID, initialDelay, notify);
		}

		[ObserversRpc(RunLocally = true)]
		private void ReceiveMessageChain(MessageChain m, string npcID, float initialDelay, bool notify)
		{
			RpcWriter___Observers_ReceiveMessageChain_3949292778(m, npcID, initialDelay, notify);
			RpcLogic___ReceiveMessageChain_3949292778(m, npcID, initialDelay, notify);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendResponse(int responseIndex, string npcID)
		{
			RpcWriter___Server_SendResponse_2801973956(responseIndex, npcID);
			RpcLogic___SendResponse_2801973956(responseIndex, npcID);
		}

		[ObserversRpc(RunLocally = true)]
		private void ReceiveResponse(int responseIndex, string npcID)
		{
			RpcWriter___Observers_ReceiveResponse_2801973956(responseIndex, npcID);
			RpcLogic___ReceiveResponse_2801973956(responseIndex, npcID);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendPlayerMessage(int sendableIndex, int sentIndex, string npcID)
		{
			RpcWriter___Server_SendPlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
			RpcLogic___SendPlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
		}

		[ObserversRpc(RunLocally = true)]
		private void ReceivePlayerMessage(int sendableIndex, int sentIndex, string npcID)
		{
			RpcWriter___Observers_ReceivePlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
			RpcLogic___ReceivePlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
		}

		[TargetRpc]
		private void ReceiveMSGConversationData(NetworkConnection conn, string npcID, MSGConversationData data)
		{
			RpcWriter___Target_ReceiveMSGConversationData_2662241369(conn, npcID, data);
		}

		[ServerRpc(RequireOwnership = false)]
		public void ClearResponses(string npcID)
		{
			RpcWriter___Server_ClearResponses_3615296227(npcID);
		}

		[ObserversRpc]
		private void ReceiveClearResponses(string npcID)
		{
			RpcWriter___Observers_ReceiveClearResponses_3615296227(npcID);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void ShowResponses(string npcID, List<Response> responses, float delay)
		{
			RpcWriter___Server_ShowResponses_995803534(npcID, responses, delay);
			RpcLogic___ShowResponses_995803534(npcID, responses, delay);
		}

		[ObserversRpc(RunLocally = true)]
		private void ReceiveShowResponses(string npcID, List<Response> responses, float delay)
		{
			RpcWriter___Observers_ReceiveShowResponses_995803534(npcID, responses, delay);
			RpcLogic___ReceiveShowResponses_995803534(npcID, responses, delay);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_SendMessage_2134336246);
				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveMessage_2134336246);
				RegisterServerRpc(2u, RpcReader___Server_SendMessageChain_3949292778);
				RegisterObserversRpc(3u, RpcReader___Observers_ReceiveMessageChain_3949292778);
				RegisterServerRpc(4u, RpcReader___Server_SendResponse_2801973956);
				RegisterObserversRpc(5u, RpcReader___Observers_ReceiveResponse_2801973956);
				RegisterServerRpc(6u, RpcReader___Server_SendPlayerMessage_1952281135);
				RegisterObserversRpc(7u, RpcReader___Observers_ReceivePlayerMessage_1952281135);
				RegisterTargetRpc(8u, RpcReader___Target_ReceiveMSGConversationData_2662241369);
				RegisterServerRpc(9u, RpcReader___Server_ClearResponses_3615296227);
				RegisterObserversRpc(10u, RpcReader___Observers_ReceiveClearResponses_3615296227);
				RegisterServerRpc(11u, RpcReader___Server_ShowResponses_995803534);
				RegisterObserversRpc(12u, RpcReader___Observers_ReceiveShowResponses_995803534);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EMessaging_002EMessagingManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendMessage_2134336246(Message m, bool notify, string npcID)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated(writer, m);
				writer.WriteBoolean(notify);
				writer.WriteString(npcID);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendMessage_2134336246(Message m, bool notify, string npcID)
		{
			ReceiveMessage(m, notify, npcID);
		}

		private void RpcReader___Server_SendMessage_2134336246(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			Message m = GeneratedReaders___Internal.Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds(PooledReader0);
			bool notify = PooledReader0.ReadBoolean();
			string npcID = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendMessage_2134336246(m, notify, npcID);
			}
		}

		private void RpcWriter___Observers_ReceiveMessage_2134336246(Message m, bool notify, string npcID)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated(writer, m);
				writer.WriteBoolean(notify);
				writer.WriteString(npcID);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveMessage_2134336246(Message m, bool notify, string npcID)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
			}
			else
			{
				ConversationMap[nPC].SendMessage(m, notify, network: false);
			}
		}

		private void RpcReader___Observers_ReceiveMessage_2134336246(PooledReader PooledReader0, Channel channel)
		{
			Message m = GeneratedReaders___Internal.Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds(PooledReader0);
			bool notify = PooledReader0.ReadBoolean();
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveMessage_2134336246(m, notify, npcID);
			}
		}

		private void RpcWriter___Server_SendMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated(writer, m);
				writer.WriteString(npcID);
				writer.WriteSingle(initialDelay);
				writer.WriteBoolean(notify);
				SendServerRpc(2u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
		{
			ReceiveMessageChain(m, npcID, initialDelay, notify);
		}

		private void RpcReader___Server_SendMessageChain_3949292778(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			MessageChain m = GeneratedReaders___Internal.Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string npcID = PooledReader0.ReadString();
			float initialDelay = PooledReader0.ReadSingle();
			bool notify = PooledReader0.ReadBoolean();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendMessageChain_3949292778(m, npcID, initialDelay, notify);
			}
		}

		private void RpcWriter___Observers_ReceiveMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated(writer, m);
				writer.WriteString(npcID);
				writer.WriteSingle(initialDelay);
				writer.WriteBoolean(notify);
				SendObserversRpc(3u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveMessageChain_3949292778(MessageChain m, string npcID, float initialDelay, bool notify)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
			}
			else
			{
				ConversationMap[nPC].SendMessageChain(m, initialDelay, notify, network: false);
			}
		}

		private void RpcReader___Observers_ReceiveMessageChain_3949292778(PooledReader PooledReader0, Channel channel)
		{
			MessageChain m = GeneratedReaders___Internal.Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds(PooledReader0);
			string npcID = PooledReader0.ReadString();
			float initialDelay = PooledReader0.ReadSingle();
			bool notify = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveMessageChain_3949292778(m, npcID, initialDelay, notify);
			}
		}

		private void RpcWriter___Server_SendResponse_2801973956(int responseIndex, string npcID)
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
				writer.WriteInt32(responseIndex);
				writer.WriteString(npcID);
				SendServerRpc(4u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendResponse_2801973956(int responseIndex, string npcID)
		{
			ReceiveResponse(responseIndex, npcID);
		}

		private void RpcReader___Server_SendResponse_2801973956(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int responseIndex = PooledReader0.ReadInt32();
			string npcID = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendResponse_2801973956(responseIndex, npcID);
			}
		}

		private void RpcWriter___Observers_ReceiveResponse_2801973956(int responseIndex, string npcID)
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
				writer.WriteInt32(responseIndex);
				writer.WriteString(npcID);
				SendObserversRpc(5u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveResponse_2801973956(int responseIndex, string npcID)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
				return;
			}
			MSGConversation mSGConversation = ConversationMap[nPC];
			if (mSGConversation.currentResponses.Count <= responseIndex)
			{
				Console.LogWarning("Response index out of range for " + nPC.fullName);
			}
			else
			{
				mSGConversation.ResponseChosen(mSGConversation.currentResponses[responseIndex], network: false);
			}
		}

		private void RpcReader___Observers_ReceiveResponse_2801973956(PooledReader PooledReader0, Channel channel)
		{
			int responseIndex = PooledReader0.ReadInt32();
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveResponse_2801973956(responseIndex, npcID);
			}
		}

		private void RpcWriter___Server_SendPlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
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
				writer.WriteInt32(sendableIndex);
				writer.WriteInt32(sentIndex);
				writer.WriteString(npcID);
				SendServerRpc(6u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendPlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
		{
			ReceivePlayerMessage(sendableIndex, sentIndex, npcID);
		}

		private void RpcReader___Server_SendPlayerMessage_1952281135(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int sendableIndex = PooledReader0.ReadInt32();
			int sentIndex = PooledReader0.ReadInt32();
			string npcID = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendPlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
			}
		}

		private void RpcWriter___Observers_ReceivePlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
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
				writer.WriteInt32(sendableIndex);
				writer.WriteInt32(sentIndex);
				writer.WriteString(npcID);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceivePlayerMessage_1952281135(int sendableIndex, int sentIndex, string npcID)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
			}
			else
			{
				ConversationMap[nPC].SendPlayerMessage(sendableIndex, sentIndex, network: false);
			}
		}

		private void RpcReader___Observers_ReceivePlayerMessage_1952281135(PooledReader PooledReader0, Channel channel)
		{
			int sendableIndex = PooledReader0.ReadInt32();
			int sentIndex = PooledReader0.ReadInt32();
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceivePlayerMessage_1952281135(sendableIndex, sentIndex, npcID);
			}
		}

		private void RpcWriter___Target_ReceiveMSGConversationData_2662241369(NetworkConnection conn, string npcID, MSGConversationData data)
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
				writer.WriteString(npcID);
				GeneratedWriters___Internal.Write___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerated(writer, data);
				SendTargetRpc(8u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveMSGConversationData_2662241369(NetworkConnection conn, string npcID, MSGConversationData data)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
			}
			else
			{
				ConversationMap[nPC].Load(data);
			}
		}

		private void RpcReader___Target_ReceiveMSGConversationData_2662241369(PooledReader PooledReader0, Channel channel)
		{
			string npcID = PooledReader0.ReadString();
			MSGConversationData data = GeneratedReaders___Internal.Read___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveMSGConversationData_2662241369(base.LocalConnection, npcID, data);
			}
		}

		private void RpcWriter___Server_ClearResponses_3615296227(string npcID)
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
				writer.WriteString(npcID);
				SendServerRpc(9u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___ClearResponses_3615296227(string npcID)
		{
			ReceiveClearResponses(npcID);
		}

		private void RpcReader___Server_ClearResponses_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___ClearResponses_3615296227(npcID);
			}
		}

		private void RpcWriter___Observers_ReceiveClearResponses_3615296227(string npcID)
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
				writer.WriteString(npcID);
				SendObserversRpc(10u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveClearResponses_3615296227(string npcID)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
			}
			else
			{
				ConversationMap[nPC].ClearResponses();
			}
		}

		private void RpcReader___Observers_ReceiveClearResponses_3615296227(PooledReader PooledReader0, Channel channel)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveClearResponses_3615296227(npcID);
			}
		}

		private void RpcWriter___Server_ShowResponses_995803534(string npcID, List<Response> responses, float delay)
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
				writer.WriteString(npcID);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated(writer, responses);
				writer.WriteSingle(delay);
				SendServerRpc(11u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___ShowResponses_995803534(string npcID, List<Response> responses, float delay)
		{
			ReceiveShowResponses(npcID, responses, delay);
		}

		private void RpcReader___Server_ShowResponses_995803534(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string npcID = PooledReader0.ReadString();
			List<Response> responses = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			float delay = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___ShowResponses_995803534(npcID, responses, delay);
			}
		}

		private void RpcWriter___Observers_ReceiveShowResponses_995803534(string npcID, List<Response> responses, float delay)
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
				writer.WriteString(npcID);
				GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated(writer, responses);
				writer.WriteSingle(delay);
				SendObserversRpc(12u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveShowResponses_995803534(string npcID, List<Response> responses, float delay)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogError("NPC not found with ID " + npcID);
			}
			else
			{
				ConversationMap[nPC].ShowResponses(responses, delay, network: false);
			}
		}

		private void RpcReader___Observers_ReceiveShowResponses_995803534(PooledReader PooledReader0, Channel channel)
		{
			string npcID = PooledReader0.ReadString();
			List<Response> responses = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
			float delay = PooledReader0.ReadSingle();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveShowResponses_995803534(npcID, responses, delay);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EMessaging_002EMessagingManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
		}
	}
}
