using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyButtons;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class QuestManager : NetworkSingleton<QuestManager>, IBaseSaveable, ISaveable
	{
		public enum EQuestAction
		{
			Begin = 0,
			Success = 1,
			Fail = 2,
			Expire = 3,
			Cancel = 4
		}

		public const EQuestState DEFAULT_QUEST_STATE = EQuestState.Inactive;

		public Quest[] DefaultQuests;

		[Header("References")]
		public Transform QuestContainer;

		public Transform ContractContainer;

		public AudioSourceController QuestCompleteSound;

		public AudioSourceController QuestEntryCompleteSound;

		[Header("Prefabs")]
		public Contract ContractPrefab;

		public DeaddropQuest DeaddropCollectionPrefab;

		private QuestsLoader loader = new QuestsLoader();

		private List<string> writtenContractFiles = new List<string>();

		private bool NetworkInitialize___EarlyScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted;

		public string SaveFolderName => "Quests";

		public string SaveFileName => "Quests";

		public Loader Loader => loader;

		public bool ShouldSaveUnderFolder => true;

		public List<string> LocalExtraFiles { get; set; } = new List<string>();


		public List<string> LocalExtraFolders { get; set; } = new List<string> { "Contracts" };


		public bool HasChanged { get; set; }

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EQuests_002EQuestManager_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public virtual void InitializeSaveable()
		{
			Singleton<SaveManager>.Instance.RegisterSaveable(this);
		}

		protected override void Start()
		{
			base.Start();
			InvokeRepeating("UpdateVariables", 0f, 0.5f);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsLocalClient)
			{
				StartCoroutine(SendQuestStuff());
			}
			IEnumerator SendQuestStuff()
			{
				yield return new WaitUntil(() => Player.GetPlayer(connection) != null && Player.GetPlayer(connection).playerDataRetrieveReturned);
				foreach (Contract contract in Contract.Contracts)
				{
					if (!((contract == null) | (contract.QuestState != EQuestState.Active)))
					{
						ContractInfo contractData = new ContractInfo(contract.Payment, contract.ProductList, contract.DeliveryLocation.GUID.ToString(), contract.DeliveryWindow, contract.Expires, 0, contract.PickupScheduleIndex, isCounterOffer: false);
						NetworkObject dealerObj = null;
						if (contract.Dealer != null)
						{
							dealerObj = contract.Dealer.NetworkObject;
						}
						CreateContract_Networked(connection, contract.GUID.ToString(), contract.IsTracked, contract.Customer, contractData, contract.Expiry, contract.AcceptTime, dealerObj);
					}
				}
				foreach (DeaddropQuest deaddropQuest in DeaddropQuest.DeaddropQuests)
				{
					if (!((deaddropQuest == null) | (deaddropQuest.QuestState != EQuestState.Active)))
					{
						CreateDeaddropCollectionQuest(connection, deaddropQuest.Drop.GUID.ToString(), deaddropQuest.GUID.ToString());
					}
				}
				Quest[] defaultQuests = DefaultQuests;
				foreach (Quest quest in defaultQuests)
				{
					if (quest == null)
					{
						Console.LogError("Default quest is null!");
					}
					else
					{
						for (int j = 0; j < quest.Entries.Count; j++)
						{
							if (quest.Entries[j].State != 0)
							{
								ReceiveQuestEntryState(connection, quest.GUID.ToString(), j, quest.Entries[j].State);
							}
						}
						if (quest.QuestState != 0)
						{
							ReceiveQuestState(connection, quest.GUID.ToString(), quest.QuestState);
						}
						if (quest.IsTracked)
						{
							SetQuestTracked(connection, quest.GUID.ToString(), tracked: true);
						}
					}
				}
			}
		}

		private void UpdateVariables()
		{
			if (InstanceFinder.IsServer)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Active_Contract_Count", Contract.Contracts.Count.ToString());
			}
		}

		[ServerRpc(RequireOwnership = false)]
		public void SendContractAccepted(NetworkObject customer, ContractInfo contractData, bool track, string guid)
		{
			RpcWriter___Server_SendContractAccepted_1030683829(customer, contractData, track, guid);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void CreateContract_Networked(NetworkConnection conn, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_CreateContract_Networked_1113640585(conn, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
				RpcLogic___CreateContract_Networked_1113640585(conn, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
			}
			else
			{
				RpcWriter___Target_CreateContract_Networked_1113640585(conn, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
			}
		}

		public Contract CreateContract_Local(string title, string description, QuestEntryData[] entries, string guid, bool tracked, NetworkObject customer, float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, bool expires, GameDateTime expiry, int pickupScheduleIndex, GameDateTime acceptTime, Dealer dealer = null)
		{
			Contract component = UnityEngine.Object.Instantiate(ContractPrefab.gameObject, ContractContainer).GetComponent<Contract>();
			component.InitializeContract(title, description, entries, guid, customer, payment, products, deliveryLocationGUID, deliveryWindow, pickupScheduleIndex, acceptTime);
			component.Entries[0].PoILocation = component.DeliveryLocation.CustomerStandPoint;
			component.Entries[0].CreatePoI();
			if (tracked)
			{
				component.SetIsTracked(tracked: true);
			}
			if (expires)
			{
				component.ConfigureExpiry(expires: true, expiry);
			}
			if (dealer != null)
			{
				component.SetDealer(dealer);
			}
			component.Begin(network: false);
			return component;
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendQuestAction(string guid, EQuestAction action)
		{
			RpcWriter___Server_SendQuestAction_2848227116(guid, action);
			RpcLogic___SendQuestAction_2848227116(guid, action);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void ReceiveQuestAction(NetworkConnection conn, string guid, EQuestAction action)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ReceiveQuestAction_920727549(conn, guid, action);
				RpcLogic___ReceiveQuestAction_920727549(conn, guid, action);
			}
			else
			{
				RpcWriter___Target_ReceiveQuestAction_920727549(conn, guid, action);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendQuestState(string guid, EQuestState state)
		{
			RpcWriter___Server_SendQuestState_4117703421(guid, state);
			RpcLogic___SendQuestState_4117703421(guid, state);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void ReceiveQuestState(NetworkConnection conn, string guid, EQuestState state)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ReceiveQuestState_3887376304(conn, guid, state);
				RpcLogic___ReceiveQuestState_3887376304(conn, guid, state);
			}
			else
			{
				RpcWriter___Target_ReceiveQuestState_3887376304(conn, guid, state);
			}
		}

		[TargetRpc]
		private void SetQuestTracked(NetworkConnection conn, string guid, bool tracked)
		{
			RpcWriter___Target_SetQuestTracked_619441887(conn, guid, tracked);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendQuestEntryState(string guid, int entryIndex, EQuestState state)
		{
			RpcWriter___Server_SendQuestEntryState_375159588(guid, entryIndex, state);
			RpcLogic___SendQuestEntryState_375159588(guid, entryIndex, state);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void ReceiveQuestEntryState(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_ReceiveQuestEntryState_311789429(conn, guid, entryIndex, state);
				RpcLogic___ReceiveQuestEntryState_311789429(conn, guid, entryIndex, state);
			}
			else
			{
				RpcWriter___Target_ReceiveQuestEntryState_311789429(conn, guid, entryIndex, state);
			}
		}

		[Button]
		public void PrintQuestStates()
		{
			for (int i = 0; i < Quest.Quests.Count; i++)
			{
				Console.Log(Quest.Quests[i].GetQuestTitle() + " state: " + Quest.Quests[i].QuestState);
			}
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public void CreateDeaddropCollectionQuest(NetworkConnection conn, string dropGUID, string guidString = "")
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_CreateDeaddropCollectionQuest_3895153758(conn, dropGUID, guidString);
				RpcLogic___CreateDeaddropCollectionQuest_3895153758(conn, dropGUID, guidString);
			}
			else
			{
				RpcWriter___Target_CreateDeaddropCollectionQuest_3895153758(conn, dropGUID, guidString);
			}
		}

		public DeaddropQuest CreateDeaddropCollectionQuest(string dropGUID, string guidString = "")
		{
			Guid guid = ((guidString != "") ? new Guid(guidString) : GUIDManager.GenerateUniqueGUID());
			if (GUIDManager.IsGUIDAlreadyRegistered(guid))
			{
				return null;
			}
			DeadDrop @object = GUIDManager.GetObject<DeadDrop>(new Guid(dropGUID));
			if (@object == null)
			{
				Console.LogWarning("Failed to find dead drop with GUID: " + dropGUID);
				return null;
			}
			DeaddropQuest component = UnityEngine.Object.Instantiate(DeaddropCollectionPrefab.gameObject, QuestContainer).GetComponent<DeaddropQuest>();
			component.SetDrop(@object);
			component.Description = "Collect the dead drop " + @object.DeadDropDescription;
			component.SetGUID(guid);
			component.Entries[0].SetEntryTitle(@object.DeadDropName);
			component.Begin();
			return component;
		}

		public void PlayCompleteQuestSound()
		{
			if (QuestEntryCompleteSound.isPlaying)
			{
				QuestEntryCompleteSound.Stop();
			}
			QuestCompleteSound.Play();
		}

		public void PlayCompleteQuestEntrySound()
		{
			QuestEntryCompleteSound.Play();
		}

		public virtual string GetSaveString()
		{
			return string.Empty;
		}

		public virtual List<string> WriteData(string parentFolderPath)
		{
			List<string> list = new List<string>();
			writtenContractFiles.Clear();
			string containerFolder = ((ISaveable)this).GetContainerFolder(parentFolderPath);
			for (int i = 0; i < Quest.Quests.Count; i++)
			{
				if (!(Quest.Quests[i] is Contract) && Quest.Quests[i].HasChanged)
				{
					list.Add(Quest.Quests[i].SaveFileName);
					new SaveRequest(Quest.Quests[i], containerFolder);
				}
			}
			string parentFolderPath2 = ((ISaveable)this).WriteFolder(parentFolderPath, "Contracts");
			for (int j = 0; j < Contract.Contracts.Count; j++)
			{
				if (Contract.Contracts[j].ShouldSave())
				{
					writtenContractFiles.Add(Contract.Contracts[j].SaveFileName + ".json");
					if (Contract.Contracts[j].HasChanged)
					{
						new SaveRequest(Contract.Contracts[j], parentFolderPath2);
					}
				}
			}
			return list;
		}

		public virtual void DeleteUnapprovedFiles(string parentFolderPath)
		{
			string[] files = Directory.GetFiles(((ISaveable)this).WriteFolder(parentFolderPath, "Contracts"));
			for (int i = 0; i < files.Length; i++)
			{
				if (!writtenContractFiles.Contains(files[i]))
				{
					try
					{
						File.Delete(files[i]);
					}
					catch (Exception ex)
					{
						Console.LogError("Failed to delete unapproved contract file: " + files[i] + " - " + ex.Message);
					}
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___Server_SendContractAccepted_1030683829);
				RegisterObserversRpc(1u, RpcReader___Observers_CreateContract_Networked_1113640585);
				RegisterTargetRpc(2u, RpcReader___Target_CreateContract_Networked_1113640585);
				RegisterServerRpc(3u, RpcReader___Server_SendQuestAction_2848227116);
				RegisterObserversRpc(4u, RpcReader___Observers_ReceiveQuestAction_920727549);
				RegisterTargetRpc(5u, RpcReader___Target_ReceiveQuestAction_920727549);
				RegisterServerRpc(6u, RpcReader___Server_SendQuestState_4117703421);
				RegisterObserversRpc(7u, RpcReader___Observers_ReceiveQuestState_3887376304);
				RegisterTargetRpc(8u, RpcReader___Target_ReceiveQuestState_3887376304);
				RegisterTargetRpc(9u, RpcReader___Target_SetQuestTracked_619441887);
				RegisterServerRpc(10u, RpcReader___Server_SendQuestEntryState_375159588);
				RegisterObserversRpc(11u, RpcReader___Observers_ReceiveQuestEntryState_311789429);
				RegisterTargetRpc(12u, RpcReader___Target_ReceiveQuestEntryState_311789429);
				RegisterObserversRpc(13u, RpcReader___Observers_CreateDeaddropCollectionQuest_3895153758);
				RegisterTargetRpc(14u, RpcReader___Target_CreateDeaddropCollectionQuest_3895153758);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EQuests_002EQuestManagerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SendContractAccepted_1030683829(NetworkObject customer, ContractInfo contractData, bool track, string guid)
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
				writer.WriteNetworkObject(customer);
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(writer, contractData);
				writer.WriteBoolean(track);
				writer.WriteString(guid);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendContractAccepted_1030683829(NetworkObject customer, ContractInfo contractData, bool track, string guid)
		{
			GameDateTime expiry = default(GameDateTime);
			expiry.time = contractData.DeliveryWindow.WindowEndTime;
			expiry.elapsedDays = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays;
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime > contractData.DeliveryWindow.WindowEndTime)
			{
				expiry.elapsedDays++;
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Accepted_Contract_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Accepted_Contract_Count") + 1f).ToString());
			GameDateTime dateTime = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime();
			CreateContract_Networked(null, guid, track, customer, contractData, expiry, dateTime);
		}

		private void RpcReader___Server_SendContractAccepted_1030683829(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkObject customer = PooledReader0.ReadNetworkObject();
			ContractInfo contractData = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
			bool track = PooledReader0.ReadBoolean();
			string guid = PooledReader0.ReadString();
			if (base.IsServerInitialized)
			{
				RpcLogic___SendContractAccepted_1030683829(customer, contractData, track, guid);
			}
		}

		private void RpcWriter___Observers_CreateContract_Networked_1113640585(NetworkConnection conn, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
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
				writer.WriteBoolean(tracked);
				writer.WriteNetworkObject(customer);
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(writer, contractData);
				GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, expiry);
				GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, acceptTime);
				writer.WriteNetworkObject(dealerObj);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___CreateContract_Networked_1113640585(NetworkConnection conn, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
		{
			if (!GUIDManager.IsGUIDAlreadyRegistered(new Guid(guid)))
			{
				DeliveryLocation @object = GUIDManager.GetObject<DeliveryLocation>(new Guid(contractData.DeliveryLocationGUID));
				QuestEntryData questEntryData = new QuestEntryData(contractData.Products.GetCommaSeperatedString() + ", " + @object.LocationName, EQuestState.Inactive);
				string nameAddress = customer.GetComponent<Customer>().NPC.GetNameAddress();
				string description = nameAddress + " has requested a delivery of " + contractData.Products.GetCommaSeperatedString() + " " + @object.GetDescription() + " for " + MoneyManager.FormatAmount(contractData.Payment) + ".";
				Dealer dealer = null;
				if (dealerObj != null)
				{
					dealer = dealerObj.GetComponent<Dealer>();
				}
				CreateContract_Local("Deal for " + nameAddress, description, new QuestEntryData[1] { questEntryData }, guid, tracked, customer, contractData.Payment, contractData.Products, contractData.DeliveryLocationGUID, contractData.DeliveryWindow, contractData.Expires, expiry, contractData.PickupScheduleIndex, acceptTime, dealer);
			}
		}

		private void RpcReader___Observers_CreateContract_Networked_1113640585(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			bool tracked = PooledReader0.ReadBoolean();
			NetworkObject customer = PooledReader0.ReadNetworkObject();
			ContractInfo contractData = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
			GameDateTime expiry = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(PooledReader0);
			GameDateTime acceptTime = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(PooledReader0);
			NetworkObject dealerObj = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___CreateContract_Networked_1113640585(null, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
			}
		}

		private void RpcWriter___Target_CreateContract_Networked_1113640585(NetworkConnection conn, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
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
				writer.WriteBoolean(tracked);
				writer.WriteNetworkObject(customer);
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(writer, contractData);
				GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, expiry);
				GeneratedWriters___Internal.Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, acceptTime);
				writer.WriteNetworkObject(dealerObj);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_CreateContract_Networked_1113640585(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			bool tracked = PooledReader0.ReadBoolean();
			NetworkObject customer = PooledReader0.ReadNetworkObject();
			ContractInfo contractData = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
			GameDateTime expiry = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(PooledReader0);
			GameDateTime acceptTime = GeneratedReaders___Internal.Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(PooledReader0);
			NetworkObject dealerObj = PooledReader0.ReadNetworkObject();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateContract_Networked_1113640585(base.LocalConnection, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
			}
		}

		private void RpcWriter___Server_SendQuestAction_2848227116(string guid, EQuestAction action)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated(writer, action);
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendQuestAction_2848227116(string guid, EQuestAction action)
		{
			ReceiveQuestAction(null, guid, action);
		}

		private void RpcReader___Server_SendQuestAction_2848227116(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string guid = PooledReader0.ReadString();
			EQuestAction action = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendQuestAction_2848227116(guid, action);
			}
		}

		private void RpcWriter___Observers_ReceiveQuestAction_920727549(NetworkConnection conn, string guid, EQuestAction action)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated(writer, action);
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveQuestAction_920727549(NetworkConnection conn, string guid, EQuestAction action)
		{
			Quest @object = GUIDManager.GetObject<Quest>(new Guid(guid));
			if (@object == null)
			{
				Console.LogWarning("Failed to find quest with GUID: " + guid);
				return;
			}
			switch (action)
			{
			case EQuestAction.Begin:
				@object.Begin(network: false);
				break;
			case EQuestAction.Success:
				@object.Complete(network: false);
				break;
			case EQuestAction.Fail:
				@object.Fail(network: false);
				break;
			case EQuestAction.Expire:
				@object.Expire(network: false);
				break;
			case EQuestAction.Cancel:
				@object.Cancel(network: false);
				break;
			}
		}

		private void RpcReader___Observers_ReceiveQuestAction_920727549(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			EQuestAction action = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveQuestAction_920727549(null, guid, action);
			}
		}

		private void RpcWriter___Target_ReceiveQuestAction_920727549(NetworkConnection conn, string guid, EQuestAction action)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated(writer, action);
				SendTargetRpc(5u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ReceiveQuestAction_920727549(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			EQuestAction action = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveQuestAction_920727549(base.LocalConnection, guid, action);
			}
		}

		private void RpcWriter___Server_SendQuestState_4117703421(string guid, EQuestState state)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendServerRpc(6u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendQuestState_4117703421(string guid, EQuestState state)
		{
			ReceiveQuestState(null, guid, state);
		}

		private void RpcReader___Server_SendQuestState_4117703421(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string guid = PooledReader0.ReadString();
			EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendQuestState_4117703421(guid, state);
			}
		}

		private void RpcWriter___Observers_ReceiveQuestState_3887376304(NetworkConnection conn, string guid, EQuestState state)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveQuestState_3887376304(NetworkConnection conn, string guid, EQuestState state)
		{
			Quest @object = GUIDManager.GetObject<Quest>(new Guid(guid));
			if (@object == null)
			{
				Console.LogWarning("Failed to find quest with GUID: " + guid);
			}
			else
			{
				@object.SetQuestState(state, network: false);
			}
		}

		private void RpcReader___Observers_ReceiveQuestState_3887376304(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveQuestState_3887376304(null, guid, state);
			}
		}

		private void RpcWriter___Target_ReceiveQuestState_3887376304(NetworkConnection conn, string guid, EQuestState state)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendTargetRpc(8u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ReceiveQuestState_3887376304(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveQuestState_3887376304(base.LocalConnection, guid, state);
			}
		}

		private void RpcWriter___Target_SetQuestTracked_619441887(NetworkConnection conn, string guid, bool tracked)
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
				writer.WriteBoolean(tracked);
				SendTargetRpc(9u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetQuestTracked_619441887(NetworkConnection conn, string guid, bool tracked)
		{
			Quest @object = GUIDManager.GetObject<Quest>(new Guid(guid));
			if (@object == null)
			{
				Console.LogWarning("Failed to find quest with GUID: " + guid);
			}
			else
			{
				@object.SetIsTracked(tracked);
			}
		}

		private void RpcReader___Target_SetQuestTracked_619441887(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			bool tracked = PooledReader0.ReadBoolean();
			if (base.IsClientInitialized)
			{
				RpcLogic___SetQuestTracked_619441887(base.LocalConnection, guid, tracked);
			}
		}

		private void RpcWriter___Server_SendQuestEntryState_375159588(string guid, int entryIndex, EQuestState state)
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
				writer.WriteInt32(entryIndex);
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendServerRpc(10u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendQuestEntryState_375159588(string guid, int entryIndex, EQuestState state)
		{
			ReceiveQuestEntryState(null, guid, entryIndex, state);
		}

		private void RpcReader___Server_SendQuestEntryState_375159588(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string guid = PooledReader0.ReadString();
			int entryIndex = PooledReader0.ReadInt32();
			EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendQuestEntryState_375159588(guid, entryIndex, state);
			}
		}

		private void RpcWriter___Observers_ReceiveQuestEntryState_311789429(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
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
				writer.WriteInt32(entryIndex);
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendObserversRpc(11u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___ReceiveQuestEntryState_311789429(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
		{
			Quest @object = GUIDManager.GetObject<Quest>(new Guid(guid));
			if (@object == null)
			{
				Console.LogWarning("Failed to find quest with GUID: " + guid);
			}
			else
			{
				@object.SetQuestEntryState(entryIndex, state, network: false);
			}
		}

		private void RpcReader___Observers_ReceiveQuestEntryState_311789429(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			int entryIndex = PooledReader0.ReadInt32();
			EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___ReceiveQuestEntryState_311789429(null, guid, entryIndex, state);
			}
		}

		private void RpcWriter___Target_ReceiveQuestEntryState_311789429(NetworkConnection conn, string guid, int entryIndex, EQuestState state)
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
				writer.WriteInt32(entryIndex);
				GeneratedWriters___Internal.Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(writer, state);
				SendTargetRpc(12u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_ReceiveQuestEntryState_311789429(PooledReader PooledReader0, Channel channel)
		{
			string guid = PooledReader0.ReadString();
			int entryIndex = PooledReader0.ReadInt32();
			EQuestState state = GeneratedReaders___Internal.Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(PooledReader0);
			if (base.IsClientInitialized)
			{
				RpcLogic___ReceiveQuestEntryState_311789429(base.LocalConnection, guid, entryIndex, state);
			}
		}

		private void RpcWriter___Observers_CreateDeaddropCollectionQuest_3895153758(NetworkConnection conn, string dropGUID, string guidString = "")
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
				writer.WriteString(dropGUID);
				writer.WriteString(guidString);
				SendObserversRpc(13u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public void RpcLogic___CreateDeaddropCollectionQuest_3895153758(NetworkConnection conn, string dropGUID, string guidString = "")
		{
			CreateDeaddropCollectionQuest(dropGUID, guidString);
		}

		private void RpcReader___Observers_CreateDeaddropCollectionQuest_3895153758(PooledReader PooledReader0, Channel channel)
		{
			string dropGUID = PooledReader0.ReadString();
			string guidString = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___CreateDeaddropCollectionQuest_3895153758(null, dropGUID, guidString);
			}
		}

		private void RpcWriter___Target_CreateDeaddropCollectionQuest_3895153758(NetworkConnection conn, string dropGUID, string guidString = "")
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
				writer.WriteString(dropGUID);
				writer.WriteString(guidString);
				SendTargetRpc(14u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_CreateDeaddropCollectionQuest_3895153758(PooledReader PooledReader0, Channel channel)
		{
			string dropGUID = PooledReader0.ReadString();
			string guidString = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___CreateDeaddropCollectionQuest_3895153758(base.LocalConnection, dropGUID, guidString);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EQuests_002EQuestManager_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			InitializeSaveable();
			Quest[] componentsInChildren = QuestContainer.GetComponentsInChildren<Quest>();
			foreach (Quest quest in componentsInChildren)
			{
				if (!DefaultQuests.Contains(quest))
				{
					Console.LogError("Quest " + quest.GetQuestTitle() + " is not in the default quests list!");
				}
			}
		}
	}
}
