using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Quests;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.UI.Shop;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Economy
{
	public class Supplier : NPC
	{
		public enum ESupplierStatus
		{
			Idle = 0,
			PreppingDeadDrop = 1,
			Meeting = 2
		}

		public const float MEETUP_RELATIONSHIP_REQUIREMENT = 4f;

		public const int MEETUP_DURATION_MINS = 360;

		public const int MEETING_COOLDOWN_MINS = 720;

		public const int DEADDROP_WAIT_PER_ITEM = 30;

		public const int DEADDROP_MAX_WAIT = 360;

		public const int DEADDROP_ITEM_LIMIT = 20;

		public static Color32 SupplierLabelColor = new Color32(byte.MaxValue, 150, 145, byte.MaxValue);

		[Header("Supplier Settings")]
		public float MinOrderLimit = 100f;

		public float MaxOrderLimit = 500f;

		public PhoneShopInterface.Listing[] OnlineShopItems;

		[Header("References")]
		public ShopInterface Shop;

		public SupplierStash Stash;

		public UnityEvent onDeaddropReady;

		private int minsSinceMeetingStart = -1;

		private int minsSinceLastMeetingEnd = 720;

		private SupplierLocation currentLocation;

		private DialogueController dialogueController;

		private DialogueController.GreetingOverride meetingGreeting;

		private DialogueController.DialogueChoice meetingChoice;

		[SyncVar]
		public float debt;

		[SyncVar]
		public bool deadDropPreparing;

		private StringIntPair[] deaddropItems;

		private int minsSinceDeaddropOrder;

		private bool repaymentReminderSent;

		public SyncVar<float> syncVar___debt;

		public SyncVar<bool> syncVar___deadDropPreparing;

		private bool NetworkInitialize___EarlyScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted;

		public ESupplierStatus Status { get; private set; }

		public float Debt => SyncAccessor_debt;

		public int minsUntilDeaddropReady { get; private set; } = -1;


		public float SyncAccessor_debt
		{
			get
			{
				return debt;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					debt = value;
				}
				if (Application.isPlaying)
				{
					syncVar___debt.SetValue(value, value != 0);
				}
			}
		}

		public bool SyncAccessor_deadDropPreparing
		{
			get
			{
				return deadDropPreparing;
			}
			set
			{
				if (value || !base.IsServerInitialized)
				{
					deadDropPreparing = value;
				}
				if (Application.isPlaying)
				{
					syncVar___deadDropPreparing.SetValue(value, value);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EEconomy_002ESupplier_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void Start()
		{
			base.Start();
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SupplierUnlocked));
			NPCRelationData relationData2 = RelationData;
			relationData2.onRelationshipChange = (Action<float>)Delegate.Combine(relationData2.onRelationshipChange, new Action<float>(RelationshipChange));
			string orderCompleteDialogue = dialogueHandler.Database.GetLine(EDialogueModule.Generic, "meeting_order_complete");
			Shop.onOrderCompleted.AddListener(delegate
			{
				dialogueHandler.ShowWorldspaceDialogue(orderCompleteDialogue, 3f);
			});
			dialogueController = dialogueHandler.GetComponent<DialogueController>();
			meetingGreeting = new DialogueController.GreetingOverride();
			meetingGreeting.Greeting = dialogueHandler.Database.GetLine(EDialogueModule.Generic, "supplier_meeting_greeting");
			meetingGreeting.PlayVO = true;
			meetingGreeting.VOType = EVOLineType.Question;
			dialogueController.AddGreetingOverride(meetingGreeting);
			meetingChoice = new DialogueController.DialogueChoice();
			meetingChoice.ChoiceText = "Yes";
			meetingChoice.onChoosen.AddListener(delegate
			{
				Shop.SetIsOpen(isOpen: true);
			});
			meetingChoice.Enabled = false;
			dialogueController.AddDialogueChoice(meetingChoice);
			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(OnTimeSkip));
			PhoneShopInterface.Listing[] onlineShopItems = OnlineShopItems;
			foreach (PhoneShopInterface.Listing listing in onlineShopItems)
			{
				if ((listing.Item as StorableItemDefinition).RequiresLevelToPurchase)
				{
					NetworkSingleton<LevelManager>.Instance.AddUnlockable(new Unlockable((listing.Item as StorableItemDefinition).RequiredRank, listing.Item.Name, listing.Item.Icon));
				}
			}
			TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance2.onHourPass = (Action)Delegate.Remove(instance2.onHourPass, new Action(HourPass));
			TimeManager instance3 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
			instance3.onHourPass = (Action)Delegate.Combine(instance3.onHourPass, new Action(HourPass));
		}

		protected override void MinPass()
		{
			base.MinPass();
			minsSinceDeaddropOrder++;
			if (Status == ESupplierStatus.Meeting)
			{
				minsSinceMeetingStart++;
				minsSinceLastMeetingEnd = 0;
				if (minsSinceMeetingStart > 360)
				{
					EndMeeting();
				}
			}
			else
			{
				minsSinceLastMeetingEnd++;
			}
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (SyncAccessor_deadDropPreparing)
			{
				minsUntilDeaddropReady--;
				if (minsUntilDeaddropReady <= 0)
				{
					CompleteDeaddrop();
				}
			}
			if (SyncAccessor_debt > 0f && !Stash.Storage.IsOpened && Stash.CashAmount > 1f && minsSinceDeaddropOrder > 3)
			{
				TryRecoverDebt();
			}
		}

		protected void HourPass()
		{
			if (InstanceFinder.IsServer && !repaymentReminderSent && SyncAccessor_debt > GetDeadDropLimit() * 0.5f && !SyncAccessor_deadDropPreparing)
			{
				float num = 1f / 48f;
				if (UnityEngine.Random.Range(0f, 1f) < num)
				{
					SendDebtReminder();
				}
			}
		}

		private void OnTimeSkip(int minsSlept)
		{
			if (SyncAccessor_deadDropPreparing)
			{
				minsUntilDeaddropReady -= minsSlept;
			}
		}

		public void MeetAtLocation(SupplierLocation location, int expireIn)
		{
		}

		public void EndMeeting()
		{
			Console.Log("Meeting ended");
			Status = ESupplierStatus.Idle;
			minsSinceMeetingStart = -1;
			meetingGreeting.ShouldShow = false;
			meetingChoice.Enabled = false;
			currentLocation.SetActiveSupplier(null);
			SetVisible(visible: false);
		}

		protected virtual void SupplierUnlocked(NPCRelationData.EUnlockType type, bool notify)
		{
			if (notify)
			{
				SetUnlockMessage();
			}
		}

		protected virtual void RelationshipChange(float change)
		{
			if (InstanceFinder.IsServer)
			{
				_ = Singleton<LoadManager>.Instance.IsLoading;
			}
		}

		public void SetUnlockMessage()
		{
			if (InstanceFinder.IsServer)
			{
				DialogueChain chain = dialogueHandler.Database.GetChain(EDialogueModule.Generic, "supplier_unlocked");
				if (chain != null)
				{
					base.MSGConversation.SendMessageChain(chain.GetMessageChain());
				}
			}
		}

		protected override void CreateMessageConversation()
		{
			base.CreateMessageConversation();
			SendableMessage sendableMessage = base.MSGConversation.CreateSendableMessage("I need to order a dead drop");
			sendableMessage.IsValidCheck = IsDeadDropValid;
			sendableMessage.disableDefaultSendBehaviour = true;
			sendableMessage.onSelected = (Action)Delegate.Combine(sendableMessage.onSelected, new Action(DeaddropRequested));
			SendableMessage sendableMessage2 = base.MSGConversation.CreateSendableMessage("I want to pay off my debt");
			sendableMessage2.onSent = (Action)Delegate.Combine(sendableMessage2.onSent, new Action(PayDebtRequested));
		}

		protected virtual void DeaddropRequested()
		{
			float orderLimit = Mathf.Max(GetDeadDropLimit() - SyncAccessor_debt, 0f);
			PlayerSingleton<MessagesApp>.Instance.PhoneShopInterface.Open("Request Dead Drop", "Select items to order from " + FirstName, base.MSGConversation, OnlineShopItems.ToList(), orderLimit, SyncAccessor_debt, DeaddropConfirmed);
		}

		protected virtual void DeaddropConfirmed(List<PhoneShopInterface.CartEntry> cart, float totalPrice)
		{
			if (SyncAccessor_deadDropPreparing)
			{
				Console.LogWarning("Already preparing a dead drop");
				return;
			}
			int num = cart.Sum((PhoneShopInterface.CartEntry x) => x.Quantity);
			StringIntPair[] array = new StringIntPair[cart.Count];
			for (int i = 0; i < cart.Count; i++)
			{
				array[i] = new StringIntPair(cart[i].Listing.Item.ID, cart[i].Quantity);
			}
			string text = "I need a dead drop:\n";
			for (int j = 0; j < cart.Count; j++)
			{
				if (cart[j].Quantity > 0)
				{
					text = text + cart[j].Quantity + "x " + cart[j].Listing.Item.Name;
					if (j < cart.Count - 1)
					{
						text += "\n";
					}
				}
			}
			base.MSGConversation.SendMessage(new Message(text, Message.ESenderType.Player));
			int num2 = Mathf.Clamp(num * 30, 30, 360);
			string line = dialogueHandler.Database.GetLine(EDialogueModule.Supplier, "deaddrop_requested");
			if (num2 < 60)
			{
				line = line.Replace("<TIME>", num2 + ((num2 == 1) ? " min" : " mins"));
			}
			else
			{
				float num3 = Mathf.FloorToInt((float)num2 / 60f);
				float num4 = (float)num2 - num3 * 60f;
				string text2 = num3 + ((num3 == 1f) ? " hour" : " hours");
				if (num4 > 0f)
				{
					text2 = text2 + " " + num4 + " min";
				}
				line = line.Replace("<TIME>", text2);
			}
			base.MSGConversation.SendMessageChain(new MessageChain
			{
				Messages = new List<string> { line },
				id = UnityEngine.Random.Range(int.MinValue, int.MaxValue)
			}, 0.5f, notify: false);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Deaddrops_Ordered", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Deaddrops_Ordered") + 1f).ToString());
			SetDeaddrop(array, num2);
			minsSinceDeaddropOrder = 0;
			ChangeDebt(totalPrice);
		}

		[ServerRpc(RequireOwnership = false)]
		private void SetDeaddrop(StringIntPair[] items, int minsUntilReady)
		{
			RpcWriter___Server_SetDeaddrop_3971994486(items, minsUntilReady);
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		private void ChangeDebt(float amount)
		{
			RpcWriter___Server_ChangeDebt_431000436(amount);
			RpcLogic___ChangeDebt_431000436(amount);
		}

		private void TryRecoverDebt()
		{
			float num = Mathf.Min(SyncAccessor_debt, Stash.CashAmount);
			if (num > 0f)
			{
				Debug.Log("Recovering debt: " + num);
				float num2 = SyncAccessor_debt;
				Stash.RemoveCash(num);
				ChangeDebt(0f - num);
				RelationData.ChangeRelationship(num / MaxOrderLimit * 0.5f);
				float num3 = num2 - num;
				string text = "I've recieved " + MoneyManager.FormatAmount(num) + " cash from you.";
				text = ((!(num3 <= 0f)) ? (text + " Your debt is now " + MoneyManager.FormatAmount(num3)) : (text + " Your debt is now paid off."));
				repaymentReminderSent = false;
				base.MSGConversation.SendMessageChain(new MessageChain
				{
					Messages = new List<string> { text },
					id = UnityEngine.Random.Range(int.MinValue, int.MaxValue)
				});
			}
		}

		private void CompleteDeaddrop()
		{
			Console.Log("Dead drop ready");
			DeadDrop randomEmptyDrop = DeadDrop.GetRandomEmptyDrop();
			if (randomEmptyDrop == null)
			{
				Console.LogError("No empty dead drop locations");
				return;
			}
			StringIntPair[] array = deaddropItems;
			foreach (StringIntPair stringIntPair in array)
			{
				ItemDefinition item = Registry.GetItem(stringIntPair.String);
				if (item == null)
				{
					Console.LogError("Item not found: " + stringIntPair.String);
					continue;
				}
				int num = stringIntPair.Int;
				while (num > 0)
				{
					int num2 = Mathf.Min(num, item.StackLimit);
					ItemInstance defaultInstance = item.GetDefaultInstance(num2);
					randomEmptyDrop.Storage.InsertItem(defaultInstance);
					num -= num2;
				}
			}
			string line = dialogueHandler.Database.GetLine(EDialogueModule.Supplier, "deaddrop_ready");
			line = line.Replace("<LOCATION>", randomEmptyDrop.DeadDropDescription);
			base.MSGConversation.SendMessageChain(new MessageChain
			{
				Messages = new List<string> { line },
				id = UnityEngine.Random.Range(int.MinValue, int.MaxValue)
			});
			syncVar___deadDropPreparing.SetValue(false, true);
			minsUntilDeaddropReady = -1;
			deaddropItems = null;
			if (onDeaddropReady != null)
			{
				onDeaddropReady.Invoke();
			}
			string guidString = GUIDManager.GenerateUniqueGUID().ToString();
			NetworkSingleton<QuestManager>.Instance.CreateDeaddropCollectionQuest(null, randomEmptyDrop.GUID.ToString(), guidString);
			SetDeaddrop(null, -1);
		}

		private void SendDebtReminder()
		{
			repaymentReminderSent = true;
			DialogueChain chain = dialogueHandler.Database.GetChain(EDialogueModule.Supplier, "supplier_request_repayment");
			chain.Lines[0] = chain.Lines[0].Replace("<DEBT>", "<color=#46CB4F>" + MoneyManager.FormatAmount(SyncAccessor_debt) + "</color>");
			base.MSGConversation.SendMessageChain(chain.GetMessageChain());
		}

		protected virtual void MeetupRequested()
		{
			SupplierLocation appropriateLocation = GetAppropriateLocation();
			string line = dialogueHandler.Database.GetLine(EDialogueModule.Generic, "supplier_meet_confirm");
			line = line.Replace("<LOCATION>", appropriateLocation.LocationDescription);
			MessageChain messageChain = new MessageChain();
			messageChain.Messages.Add(line);
			messageChain.id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			base.MSGConversation.SendMessageChain(messageChain, 0.5f);
			MeetAtLocation(appropriateLocation, 360);
		}

		protected virtual void PayDebtRequested()
		{
			if (InstanceFinder.IsServer)
			{
				MessageChain messageChain = new MessageChain();
				messageChain.Messages.Add("You can pay off your debt by placing cash in my stash. It's " + Stash.locationDescription + ".");
				messageChain.id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
				base.MSGConversation.SendMessageChain(messageChain, 0.5f);
			}
		}

		protected SupplierLocation GetAppropriateLocation()
		{
			List<SupplierLocation> list = new List<SupplierLocation>();
			list.AddRange(SupplierLocation.AllLocations);
			foreach (SupplierLocation allLocation in SupplierLocation.AllLocations)
			{
				if (allLocation.IsOccupied)
				{
					list.Remove(allLocation);
				}
			}
			foreach (SupplierLocation allLocation2 in SupplierLocation.AllLocations)
			{
				foreach (Player player in Player.PlayerList)
				{
					if (Vector3.Distance(allLocation2.transform.position, player.Avatar.CenterPoint) < 30f)
					{
						list.Remove(allLocation2);
					}
				}
			}
			if (list.Count == 0)
			{
				Console.LogError("No available locations for supplier");
				return null;
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		private bool IsDeadDropValid(SendableMessage message, out string invalidReason)
		{
			invalidReason = string.Empty;
			if (SyncAccessor_deadDropPreparing)
			{
				invalidReason = "Already waiting for a dead drop";
				return false;
			}
			return true;
		}

		private bool IsMeetupValid(SendableMessage message, out string invalidReason)
		{
			if (RelationData.RelationDelta < 4f)
			{
				invalidReason = "Insufficient trust";
				return false;
			}
			if (Status != 0)
			{
				invalidReason = "Busy";
				return false;
			}
			if (minsSinceLastMeetingEnd < 720)
			{
				invalidReason = "Too soon since last meeting";
				return false;
			}
			invalidReason = "Unavailable in demo";
			return false;
		}

		public virtual float GetDeadDropLimit()
		{
			return Mathf.Lerp(MinOrderLimit, MaxOrderLimit, RelationData.RelationDelta / 5f);
		}

		public override string GetSaveString()
		{
			return new SupplierData(ID, minsSinceMeetingStart, minsSinceLastMeetingEnd, SyncAccessor_debt, minsUntilDeaddropReady, deaddropItems, repaymentReminderSent).GetJson();
		}

		public override void Load(NPCData data, string containerPath)
		{
			base.Load(data, containerPath);
			if (((ISaveable)this).TryLoadFile(containerPath, "NPC", out string contents))
			{
				SupplierData supplierData = null;
				try
				{
					supplierData = JsonUtility.FromJson<SupplierData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogWarning("Failed to deserialize character data: " + ex.Message);
					return;
				}
				minsSinceMeetingStart = supplierData.timeSinceMeetingStart;
				minsSinceLastMeetingEnd = supplierData.timeSinceLastMeetingEnd;
				syncVar___debt.SetValue(supplierData.debt, true);
				minsUntilDeaddropReady = supplierData.minsUntilDeadDropReady;
				if (minsUntilDeaddropReady > 0)
				{
                    syncVar___deadDropPreparing.SetValue(true, true);
				}
				if (supplierData.deaddropItems != null)
				{
					deaddropItems = supplierData.deaddropItems.ToArray();
				}
				repaymentReminderSent = supplierData.debtReminderSent;
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___deadDropPreparing = new SyncVar<bool>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, deadDropPreparing);
				syncVar___debt = new SyncVar<float>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, debt);
				RegisterServerRpc(34u, RpcReader___Server_SetDeaddrop_3971994486);
				RegisterServerRpc(35u, RpcReader___Server_ChangeDebt_431000436);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEconomy_002ESupplier);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEconomy_002ESupplierAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___deadDropPreparing.SetRegistered();
				syncVar___debt.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_SetDeaddrop_3971994486(StringIntPair[] items, int minsUntilReady)
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
				GeneratedWriters___Internal.Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated(writer, items);
				writer.WriteInt32(minsUntilReady);
				SendServerRpc(34u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___SetDeaddrop_3971994486(StringIntPair[] items, int minsUntilReady)
		{
			if (items != null)
			{
				minsSinceDeaddropOrder = 0;
				syncVar___deadDropPreparing.SetValue(true, true);
			}
			else
			{
                syncVar___deadDropPreparing.SetValue(false, true);
			}
			minsUntilDeaddropReady = minsUntilReady;
			deaddropItems = items;
		}

		private void RpcReader___Server_SetDeaddrop_3971994486(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			StringIntPair[] items = GeneratedReaders___Internal.Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
			int minsUntilReady = PooledReader0.ReadInt32();
			if (base.IsServerInitialized)
			{
				RpcLogic___SetDeaddrop_3971994486(items, minsUntilReady);
			}
		}

		private void RpcWriter___Server_ChangeDebt_431000436(float amount)
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
				writer.WriteSingle(amount);
				SendServerRpc(35u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		private void RpcLogic___ChangeDebt_431000436(float amount)
		{
			syncVar___debt.SetValue(Mathf.Clamp(SyncAccessor_debt + amount, 0f, GetDeadDropLimit()), true);
		}

		private void RpcReader___Server_ChangeDebt_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float amount = PooledReader0.ReadSingle();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___ChangeDebt_431000436(amount);
			}
		}

		public virtual bool ReadSyncVar___ScheduleOne_002EEconomy_002ESupplier(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
                            syncVar___deadDropPreparing.SetValue(syncVar___deadDropPreparing.GetValue(calledByUser: true), true);
					return true;
				}
				bool value2 = PooledReader0.ReadBoolean();
                        syncVar___deadDropPreparing.SetValue(value2, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar___debt.SetValue(syncVar___debt.GetValue(calledByUser: true), true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
                        syncVar___debt.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EEconomy_002ESupplier_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
		}
	}
}
