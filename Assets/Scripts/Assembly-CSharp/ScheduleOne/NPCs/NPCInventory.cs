using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.NPCs
{
	public class NPCInventory : NetworkBehaviour, IItemSlotOwner
	{
		public InteractableObject PickpocketIntObj;

		public const float COOLDOWN = 30f;

		[Header("Settings")]
		public int SlotCount = 5;

		public bool CanBePickpocketed = true;

		public bool ClearInventoryEachNight = true;

		public ItemDefinition[] TestItems;

		[Header("Random cash")]
		public bool RandomCash = true;

		public int RandomCashMin;

		public int RandomCashMax = 100;

		[Header("Random items")]
		public bool RandomItems = true;

		public StorableItemDefinition[] RandomItemDefinitions;

		public int RandomItemMin = -1;

		public int RandomItemMax = 2;

		private NPC npc;

		public UnityEvent onContentsChanged;

		private float timeOnLastExpire = -100f;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCInventoryAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCInventoryAssembly_002DCSharp_002Edll_Excuted;

		public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();


		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCInventory_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected virtual void Start()
		{
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.RemoveListener(OnSleepStart);
			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.AddListener(OnSleepStart);
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (!connection.IsLocalClient)
			{
				((IItemSlotOwner)this).SendItemsToClient(connection);
			}
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
			{
				NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance._onSleepStart.RemoveListener(OnSleepStart);
			}
		}

		protected virtual void OnSleepStart()
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (ClearInventoryEachNight)
			{
				foreach (ItemSlot itemSlot in ItemSlots)
				{
					itemSlot.ClearStoredInstance();
				}
			}
			if (GetTotalItemCount() >= 3)
			{
				return;
			}
			if (RandomCash)
			{
				int num = UnityEngine.Random.Range(RandomCashMin, RandomCashMax);
				if (num > 0)
				{
					CashInstance cashInstance = NetworkSingleton<MoneyManager>.Instance.GetCashInstance(num);
					InsertItem(cashInstance);
				}
			}
			if (RandomItems)
			{
				int num2 = UnityEngine.Random.Range(RandomItemMin, RandomItemMax + 1);
				for (int i = 0; i < num2; i++)
				{
					ItemInstance defaultInstance = RandomItemDefinitions[UnityEngine.Random.Range(0, RandomItemDefinitions.Length)].GetDefaultInstance();
					InsertItem(defaultInstance);
				}
			}
		}

		public int GetItemCount()
		{
			return ((IItemSlotOwner)this).GetTotalItemCount();
		}

		public int _GetItemAmount(string id)
		{
			return ((IItemSlotOwner)this).GetItemCount(id);
		}

		public bool CanItemFit(ItemInstance item, int quantity = 1)
		{
			return HowManyCanFit(item) >= quantity;
		}

		public int HowManyCanFit(ItemInstance item)
		{
			if (item == null)
			{
				return 0;
			}
			int num = 0;
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (ItemSlots[i] != null && !ItemSlots[i].IsLocked && !ItemSlots[i].IsAddLocked)
				{
					if (ItemSlots[i].ItemInstance == null)
					{
						num += item.StackLimit;
					}
					else if (ItemSlots[i].ItemInstance.CanStackWith(item))
					{
						num += item.StackLimit - ItemSlots[i].ItemInstance.Quantity;
					}
				}
			}
			return num;
		}

		public void InsertItem(ItemInstance item, bool network = true)
		{
			if (!CanItemFit(item, item.Quantity))
			{
				Console.LogWarning("StorageEntity InsertItem() called but CanItemFit() returned false");
				return;
			}
			int num = item.Quantity;
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (!ItemSlots[i].IsLocked && !ItemSlots[i].IsAddLocked)
				{
					if (ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.CanStackWith(item))
					{
						int num2 = Mathf.Min(item.StackLimit - ItemSlots[i].ItemInstance.Quantity, num);
						num -= num2;
						ItemSlots[i].ChangeQuantity(num2, network);
					}
					if (num <= 0)
					{
						return;
					}
				}
			}
			for (int j = 0; j < ItemSlots.Count; j++)
			{
				if (!ItemSlots[j].IsLocked && !ItemSlots[j].IsAddLocked)
				{
					if (ItemSlots[j].ItemInstance == null)
					{
						num -= item.StackLimit;
						ItemSlots[j].SetStoredItem(item, !network);
						break;
					}
					if (num <= 0)
					{
						break;
					}
				}
			}
		}

		public ItemInstance GetFirstItem(string id)
		{
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (ItemSlots[i].ItemInstance != null && ItemSlots[i].ItemInstance.ID == id)
				{
					return ItemSlots[i].ItemInstance;
				}
			}
			return null;
		}

		protected virtual void InventoryContentsChanged()
		{
			if (onContentsChanged != null)
			{
				onContentsChanged.Invoke();
			}
		}

		public int GetTotalItemCount()
		{
			int num = 0;
			for (int i = 0; i < ItemSlots.Count; i++)
			{
				if (ItemSlots[i].ItemInstance != null)
				{
					num += ItemSlots[i].ItemInstance.Quantity;
				}
			}
			return num;
		}

		public void Hovered()
		{
			if (CanPickpocket())
			{
				PickpocketIntObj.SetMessage("Pickpocket");
				PickpocketIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			}
			else
			{
				PickpocketIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
			}
		}

		public void Interacted()
		{
			if (CanPickpocket())
			{
				StartPickpocket();
			}
		}

		private void StartPickpocket()
		{
			Singleton<PickpocketScreen>.Instance.Open(npc);
		}

		public void ExpirePickpocket()
		{
			timeOnLastExpire = Time.time;
		}

		private bool CanPickpocket()
		{
			if (!CanBePickpocketed)
			{
				return false;
			}
			if (!PlayerSingleton<PlayerMovement>.Instance.isCrouched)
			{
				return false;
			}
			if (Player.Local.CrimeData.CurrentPursuitLevel != 0)
			{
				return false;
			}
			if (Time.time - timeOnLastExpire < 30f)
			{
				return false;
			}
			if (!npc.IsConscious)
			{
				return false;
			}
			if (npc.behaviour.CallPoliceBehaviour.Active)
			{
				return false;
			}
			if (npc.behaviour.CombatBehaviour.Active)
			{
				return false;
			}
			if (npc.behaviour.FacePlayerBehaviour.Active)
			{
				return false;
			}
			if (npc.behaviour.FleeBehaviour.Active)
			{
				return false;
			}
			if (npc.behaviour.GenericDialogueBehaviour.Active)
			{
				return false;
			}
			if (npc.behaviour.StationaryBehaviour.Active)
			{
				return false;
			}
			if (npc.behaviour.RequestProductBehaviour.Active)
			{
				return false;
			}
			if (GameManager.IS_TUTORIAL)
			{
				return false;
			}
			return true;
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
			RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc(RunLocally = true)]
		private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
				RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			}
			else
			{
				RpcWriter___Target_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
				RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
			}
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetItemSlotQuantity(int itemSlotIndex, int quantity)
		{
			RpcWriter___Server_SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
			RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
		}

		[ObserversRpc(RunLocally = true)]
		private void SetItemSlotQuantity_Internal(int itemSlotIndex, int quantity)
		{
			RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
			RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
		}

		[ServerRpc(RunLocally = true, RequireOwnership = false)]
		public void SetSlotLocked(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			RpcWriter___Server_SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			RpcLogic___SetSlotLocked_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
		}

		[TargetRpc(RunLocally = true)]
		[ObserversRpc(RunLocally = true)]
		private void SetSlotLocked_Internal(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
				RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			}
			else
			{
				RpcWriter___Target_SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
				RpcLogic___SetSlotLocked_Internal_3170825843(conn, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCInventoryAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ENPCInventoryAssembly_002DCSharp_002Edll_Excuted = true;
				RegisterServerRpc(0u, RpcReader___Server_SetStoredInstance_2652194801);
				RegisterObserversRpc(1u, RpcReader___Observers_SetStoredInstance_Internal_2652194801);
				RegisterTargetRpc(2u, RpcReader___Target_SetStoredInstance_Internal_2652194801);
				RegisterServerRpc(3u, RpcReader___Server_SetItemSlotQuantity_1692629761);
				RegisterObserversRpc(4u, RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761);
				RegisterServerRpc(5u, RpcReader___Server_SetSlotLocked_3170825843);
				RegisterTargetRpc(6u, RpcReader___Target_SetSlotLocked_Internal_3170825843);
				RegisterObserversRpc(7u, RpcReader___Observers_SetSlotLocked_Internal_3170825843);
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCInventoryAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ENPCInventoryAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
				writer.WriteNetworkConnection(conn);
				writer.WriteInt32(itemSlotIndex);
				writer.WriteItemInstance(instance);
				SendServerRpc(0u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			if (conn == null || conn.ClientId == -1)
			{
				SetStoredInstance_Internal(null, itemSlotIndex, instance);
			}
			else
			{
				SetStoredInstance_Internal(conn, itemSlotIndex, instance);
			}
		}

		private void RpcReader___Server_SetStoredInstance_2652194801(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			int itemSlotIndex = PooledReader0.ReadInt32();
			ItemInstance instance = PooledReader0.ReadItemInstance();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetStoredInstance_2652194801(conn2, itemSlotIndex, instance);
			}
		}

		private void RpcWriter___Observers_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteItemInstance(instance);
				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
		{
			if (instance != null)
			{
				ItemSlots[itemSlotIndex].SetStoredItem(instance, _internal: true);
			}
			else
			{
				ItemSlots[itemSlotIndex].ClearStoredInstance(_internal: true);
			}
		}

		private void RpcReader___Observers_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			ItemInstance instance = PooledReader0.ReadItemInstance();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetStoredInstance_Internal_2652194801(null, itemSlotIndex, instance);
			}
		}

		private void RpcWriter___Target_SetStoredInstance_Internal_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteItemInstance(instance);
				SendTargetRpc(2u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetStoredInstance_Internal_2652194801(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			ItemInstance instance = PooledReader0.ReadItemInstance();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetStoredInstance_Internal_2652194801(base.LocalConnection, itemSlotIndex, instance);
			}
		}

		private void RpcWriter___Server_SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteInt32(quantity);
				SendServerRpc(3u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetItemSlotQuantity_1692629761(int itemSlotIndex, int quantity)
		{
			SetItemSlotQuantity_Internal(itemSlotIndex, quantity);
		}

		private void RpcReader___Server_SetItemSlotQuantity_1692629761(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetItemSlotQuantity_1692629761(itemSlotIndex, quantity);
			}
		}

		private void RpcWriter___Observers_SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteInt32(quantity);
				SendObserversRpc(4u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetItemSlotQuantity_Internal_1692629761(int itemSlotIndex, int quantity)
		{
			ItemSlots[itemSlotIndex].SetQuantity(quantity, _internal: true);
		}

		private void RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			int quantity = PooledReader0.ReadInt32();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetItemSlotQuantity_Internal_1692629761(itemSlotIndex, quantity);
			}
		}

		private void RpcWriter___Server_SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
				writer.WriteNetworkConnection(conn);
				writer.WriteInt32(itemSlotIndex);
				writer.WriteBoolean(locked);
				writer.WriteNetworkObject(lockOwner);
				writer.WriteString(lockReason);
				SendServerRpc(5u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetSlotLocked_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			if (conn == null || conn.ClientId == -1)
			{
				SetSlotLocked_Internal(null, itemSlotIndex, locked, lockOwner, lockReason);
			}
			else
			{
				SetSlotLocked_Internal(conn, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		private void RpcReader___Server_SetSlotLocked_3170825843(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			NetworkConnection conn2 = PooledReader0.ReadNetworkConnection();
			int itemSlotIndex = PooledReader0.ReadInt32();
			bool locked = PooledReader0.ReadBoolean();
			NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
			string lockReason = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SetSlotLocked_3170825843(conn2, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		private void RpcWriter___Target_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteBoolean(locked);
				writer.WriteNetworkObject(lockOwner);
				writer.WriteString(lockReason);
				SendTargetRpc(6u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcLogic___SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
		{
			if (locked)
			{
				ItemSlots[itemSlotIndex].ApplyLock(lockOwner, lockReason, _internal: true);
			}
			else
			{
				ItemSlots[itemSlotIndex].RemoveLock(_internal: true);
			}
		}

		private void RpcReader___Target_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			bool locked = PooledReader0.ReadBoolean();
			NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
			string lockReason = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetSlotLocked_Internal_3170825843(base.LocalConnection, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		private void RpcWriter___Observers_SetSlotLocked_Internal_3170825843(NetworkConnection conn, int itemSlotIndex, bool locked, NetworkObject lockOwner, string lockReason)
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
				writer.WriteInt32(itemSlotIndex);
				writer.WriteBoolean(locked);
				writer.WriteNetworkObject(lockOwner);
				writer.WriteString(lockReason);
				SendObserversRpc(7u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcReader___Observers_SetSlotLocked_Internal_3170825843(PooledReader PooledReader0, Channel channel)
		{
			int itemSlotIndex = PooledReader0.ReadInt32();
			bool locked = PooledReader0.ReadBoolean();
			NetworkObject lockOwner = PooledReader0.ReadNetworkObject();
			string lockReason = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetSlotLocked_Internal_3170825843(null, itemSlotIndex, locked, lockOwner, lockReason);
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002ENPCs_002ENPCInventory_Assembly_002DCSharp_002Edll()
		{
			for (int i = 0; i < SlotCount; i++)
			{
				ItemSlot itemSlot = new ItemSlot();
				itemSlot.SetSlotOwner(this);
				itemSlot.onItemDataChanged = (Action)Delegate.Combine(itemSlot.onItemDataChanged, new Action(InventoryContentsChanged));
			}
			if (Application.isEditor)
			{
				ItemDefinition[] testItems = TestItems;
				for (int j = 0; j < testItems.Length; j++)
				{
					ItemInstance defaultInstance = testItems[j].GetDefaultInstance();
					InsertItem(defaultInstance);
				}
			}
			npc = GetComponent<NPC>();
			PickpocketIntObj.onHovered.AddListener(Hovered);
			PickpocketIntObj.onInteractStart.AddListener(Interacted);
		}
	}
}
