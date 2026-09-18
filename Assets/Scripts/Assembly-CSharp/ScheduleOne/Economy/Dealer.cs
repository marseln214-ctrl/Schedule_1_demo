using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ScheduleOne.Map;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.NPCs.Schedules;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Quests;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Economy
{
	public class Dealer : NPC, IItemSlotOwner
	{
		public const int MAX_CUSTOMERS = 6;

		public const int DEAL_ARRIVAL_DELAY = 30;

		public const int MIN_TRAVEL_TIME = 15;

		public const int MAX_TRAVEL_TIME = 360;

		public const int OVERFLOW_SLOT_COUNT = 10;

		public const float CASH_REMINDER_THRESHOLD = 500f;

		public static Action<Dealer> onDealerRecruited;

		public static Color32 DealerLabelColor = new Color32(120, 200, byte.MaxValue, byte.MaxValue);

		public static List<Dealer> AllDealers = new List<Dealer>();

		[Header("Debug")]
		public List<Customer> InitialCustomers = new List<Customer>();

		public List<ProductDefinition> InitialItems = new List<ProductDefinition>();

		[Header("Dealer References")]
		public NPCEnterableBuilding Home;

		public NPCSignal_HandleDeal DealSignal;

		public NPCEvent_StayInBuilding HomeEvent;

		public DialogueController_Supplier DialogueController;

		[Header("Dialogue stuff")]
		public DialogueContainer RecruitDialogue;

		public DialogueContainer CollectCashDialogue;

		public DialogueContainer AssignCustomersDialogue;

		[Header("Dealer Settings")]
		public string HomeName = "Home";

		public float SigningFee = 500f;

		public float Cut = 0.2f;

		public bool SellInsufficientQualityItems;

		public bool SellExcessQualityItems = true;

		[Header("Variables")]
		public string CompletedDealsVariable = string.Empty;

		[CompilerGenerated]
		[SyncVar(OnChange = "UpdateCollectCashChoice")]
		public float _003CCash_003Ek__BackingField;

		public List<Customer> AssignedCustomers = new List<Customer>();

		public List<Contract> ActiveContracts = new List<Contract>();

		protected ItemSlot[] OverflowSlots;

		private Contract currentContract;

		private DialogueController.DialogueChoice recruitChoice;

		private DialogueController.DialogueChoice collectCashChoice;

		private DialogueController.DialogueChoice assignCustomersChoice;

		[SyncVar]
		public List<string> acceptedContractGUIDs = new List<string>();

		private int itemCountOnTradeStart;

		public SyncVar<float> syncVar____003CCash_003Ek__BackingField;

		public SyncVar<List<string>> syncVar___acceptedContractGUIDs;

		private bool NetworkInitialize___EarlyScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted;

		public bool IsRecruited { get; private set; }

		public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();


		public float Cash
		{
			get
			{
				return _003CCash_003Ek__BackingField;
			}
			private set
			{
                _003CCash_003Ek__BackingField = value;

                syncVar____003CCash_003Ek__BackingField.SetValue(value, true);
            }
		}

		public NPCPoI potentialDealerPoI { get; protected set; }

		public NPCPoI dealerPoI { get; protected set; }

		public float SyncAccessor__003CCash_003Ek__BackingField
		{
			get
			{
				return Cash;
			}
			set
			{
				if (value != 0 || !base.IsServerInitialized)
				{
					Cash = value;
				}
				if (Application.isPlaying)
				{
					syncVar____003CCash_003Ek__BackingField.SetValue(value, value != 0);
				}
			}
		}

		public List<string> SyncAccessor_acceptedContractGUIDs
		{
			get
			{
				return acceptedContractGUIDs;
			}
			set
			{
				if (value != null || !base.IsServerInitialized)
				{
					acceptedContractGUIDs = value;
				}
				if (Application.isPlaying)
				{
					syncVar___acceptedContractGUIDs.SetValue(value, value != null);
				}
			}
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EEconomy_002EDealer_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			HomeEvent.Building = Home;
		}

		private void OnDestroy()
		{
			AllDealers.Remove(this);
		}

		protected override void Start()
		{
			base.Start();
			if (Application.isEditor)
			{
				foreach (Customer initialCustomer in InitialCustomers)
				{
					SendAddCustomer(initialCustomer.NPC.ID);
				}
				foreach (ProductDefinition initialItem in InitialItems)
				{
					base.Inventory.InsertItem(initialItem.GetDefaultInstance(10));
				}
			}
			for (int i = 0; i < base.Inventory.ItemSlots.Count; i++)
			{
				base.Inventory.ItemSlots[i].AddFilter(new ItemFilter_PackagedProduct());
			}
			SetUpDialogue();
			SetupPoI();
			NPCRelationData relationData = RelationData;
			relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(OnDealerUnlocked));
		}

		public override void OnSpawnServer(NetworkConnection connection)
		{
			base.OnSpawnServer(connection);
			if (connection.IsLocalClient)
			{
				return;
			}
			if (IsRecruited)
			{
				SetIsRecruited(connection);
			}
			foreach (Customer assignedCustomer in AssignedCustomers)
			{
				AddCustomer(connection, assignedCustomer.NPC.ID);
			}
		}

		private void SetupPoI()
		{
			if (dealerPoI == null)
			{
				dealerPoI = UnityEngine.Object.Instantiate(NetworkSingleton<NPCManager>.Instance.NPCPoIPrefab, base.transform);
				dealerPoI.SetMainText(base.fullName + "\n(Dealer)");
				dealerPoI.SetNPC(this);
				dealerPoI.transform.localPosition = Vector3.zero;
				dealerPoI.enabled = IsRecruited;
			}
			if (potentialDealerPoI == null)
			{
				potentialDealerPoI = UnityEngine.Object.Instantiate(NetworkSingleton<NPCManager>.Instance.PotentialDealerPoIPrefab, base.transform);
				potentialDealerPoI.SetMainText("Potential Dealer\n" + base.fullName);
				potentialDealerPoI.SetNPC(this);
				float y = (float)(FirstName[0] % 36) * 10f;
				float num = Mathf.Clamp((float)FirstName.Length * 1.5f, 1f, 10f);
				Vector3 forward = base.transform.forward;
				forward = Quaternion.Euler(0f, y, 0f) * forward;
				potentialDealerPoI.transform.localPosition = forward * num;
			}
			UpdatePotentialDealerPoI();
		}

		private void SetUpDialogue()
		{
			recruitChoice = new DialogueController.DialogueChoice();
			recruitChoice.ChoiceText = "Do you want to work for me as a distributor?";
			recruitChoice.Enabled = !IsRecruited;
			recruitChoice.Conversation = RecruitDialogue;
			recruitChoice.onChoosen.AddListener(RecruitmentRequested);
			recruitChoice.isValidCheck = CanOfferRecruitment;
			DialogueController.AddDialogueChoice(recruitChoice);
			DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
			dialogueChoice.ChoiceText = "Nevermind";
			dialogueChoice.Enabled = true;
			DialogueController.AddDialogueChoice(dialogueChoice);
		}

		protected override void MinPass()
		{
			base.MinPass();
			UpdatePotentialDealerPoI();
			if (InstanceFinder.IsServer && !Singleton<LoadManager>.Instance.IsLoading)
			{
				if (currentContract != null)
				{
					UpdateCurrentDeal();
				}
				else
				{
					CheckAttendStart();
				}
				HomeEvent.gameObject.SetActive(currentContract == null);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void InitialRecruitment()
		{
			RpcWriter___Server_InitialRecruitment_2166136261();
			RpcLogic___InitialRecruitment_2166136261();
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		public virtual void SetIsRecruited(NetworkConnection conn)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_SetIsRecruited_328543758(conn);
				RpcLogic___SetIsRecruited_328543758(conn);
			}
			else
			{
				RpcWriter___Target_SetIsRecruited_328543758(conn);
			}
		}

		protected virtual void OnDealerUnlocked(NPCRelationData.EUnlockType unlockType, bool b)
		{
			UpdatePotentialDealerPoI();
			NetworkSingleton<MoneyManager>.Instance.CashSound.Play();
		}

		protected virtual void UpdatePotentialDealerPoI()
		{
			potentialDealerPoI.enabled = RelationData.IsMutuallyKnown() && !RelationData.Unlocked;
		}

		private void TradeItems()
		{
			dialogueHandler.SkipNextDialogueBehaviourEnd();
			itemCountOnTradeStart = base.Inventory.GetTotalItemCount();
			Singleton<StorageMenu>.Instance.Open(base.Inventory, base.fullName + "'s Inventory", "Place <color=#4CB0FF>packaged product</color> here and the dealer will sell it to assigned customers");
			Singleton<StorageMenu>.Instance.onClosed.AddListener(TradeItemsDone);
		}

		private void TradeItemsDone()
		{
			Singleton<StorageMenu>.Instance.onClosed.RemoveListener(TradeItemsDone);
			behaviour.GenericDialogueBehaviour.SendDisable();
			if (base.Inventory.GetTotalItemCount() > itemCountOnTradeStart)
			{
				dialogueHandler.WorldspaceRend.ShowText("Thanks boss", 2.5f);
				PlayVO(EVOLineType.Thanks);
			}
			TryMoveOverflowItems();
		}

		private bool CanCollectCash(out string reason)
		{
			reason = string.Empty;
			if (SyncAccessor__003CCash_003Ek__BackingField <= 0f)
			{
				return false;
			}
			return true;
		}

		private void UpdateCollectCashChoice(float oldCash, float newCash, bool asServer)
		{
			if (collectCashChoice != null)
			{
				collectCashChoice.ChoiceText = "I need to collect the earnings <color=#54E717>(" + MoneyManager.FormatAmount(SyncAccessor__003CCash_003Ek__BackingField) + ")</color>";
			}
		}

		private void CollectCash()
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(SyncAccessor__003CCash_003Ek__BackingField, visualizeChange: true, playCashSound: true);
			SetCash(0f);
		}

		private void UpdateCurrentDeal()
		{
			if (currentContract.QuestState != EQuestState.Active)
			{
				currentContract.SetDealer(null);
				currentContract = null;
				DealSignal.gameObject.SetActive(value: false);
			}
		}

		private bool CanOfferRecruitment(out string reason)
		{
			reason = string.Empty;
			if (IsRecruited)
			{
				return false;
			}
			if (!RelationData.IsMutuallyKnown())
			{
				reason = "Unlock one of " + FirstName + "'s connections";
				return false;
			}
			return true;
		}

		private void CheckAttendStart()
		{
			Contract contract = ActiveContracts.FirstOrDefault();
			if (!(contract == null))
			{
				int time = ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(contract.DeliveryWindow.WindowStartTime, 30);
				int value = Mathf.CeilToInt(Vector3.Distance(Avatar.CenterPoint, contract.DeliveryLocation.CustomerStandPoint.position) / base.Movement.WalkSpeed * 1.5f);
				value = Mathf.Clamp(value, 15, 360);
				int min = ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(time, -value);
				int minsUntilExpiry = contract.GetMinsUntilExpiry();
				if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.IsCurrentTimeWithinRange(min, contract.DeliveryWindow.WindowEndTime) || minsUntilExpiry <= 240)
				{
					Debug.Log("Dealer start attend deal: " + contract.Title);
					currentContract = contract;
					DealSignal.SetStartTime(NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime);
					DealSignal.AssignContract(contract);
					DealSignal.gameObject.SetActive(value: true);
				}
			}
		}

		public virtual bool ShouldAcceptContract(ContractInfo contractInfo, Customer customer)
		{
			foreach (ProductList.Entry entry in contractInfo.Products.entries)
			{
				string productID = entry.ProductID;
				EQuality correspondingQuality = customer.CustomerData.Standards.GetCorrespondingQuality();
				EQuality correspondingQuality2 = customer.CustomerData.Standards.GetCorrespondingQuality();
				_ = SellInsufficientQualityItems;
				correspondingQuality = EQuality.Trash;
				_ = SellExcessQualityItems;
				correspondingQuality2 = EQuality.Heavenly;
				int productCount = GetProductCount(productID, correspondingQuality, correspondingQuality2);
				if (entry.Quantity > productCount)
				{
					Console.Log("Dealer " + base.fullName + " does not have enough " + productID + " for " + customer.NPC.fullName);
					return false;
				}
			}
			return true;
		}

		public virtual void ContractedOffered(ContractInfo contractInfo, Customer customer)
		{
			Console.Log("Contracted offered by " + customer.NPC.fullName);
			if (!ShouldAcceptContract(contractInfo, customer))
			{
				Console.Log("Contract accepted by dealer " + base.fullName);
				return;
			}
			EDealWindow dealWindow = GetDealWindow();
			Console.Log("Contract accepted by dealer " + base.fullName + " in window " + dealWindow);
			SyncAccessor_acceptedContractGUIDs.Add(customer.ContractAccepted(dealWindow, trackContract: false));
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendAddCustomer(string npcID)
		{
			RpcWriter___Server_SendAddCustomer_3615296227(npcID);
			RpcLogic___SendAddCustomer_3615296227(npcID);
		}

		[ObserversRpc(RunLocally = true)]
		[TargetRpc]
		private void AddCustomer(NetworkConnection conn, string npcID)
		{
			if ((object)conn == null)
			{
				RpcWriter___Observers_AddCustomer_2971853958(conn, npcID);
				RpcLogic___AddCustomer_2971853958(conn, npcID);
			}
			else
			{
				RpcWriter___Target_AddCustomer_2971853958(conn, npcID);
			}
		}

		protected virtual void AddCustomer(Customer customer)
		{
			if (!AssignedCustomers.Contains(customer))
			{
				AssignedCustomers.Add(customer);
				customer.AssignDealer(this);
				customer.onContractAssigned.AddListener(CustomerContractStarted);
			}
		}

		[ServerRpc(RequireOwnership = false, RunLocally = true)]
		public void SendRemoveCustomer(string npcID)
		{
			RpcWriter___Server_SendRemoveCustomer_3615296227(npcID);
			RpcLogic___SendRemoveCustomer_3615296227(npcID);
		}

		[ObserversRpc(RunLocally = true)]
		private void RemoveCustomer(string npcID)
		{
			RpcWriter___Observers_RemoveCustomer_3615296227(npcID);
			RpcLogic___RemoveCustomer_3615296227(npcID);
		}

		public virtual void RemoveCustomer(Customer customer)
		{
			if (AssignedCustomers.Contains(customer))
			{
				AssignedCustomers.Remove(customer);
				customer.AssignDealer(null);
				customer.onContractAssigned.RemoveListener(CustomerContractStarted);
			}
		}

		public void ChangeCash(float change)
		{
			SetCash(SyncAccessor__003CCash_003Ek__BackingField + change);
		}

		[ServerRpc(RequireOwnership = false)]
		public void SetCash(float cash)
		{
			RpcWriter___Server_SetCash_431000436(cash);
		}

		[ServerRpc(RequireOwnership = false)]
		public virtual void CompletedDeal()
		{
			RpcWriter___Server_CompletedDeal_2166136261();
		}

		[ServerRpc(RequireOwnership = false)]
		public void SubmitPayment(float payment)
		{
			RpcWriter___Server_SubmitPayment_431000436(payment);
		}

		public List<ProductDefinition> GetOrderableProducts()
		{
			List<ProductDefinition> list = new List<ProductDefinition>();
			foreach (ItemSlot allSlot in GetAllSlots())
			{
				if (allSlot.ItemInstance != null && allSlot.ItemInstance is ProductItemInstance)
				{
					ProductItemInstance product = allSlot.ItemInstance as ProductItemInstance;
					if (list.Find((ProductDefinition x) => x.ID == product.ID) == null)
					{
						list.Add(product.Definition as ProductDefinition);
					}
				}
			}
			return list;
		}

		public int GetProductCount(string productID, EQuality minQuality, EQuality maxQuality)
		{
			int num = 0;
			foreach (ItemSlot allSlot in GetAllSlots())
			{
				if (allSlot.ItemInstance != null && allSlot.ItemInstance is ProductItemInstance)
				{
					ProductItemInstance productItemInstance = allSlot.ItemInstance as ProductItemInstance;
					if (productItemInstance.ID == productID && productItemInstance.Quality >= minQuality && productItemInstance.Quality <= maxQuality)
					{
						num += productItemInstance.Quantity * productItemInstance.Amount;
					}
				}
			}
			return num;
		}

		private EDealWindow GetDealWindow()
		{
			EDealWindow window = DealWindowInfo.GetWindow(NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime);
			int num = (int)window;
			int num2 = ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(DealWindowInfo.GetWindowInfo(window).EndTime) - ScheduleOne.GameTime.TimeManager.GetMinSumFrom24HourTime(NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime);
			List<EDealWindow> list = new List<EDealWindow>();
			if (num2 > 120)
			{
				list.Add(window);
			}
			for (int i = 1; i < 4; i++)
			{
				int item = (num + i) % 4;
				list.Add((EDealWindow)item);
			}
			int num3 = 3;
			while (true)
			{
				foreach (EDealWindow item2 in list)
				{
					if (GetContractCountInWindow(item2) <= num3)
					{
						return item2;
					}
				}
				num3++;
			}
		}

		private int GetContractCountInWindow(EDealWindow window)
		{
			int num = 0;
			foreach (Contract activeContract in ActiveContracts)
			{
				if (DealWindowInfo.GetWindow(ScheduleOne.GameTime.TimeManager.AddMinutesTo24HourTime(activeContract.DeliveryWindow.WindowStartTime, 1)) == window)
				{
					num++;
				}
			}
			return num;
		}

		private void CustomerContractStarted(Contract contract)
		{
			if (SyncAccessor_acceptedContractGUIDs.Contains(contract.GUID.ToString()))
			{
				ActiveContracts.Add(contract);
				contract.SetDealer(this);
				contract.onQuestEnd.AddListener(delegate
				{
					CustomerContractEnded(contract);
				});
				contract.ShouldSendExpiredNotification = false;
				contract.ShouldSendExpiryReminder = false;
				Invoke("SortContracts", 0.05f);
			}
		}

		private void CustomerContractEnded(Contract contract)
		{
			if (ActiveContracts.Contains(contract))
			{
				ActiveContracts.Remove(contract);
				contract.SetDealer(null);
				if (InstanceFinder.IsServer && GetTotalInventoryItemCount() == 0)
				{
					DialogueChain chain = dialogueHandler.Database.GetChain(EDialogueModule.Dealer, "inventory_depleted");
					base.MSGConversation.SendMessageChain(chain.GetMessageChain());
				}
				Invoke("SortContracts", 0.05f);
			}
		}

		private void SortContracts()
		{
			ActiveContracts = ActiveContracts.OrderBy((Contract x) => x.GetMinsUntilExpiry()).ToList();
		}

		protected virtual void RecruitmentRequested()
		{
		}

		public bool RemoveContractItems(Contract contract, EQuality targetQuality, out List<ItemInstance> items)
		{
			items = new List<ItemInstance>();
			foreach (ProductList.Entry entry in contract.ProductList.entries)
			{
				int returnedQuantity;
				List<ItemInstance> items2 = GetItems(entry.ProductID, entry.Quantity, DoesQualityMatch, out returnedQuantity);
				if (returnedQuantity < entry.Quantity)
				{
					Console.LogWarning("Could not find enough items for contract entry: " + entry.ProductID);
				}
				items.AddRange(items2);
			}
			TryMoveOverflowItems();
			return true;
			bool DoesQualityMatch(ProductItemInstance product)
			{
				EQuality eQuality = targetQuality;
				EQuality eQuality2 = targetQuality;
				if (SellInsufficientQualityItems)
				{
					eQuality = EQuality.Trash;
				}
				if (SellExcessQualityItems)
				{
					eQuality2 = EQuality.Heavenly;
				}
				if (product.Quality >= eQuality)
				{
					return product.Quality <= eQuality2;
				}
				return false;
			}
		}

		private List<ItemInstance> GetItems(string ID, int requiredQuantity, Func<ProductItemInstance, bool> qualityCheck, out int returnedQuantity)
		{
			List<ItemInstance> list = new List<ItemInstance>();
			returnedQuantity = 0;
			List<ItemSlot> allSlots = GetAllSlots();
			for (int i = 0; i < allSlots.Count; i++)
			{
				if (allSlots[i].ItemInstance == null)
				{
					allSlots.RemoveAt(i);
					i--;
				}
				else if (!(allSlots[i].ItemInstance is ProductItemInstance productItemInstance) || productItemInstance.ID != ID || productItemInstance.AppliedPackaging == null || !qualityCheck(productItemInstance))
				{
					allSlots.RemoveAt(i);
					i--;
				}
			}
			allSlots.Sort(delegate(ItemSlot x, ItemSlot y)
			{
				if (x.ItemInstance == null)
				{
					return 1;
				}
				return (y.ItemInstance == null) ? (-1) : (y.ItemInstance as ProductItemInstance).Amount.CompareTo((x.ItemInstance as ProductItemInstance).Amount);
			});
			foreach (ItemSlot item in allSlots)
			{
				int amount = (item.ItemInstance as ProductItemInstance).Amount;
				while (requiredQuantity >= amount && item.Quantity > 0)
				{
					list.Add(item.ItemInstance.GetCopy(1));
					item.ChangeQuantity(-1);
					returnedQuantity += amount;
					requiredQuantity -= amount;
				}
			}
			if (requiredQuantity > 0)
			{
				while (requiredQuantity > 0)
				{
					allSlots = GetAllSlots();
					for (int j = 0; j < allSlots.Count; j++)
					{
						if (allSlots[j].ItemInstance == null)
						{
							allSlots.RemoveAt(j);
							j--;
						}
						else if (!(allSlots[j].ItemInstance is ProductItemInstance productItemInstance2) || productItemInstance2.ID != ID || productItemInstance2.AppliedPackaging == null || !qualityCheck(productItemInstance2))
						{
							allSlots.RemoveAt(j);
							j--;
						}
					}
					if (allSlots.Count == 0)
					{
						Console.LogWarning("Dealer " + base.fullName + " has no items to fulfill contract");
						return list;
					}
					allSlots.Sort(delegate(ItemSlot x, ItemSlot y)
					{
						if (x.ItemInstance == null)
						{
							return -1;
						}
						return (y.ItemInstance == null) ? 1 : (x.ItemInstance as ProductItemInstance).Amount.CompareTo((y.ItemInstance as ProductItemInstance).Amount);
					});
					ItemSlot itemSlot = allSlots[0];
					int amount2 = (itemSlot.ItemInstance as ProductItemInstance).Amount;
					if (requiredQuantity >= amount2)
					{
						while (requiredQuantity >= amount2 && itemSlot.Quantity > 0)
						{
							Console.Log("Removing 1x " + itemSlot.ItemInstance.Name + "(" + (itemSlot.ItemInstance as ProductItemInstance).AppliedPackaging.Name + ")");
							list.Add(itemSlot.ItemInstance.GetCopy(1));
							itemSlot.ChangeQuantity(-1);
							returnedQuantity += amount2;
							requiredQuantity -= amount2;
						}
						continue;
					}
					PackagingDefinition appliedPackaging = (itemSlot.ItemInstance as ProductItemInstance).AppliedPackaging;
					ProductDefinition productDefinition = (itemSlot.ItemInstance as ProductItemInstance).Definition as ProductDefinition;
					PackagingDefinition packagingDefinition = null;
					for (int k = 0; k < productDefinition.ValidPackaging.Length; k++)
					{
						if (productDefinition.ValidPackaging[k].ID == appliedPackaging.ID && k > 0)
						{
							packagingDefinition = productDefinition.ValidPackaging[k - 1];
						}
					}
					if (packagingDefinition == null)
					{
						Console.LogWarning("Failed to find next packaging smaller than " + appliedPackaging.ID);
						break;
					}
					int quantity = packagingDefinition.Quantity;
					int overrideQuantity = appliedPackaging.Quantity / quantity;
					Console.Log("Splitting 1x " + itemSlot.ItemInstance.Name + "(" + appliedPackaging.Name + ") into " + overrideQuantity + "x " + packagingDefinition.Name);
					ProductItemInstance productItemInstance3 = itemSlot.ItemInstance.GetCopy(overrideQuantity) as ProductItemInstance;
					productItemInstance3.SetPackaging(packagingDefinition);
					itemSlot.ChangeQuantity(-1);
					AddItemToInventory(productItemInstance3);
				}
			}
			return list;
		}

		public List<ItemSlot> GetAllSlots()
		{
			List<ItemSlot> list = new List<ItemSlot>(base.Inventory.ItemSlots);
			list.AddRange(OverflowSlots);
			return list;
		}

		public void AddItemToInventory(ItemInstance item)
		{
			while (base.Inventory.CanItemFit(item) && item.Quantity > 0)
			{
				base.Inventory.InsertItem(item.GetCopy(1));
				item.ChangeQuantity(-1);
			}
			if (item.Quantity > 0 && !ItemSlot.TryInsertItemIntoSet(OverflowSlots.ToList(), item))
			{
				Console.LogWarning("Dealer " + base.fullName + " has doesn't have enough space for item " + item.ID);
			}
		}

		public void TryMoveOverflowItems()
		{
			ItemSlot[] overflowSlots = OverflowSlots;
			foreach (ItemSlot itemSlot in overflowSlots)
			{
				if (itemSlot.ItemInstance != null)
				{
					while (base.Inventory.CanItemFit(itemSlot.ItemInstance) && itemSlot.ItemInstance.Quantity > 0)
					{
						base.Inventory.InsertItem(itemSlot.ItemInstance.GetCopy(1));
						itemSlot.ItemInstance.ChangeQuantity(-1);
					}
				}
			}
		}

		public int GetTotalInventoryItemCount()
		{
			List<ItemSlot> allSlots = GetAllSlots();
			int num = 0;
			foreach (ItemSlot item in allSlots)
			{
				if (item.ItemInstance != null)
				{
					num += item.ItemInstance.Quantity;
				}
			}
			return num;
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

		public override string GetSaveString()
		{
			string[] array = new string[AssignedCustomers.Count];
			for (int i = 0; i < AssignedCustomers.Count; i++)
			{
				array[i] = AssignedCustomers[i].NPC.ID;
			}
			string[] array2 = new string[ActiveContracts.Count];
			for (int j = 0; j < ActiveContracts.Count; j++)
			{
				array2[j] = ActiveContracts[j].GUID.ToString();
			}
			return new DealerData(ID, IsRecruited, array, array2, SyncAccessor__003CCash_003Ek__BackingField, new ItemSet(OverflowSlots)).GetJson();
		}

		public override void Load(NPCData data, string containerPath)
		{
			base.Load(data, containerPath);
			if (!((ISaveable)this).TryLoadFile(containerPath, "NPC", out string contents))
			{
				return;
			}
			DealerData dealerData = null;
			try
			{
				dealerData = JsonUtility.FromJson<DealerData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogWarning("Failed to deserialize character data: " + ex.Message);
				return;
			}
			if (dealerData == null)
			{
				return;
			}
			if (dealerData.Recruited)
			{
				SetIsRecruited(null);
			}
			SetCash(dealerData.Cash);
			for (int i = 0; i < dealerData.AssignedCustomerIDs.Length; i++)
			{
				NPC nPC = NPCManager.GetNPC(dealerData.AssignedCustomerIDs[i]);
				if (nPC == null)
				{
					Console.LogWarning("Failed to find customer NPC with ID " + dealerData.AssignedCustomerIDs[i]);
					continue;
				}
				Customer component = nPC.GetComponent<Customer>();
				if (component == null)
				{
					Console.LogWarning("NPC is not a customer: " + nPC.fullName);
				}
				else
				{
					SendAddCustomer(component.NPC.ID);
				}
			}
			if (dealerData.ActiveContractGUIDs != null)
			{
				for (int j = 0; j < dealerData.ActiveContractGUIDs.Length; j++)
				{
					if (!GUIDManager.IsGUIDValid(dealerData.ActiveContractGUIDs[j]))
					{
						Console.LogWarning("Invalid contract GUID: " + dealerData.ActiveContractGUIDs[j]);
						continue;
					}
					Contract @object = GUIDManager.GetObject<Contract>(new Guid(dealerData.ActiveContractGUIDs[j]));
					if (@object != null)
					{
						SyncAccessor_acceptedContractGUIDs.Add(@object.GUID.ToString());
						CustomerContractStarted(@object);
					}
				}
			}
			for (int k = 0; k < dealerData.OverflowItems.Items.Length; k++)
			{
				ItemInstance instance = ItemDeserializer.LoadItem(dealerData.OverflowItems.Items[k]);
				if (OverflowSlots.Length > k)
				{
					OverflowSlots[k].SetStoredItem(instance);
				}
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				syncVar___acceptedContractGUIDs = new SyncVar<List<string>>(this, 2u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, acceptedContractGUIDs);
				syncVar____003CCash_003Ek__BackingField = new SyncVar<float>(this, 1u, WritePermission.ServerOnly, ReadPermission.Observers, -1f, Channel.Reliable, Cash);
				syncVar____003CCash_003Ek__BackingField.OnChange += UpdateCollectCashChoice;
				RegisterServerRpc(34u, RpcReader___Server_InitialRecruitment_2166136261);
				RegisterObserversRpc(35u, RpcReader___Observers_SetIsRecruited_328543758);
				RegisterTargetRpc(36u, RpcReader___Target_SetIsRecruited_328543758);
				RegisterServerRpc(37u, RpcReader___Server_SendAddCustomer_3615296227);
				RegisterObserversRpc(38u, RpcReader___Observers_AddCustomer_2971853958);
				RegisterTargetRpc(39u, RpcReader___Target_AddCustomer_2971853958);
				RegisterServerRpc(40u, RpcReader___Server_SendRemoveCustomer_3615296227);
				RegisterObserversRpc(41u, RpcReader___Observers_RemoveCustomer_3615296227);
				RegisterServerRpc(42u, RpcReader___Server_SetCash_431000436);
				RegisterServerRpc(43u, RpcReader___Server_CompletedDeal_2166136261);
				RegisterServerRpc(44u, RpcReader___Server_SubmitPayment_431000436);
				RegisterServerRpc(45u, RpcReader___Server_SetStoredInstance_2652194801);
				RegisterObserversRpc(46u, RpcReader___Observers_SetStoredInstance_Internal_2652194801);
				RegisterTargetRpc(47u, RpcReader___Target_SetStoredInstance_Internal_2652194801);
				RegisterServerRpc(48u, RpcReader___Server_SetItemSlotQuantity_1692629761);
				RegisterObserversRpc(49u, RpcReader___Observers_SetItemSlotQuantity_Internal_1692629761);
				RegisterServerRpc(50u, RpcReader___Server_SetSlotLocked_3170825843);
				RegisterTargetRpc(51u, RpcReader___Target_SetSlotLocked_Internal_3170825843);
				RegisterObserversRpc(52u, RpcReader___Observers_SetSlotLocked_Internal_3170825843);
				RegisterSyncVarRead(ReadSyncVar___ScheduleOne_002EEconomy_002EDealer);
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EEconomy_002EDealerAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
				syncVar___acceptedContractGUIDs.SetRegistered();
				syncVar____003CCash_003Ek__BackingField.SetRegistered();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		private void RpcWriter___Server_InitialRecruitment_2166136261()
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
				SendServerRpc(34u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___InitialRecruitment_2166136261()
		{
			SetIsRecruited(null);
		}

		private void RpcReader___Server_InitialRecruitment_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___InitialRecruitment_2166136261();
			}
		}

		private void RpcWriter___Observers_SetIsRecruited_328543758(NetworkConnection conn)
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
				SendObserversRpc(35u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		public virtual void RpcLogic___SetIsRecruited_328543758(NetworkConnection conn)
		{
			if (!IsRecruited)
			{
				IsRecruited = true;
				DialogueController.GreetingOverride greetingOverride = new DialogueController.GreetingOverride();
				greetingOverride.Greeting = "Hi boss, what do you need?";
				greetingOverride.PlayVO = true;
				greetingOverride.VOType = EVOLineType.Greeting;
				greetingOverride.ShouldShow = true;
				DialogueController.AddGreetingOverride(greetingOverride);
				DialogueController.DialogueChoice dialogueChoice = new DialogueController.DialogueChoice();
				dialogueChoice.ChoiceText = "I need to trade some items";
				dialogueChoice.Enabled = true;
				dialogueChoice.onChoosen.AddListener(TradeItems);
				DialogueController.AddDialogueChoice(dialogueChoice);
				DialogueController.SetDialogueChoiceIndex(dialogueChoice, 0);
				collectCashChoice = new DialogueController.DialogueChoice();
				UpdateCollectCashChoice(0f, 0f, asServer: false);
				collectCashChoice.Enabled = true;
				collectCashChoice.isValidCheck = CanCollectCash;
				collectCashChoice.onChoosen.AddListener(CollectCash);
				collectCashChoice.Conversation = CollectCashDialogue;
				DialogueController.AddDialogueChoice(collectCashChoice);
				DialogueController.SetDialogueChoiceIndex(collectCashChoice, 1);
				assignCustomersChoice = new DialogueController.DialogueChoice();
				assignCustomersChoice.ChoiceText = "How do I assign customers to you?";
				assignCustomersChoice.Enabled = true;
				assignCustomersChoice.Conversation = AssignCustomersDialogue;
				DialogueController.AddDialogueChoice(assignCustomersChoice);
				DialogueController.SetDialogueChoiceIndex(assignCustomersChoice, 2);
				if (dealerPoI != null)
				{
					dealerPoI.enabled = true;
				}
				if (!RelationData.Unlocked)
				{
					RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach, notify: false);
				}
				if (recruitChoice != null)
				{
					recruitChoice.Enabled = false;
				}
				if (onDealerRecruited != null)
				{
					onDealerRecruited(this);
				}
				base.HasChanged = true;
			}
		}

		private void RpcReader___Observers_SetIsRecruited_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___SetIsRecruited_328543758(null);
			}
		}

		private void RpcWriter___Target_SetIsRecruited_328543758(NetworkConnection conn)
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
				SendTargetRpc(36u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_SetIsRecruited_328543758(PooledReader PooledReader0, Channel channel)
		{
			if (base.IsClientInitialized)
			{
				RpcLogic___SetIsRecruited_328543758(base.LocalConnection);
			}
		}

		private void RpcWriter___Server_SendAddCustomer_3615296227(string npcID)
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
				SendServerRpc(37u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendAddCustomer_3615296227(string npcID)
		{
			AddCustomer(null, npcID);
		}

		private void RpcReader___Server_SendAddCustomer_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendAddCustomer_3615296227(npcID);
			}
		}

		private void RpcWriter___Observers_AddCustomer_2971853958(NetworkConnection conn, string npcID)
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
				SendObserversRpc(38u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___AddCustomer_2971853958(NetworkConnection conn, string npcID)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogWarning("Failed to find NPC with ID: " + npcID);
				return;
			}
			Customer component = nPC.GetComponent<Customer>();
			if (component == null)
			{
				Console.LogWarning("NPC " + npcID + " is not a customer");
			}
			else
			{
				AddCustomer(component);
			}
		}

		private void RpcReader___Observers_AddCustomer_2971853958(PooledReader PooledReader0, Channel channel)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___AddCustomer_2971853958(null, npcID);
			}
		}

		private void RpcWriter___Target_AddCustomer_2971853958(NetworkConnection conn, string npcID)
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
				SendTargetRpc(39u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
				writer.Store();
			}
		}

		private void RpcReader___Target_AddCustomer_2971853958(PooledReader PooledReader0, Channel channel)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized)
			{
				RpcLogic___AddCustomer_2971853958(base.LocalConnection, npcID);
			}
		}

		private void RpcWriter___Server_SendRemoveCustomer_3615296227(string npcID)
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
				SendServerRpc(40u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SendRemoveCustomer_3615296227(string npcID)
		{
			RemoveCustomer(npcID);
		}

		private void RpcReader___Server_SendRemoveCustomer_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsServerInitialized && !conn.IsLocalClient)
			{
				RpcLogic___SendRemoveCustomer_3615296227(npcID);
			}
		}

		private void RpcWriter___Observers_RemoveCustomer_3615296227(string npcID)
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
				SendObserversRpc(41u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
				writer.Store();
			}
		}

		private void RpcLogic___RemoveCustomer_3615296227(string npcID)
		{
			NPC nPC = NPCManager.GetNPC(npcID);
			if (nPC == null)
			{
				Console.LogWarning("Failed to find NPC with ID: " + npcID);
				return;
			}
			Customer component = nPC.GetComponent<Customer>();
			if (component == null)
			{
				Console.LogWarning("NPC " + npcID + " is not a customer");
			}
			else
			{
				RemoveCustomer(component);
			}
		}

		private void RpcReader___Observers_RemoveCustomer_3615296227(PooledReader PooledReader0, Channel channel)
		{
			string npcID = PooledReader0.ReadString();
			if (base.IsClientInitialized && !base.IsHost)
			{
				RpcLogic___RemoveCustomer_3615296227(npcID);
			}
		}

		private void RpcWriter___Server_SetCash_431000436(float cash)
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
				writer.WriteSingle(cash);
				SendServerRpc(42u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SetCash_431000436(float cash)
		{
			Cash = Mathf.Clamp(cash, 0f, float.MaxValue);
			base.HasChanged = true;
			UpdateCollectCashChoice(0f, 0f, asServer: false);
		}

		private void RpcReader___Server_SetCash_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float cash = PooledReader0.ReadSingle();
			if (base.IsServerInitialized)
			{
				RpcLogic___SetCash_431000436(cash);
			}
		}

		private void RpcWriter___Server_CompletedDeal_2166136261()
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
				SendServerRpc(43u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public virtual void RpcLogic___CompletedDeal_2166136261()
		{
			if (CompletedDealsVariable != string.Empty)
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(CompletedDealsVariable, (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>(CompletedDealsVariable) + 1f).ToString());
			}
		}

		private void RpcReader___Server_CompletedDeal_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			if (base.IsServerInitialized)
			{
				RpcLogic___CompletedDeal_2166136261();
			}
		}

		private void RpcWriter___Server_SubmitPayment_431000436(float payment)
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
				writer.WriteSingle(payment);
				SendServerRpc(44u, writer, channel, DataOrderType.Default);
				writer.Store();
			}
		}

		public void RpcLogic___SubmitPayment_431000436(float payment)
		{
			Console.Log("Dealer " + base.fullName + " received payment: " + payment);
			float num = SyncAccessor__003CCash_003Ek__BackingField;
			ChangeCash(payment * (1f - Cut));
			if (InstanceFinder.IsServer && SyncAccessor__003CCash_003Ek__BackingField >= 500f && num < 500f)
			{
				base.MSGConversation.SendMessage(new Message("Hey boss, just letting you know I've got " + MoneyManager.FormatAmount(SyncAccessor__003CCash_003Ek__BackingField) + " ready for you to collect.", Message.ESenderType.Other, _endOfGroup: true));
			}
		}

		private void RpcReader___Server_SubmitPayment_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			float payment = PooledReader0.ReadSingle();
			if (base.IsServerInitialized)
			{
				RpcLogic___SubmitPayment_431000436(payment);
			}
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
				SendServerRpc(45u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(46u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendTargetRpc(47u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendServerRpc(48u, writer, channel, DataOrderType.Default);
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
				SendObserversRpc(49u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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
				SendServerRpc(50u, writer, channel, DataOrderType.Default);
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
				SendTargetRpc(51u, writer, channel, DataOrderType.Default, conn, excludeServer: false);
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
				SendObserversRpc(52u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
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

		public virtual bool ReadSyncVar___ScheduleOne_002EEconomy_002EDealer(PooledReader PooledReader0, byte UInt321, bool Boolean2)
		{
			switch (UInt321)
			{
			case 2:
			{
				if (PooledReader0 == null)
				{
					syncVar___acceptedContractGUIDs.SetValue(syncVar___acceptedContractGUIDs.GetValue(calledByUser: true), true);
					return true;
				}
				List<string> value2 = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(PooledReader0);
                        syncVar___acceptedContractGUIDs.SetValue(value2, Boolean2);
				return true;
			}
			case 1:
			{
				if (PooledReader0 == null)
				{
					syncVar____003CCash_003Ek__BackingField.SetValue(syncVar____003CCash_003Ek__BackingField.GetValue(calledByUser: true), true);
					return true;
				}
				float value = PooledReader0.ReadSingle();
                        syncVar____003CCash_003Ek__BackingField.SetValue(value, Boolean2);
				return true;
			}
			default:
				return false;
			}
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EEconomy_002EDealer_Assembly_002DCSharp_002Edll()
		{
			base.Awake();
			HomeEvent.Building = Home;
			OverflowSlots = new ItemSlot[10];
			for (int i = 0; i < 10; i++)
			{
				OverflowSlots[i] = new ItemSlot();
				OverflowSlots[i].SetSlotOwner(this);
			}
			if (RelationData.Unlocked)
			{
				SetIsRecruited(null);
			}
			else
			{
				NPCRelationData relationData = RelationData;
				relationData.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData.onUnlocked, (Action<NPCRelationData.EUnlockType, bool>)delegate
				{
					SetIsRecruited(null);
				});
			}
			if (!AllDealers.Contains(this))
			{
				AllDealers.Add(this);
			}
		}
	}
}
