using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Product;
using ScheduleOne.UI;
using ScheduleOne.UI.Handover;
using ScheduleOne.UI.Phone;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.Quests
{
	public class Contract : Quest
	{
		public class BonusPayment
		{
			public string Title;

			public float Amount;

			public BonusPayment(string title, float amount)
			{
				Title = title;
				Amount = amount;
			}
		}

		public const int DefaultExpiryTime = 2880;

		public static List<Contract> Contracts = new List<Contract>();

		[Header("Contract Settings")]
		public ProductList ProductList;

		public DeliveryLocation DeliveryLocation;

		public QuestWindowConfig DeliveryWindow;

		private bool completedContractsIncremented;

		public NetworkObject Customer { get; protected set; }

		public Dealer Dealer { get; protected set; }

		public float Payment { get; protected set; }

		public int PickupScheduleIndex { get; protected set; }

		public GameDateTime AcceptTime { get; protected set; }

		protected override void Start()
		{
			autoInitialize = false;
			base.Start();
		}

		public virtual void InitializeContract(string title, string description, QuestEntryData[] entries, string guid, NetworkObject customer, float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, int pickupScheduleIndex, GameDateTime acceptTime)
		{
			SilentlyInitializeContract(base.title, Description, entries, guid, customer, payment, products, deliveryLocationGUID, deliveryWindow, pickupScheduleIndex, acceptTime);
			Contracts.Add(this);
			base.InitializeQuest(title, description, entries, guid);
			Customer.GetComponent<Customer>().AssignContract(this);
		}

		public virtual void SilentlyInitializeContract(string title, string description, QuestEntryData[] entries, string guid, NetworkObject customer, float payment, ProductList products, string deliveryLocationGUID, QuestWindowConfig deliveryWindow, int pickupScheduleIndex, GameDateTime acceptTime)
		{
			Customer = customer;
			Payment = payment;
			ProductList = products;
			if (GUIDManager.IsGUIDValid(deliveryLocationGUID))
			{
				DeliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(deliveryLocationGUID));
			}
			DeliveryWindow = deliveryWindow;
			PickupScheduleIndex = pickupScheduleIndex;
			AcceptTime = acceptTime;
		}

		protected override void MinPass()
		{
			base.MinPass();
			UpdateTiming();
		}

		private void OnDestroy()
		{
			Contracts.Remove(this);
		}

		private void UpdateTiming()
		{
			if (!base.Expires || ExpiryVisibility == EExpiryVisibility.Never)
			{
				return;
			}
			int minsUntilExpiry = GetMinsUntilExpiry();
			int num = Mathf.FloorToInt((float)minsUntilExpiry / 60f);
			int num2 = minsUntilExpiry - 360;
			int num3 = Mathf.FloorToInt((float)num2 / 60f);
			if (num2 > 0)
			{
				if (num3 > 0)
				{
					SetSubtitle("<color=#c0c0c0ff> (Begins in " + num3 + " hrs)</color>");
				}
				else
				{
					SetSubtitle("<color=#c0c0c0ff> (Begins in " + num2 + " min)</color>");
				}
			}
			else if (minsUntilExpiry < 120)
			{
				if (num > 0)
				{
					SetSubtitle("<color=#" + ColorUtility.ToHtmlStringRGBA(criticalTimeBackground.color) + "> (Expires in " + num + " hrs)</color>");
				}
				else
				{
					SetSubtitle("<color=#" + ColorUtility.ToHtmlStringRGBA(criticalTimeBackground.color) + "> (Expires in " + minsUntilExpiry + " min)</color>");
				}
			}
			else if (num > 0)
			{
				SetSubtitle("<color=green> (Expires in " + num + " hrs)</color>");
			}
			else
			{
				SetSubtitle("<color=green> (Expires in " + num + " min)</color>");
			}
		}

		public override void End()
		{
			base.End();
			Contracts.Remove(this);
		}

		public override void Complete(bool network = true)
		{
			if (InstanceFinder.IsServer && !completedContractsIncremented)
			{
				completedContractsIncremented = true;
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Completed_Contracts_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Completed_Contracts_Count") + 1f).ToString());
			}
			_ = (Registry.GetItem(ProductList.entries[0].ProductID) as ProductDefinition).LawIntensityChange;
			Mathf.Lerp(0.5f, 2f, (float)ProductList.entries[0].Quantity / 25f);
			base.Complete(network);
		}

		public void SetDealer(Dealer dealer)
		{
			Dealer = dealer;
			if (journalEntry != null)
			{
				journalEntry.gameObject.SetActive(ShouldShowJournalEntry());
			}
		}

		public virtual void SubmitPayment(float bonusTotal)
		{
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(Payment + bonusTotal);
		}

		protected override void SendExpiryReminder()
		{
			Singleton<NotificationsManager>.Instance.SendNotification("<color=#FFB43C>Deal Expiring Soon</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
		}

		protected override void SendExpiredNotification()
		{
			Singleton<NotificationsManager>.Instance.SendNotification("<color=#FF6455>Deal Expired</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
		}

		protected override bool ShouldShowJournalEntry()
		{
			if (Dealer != null)
			{
				return false;
			}
			return base.ShouldShowJournalEntry();
		}

		protected override bool CanExpire()
		{
			if (Singleton<HandoverScreen>.Instance.CurrentContract == this)
			{
				return false;
			}
			if (Customer.GetComponent<NPC>().dialogueHandler.IsPlaying)
			{
				return false;
			}
			return base.CanExpire();
		}

		public bool DoesProductListMatchSpecified(List<ItemInstance> items, bool enforceQuality)
		{
			foreach (ProductList.Entry entry in ProductList.entries)
			{
				List<ItemInstance> list = items.Where((ItemInstance x) => x.ID == entry.ProductID).ToList();
				List<ProductItemInstance> list2 = new List<ProductItemInstance>();
				for (int i = 0; i < list.Count; i++)
				{
					list2.Add(list[i] as ProductItemInstance);
				}
				List<ProductItemInstance> list3 = new List<ProductItemInstance>();
				for (int j = 0; j < items.Count; j++)
				{
					ProductItemInstance productItemInstance = items[j] as ProductItemInstance;
					if (productItemInstance.Quality >= entry.Quality)
					{
						list3.Add(productItemInstance);
					}
				}
				int num = 0;
				for (int k = 0; k < list2.Count; k++)
				{
					num += list2[k].Quantity * list2[k].Amount;
				}
				int num2 = 0;
				for (int l = 0; l < list3.Count; l++)
				{
					num2 += list3[l].Quantity * list2[l].Amount;
				}
				if (enforceQuality)
				{
					if (num2 < entry.Quantity)
					{
						return false;
					}
				}
				else if (num < entry.Quantity)
				{
					return false;
				}
			}
			return true;
		}

		public float GetProductListMatch(List<ItemInstance> items, out int matchedProductCount)
		{
			float num = 0f;
			int totalQuantity = ProductList.GetTotalQuantity();
			matchedProductCount = 0;
			List<ItemInstance> list = new List<ItemInstance>();
			for (int i = 0; i < items.Count; i++)
			{
				list.Add(items[i].GetCopy());
			}
			foreach (ProductList.Entry entry in ProductList.entries)
			{
				int num2 = entry.Quantity;
				ProductDefinition definition = Registry.GetItem(entry.ProductID) as ProductDefinition;
				Dictionary<ProductItemInstance, float> matchRatings = new Dictionary<ProductItemInstance, float>();
				foreach (ItemInstance item in list)
				{
					if (item.Quantity != 0 && item is ProductItemInstance productItemInstance)
					{
						matchRatings.Add(productItemInstance, productItemInstance.GetSimilarity(definition, entry.Quality));
					}
				}
				List<ProductItemInstance> list2 = matchRatings.Keys.ToList();
				list2.Sort((ProductItemInstance x, ProductItemInstance y) => matchRatings[y].CompareTo(matchRatings[x]));
				for (int j = 0; j < list2.Count; j++)
				{
					int amount = list2[j].Amount;
					_ = list2[j].Quantity;
					int num3 = Mathf.Min(Mathf.CeilToInt((float)num2 / (float)amount), list2[j].Quantity);
					num2 -= num3 * amount;
					num += matchRatings[list2[j]] * (float)num3 * (float)amount;
					if (matchRatings[list2[j]] > 0f)
					{
						matchedProductCount += num3 * amount;
					}
					list2[j].ChangeQuantity(-num3);
				}
			}
			return num / (float)totalQuantity;
		}

		public override string GetSaveString()
		{
			List<QuestEntryData> list = new List<QuestEntryData>();
			for (int i = 0; i < Entries.Count; i++)
			{
				list.Add(Entries[i].GetSaveData());
			}
			return new ContractData(base.GUID.ToString(), base.QuestState, base.IsTracked, title, Description, base.Expires, new GameDateTimeData(base.Expiry), list.ToArray(), Customer.GetComponent<NPC>().GUID.ToString(), Payment, ProductList, DeliveryLocation.GUID.ToString(), DeliveryWindow, PickupScheduleIndex, new GameDateTimeData(AcceptTime)).GetJson();
		}

		public bool ShouldSave()
		{
			if (base.gameObject == null)
			{
				return false;
			}
			return base.QuestState == EQuestState.Active;
		}
	}
}
