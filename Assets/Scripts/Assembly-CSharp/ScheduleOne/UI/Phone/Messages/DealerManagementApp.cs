using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages
{
	public class DealerManagementApp : App<DealerManagementApp>
	{
		[Header("References")]
		public Text NoDealersLabel;

		public RectTransform Content;

		public CustomerSelector CustomerSelector;

		[Header("Selector")]
		public Image SelectorImage;

		public Text SelectorTitle;

		public Button BackButton;

		public Button NextButton;

		[Header("Basic Info")]
		public Text CashLabel;

		public Text CutLabel;

		public Text HomeLabel;

		[Header("Inventory")]
		public RectTransform[] InventoryEntries;

		[Header("Customers")]
		public Text CustomerTitleLabel;

		public RectTransform[] CustomerEntries;

		public Button AssignCustomerButton;

		private List<Dealer> dealers = new List<Dealer>();

		public Dealer SelectedDealer { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			foreach (Dealer allDealer in Dealer.AllDealers)
			{
				if (allDealer.IsRecruited)
				{
					AddDealer(allDealer);
				}
			}
			Dealer.onDealerRecruited = (Action<Dealer>)Delegate.Combine(Dealer.onDealerRecruited, new Action<Dealer>(AddDealer));
			BackButton.onClick.AddListener(BackPressed);
			NextButton.onClick.AddListener(NextPressed);
			AssignCustomerButton.onClick.AddListener(AssignCustomer);
		}

		protected override void Start()
		{
			base.Start();
			CustomerSelector.onCustomerSelected.AddListener(AddCustomer);
		}

		protected override void OnDestroy()
		{
			Dealer.onDealerRecruited = (Action<Dealer>)Delegate.Remove(Dealer.onDealerRecruited, new Action<Dealer>(AddDealer));
			base.OnDestroy();
		}

		public override void SetOpen(bool open)
		{
			if (SelectedDealer != null)
			{
				SetDisplayedDealer(SelectedDealer);
			}
			else if (dealers.Count > 0)
			{
				SetDisplayedDealer(dealers[0]);
			}
			else
			{
				NoDealersLabel.gameObject.SetActive(value: true);
				Content.gameObject.SetActive(value: false);
			}
			base.SetOpen(open);
		}

		public void SetDisplayedDealer(Dealer dealer)
		{
			SelectedDealer = dealer;
			SelectorImage.sprite = dealer.MugshotSprite;
			SelectorTitle.text = dealer.fullName;
			CashLabel.text = MoneyManager.FormatAmount(dealer.Cash);
			CutLabel.text = Mathf.RoundToInt(dealer.Cut * 100f) + "%";
			HomeLabel.text = dealer.HomeName;
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			List<string> list = new List<string>();
			foreach (ItemSlot allSlot in dealer.GetAllSlots())
			{
				if (allSlot.Quantity != 0)
				{
					int num = allSlot.Quantity;
					if (allSlot.ItemInstance is ProductItemInstance)
					{
						num *= ((ProductItemInstance)allSlot.ItemInstance).Amount;
					}
					if (list.Contains(allSlot.ItemInstance.ID))
					{
						dictionary[allSlot.ItemInstance.ID] += num;
						continue;
					}
					list.Add(allSlot.ItemInstance.ID);
					dictionary.Add(allSlot.ItemInstance.ID, num);
				}
			}
			for (int i = 0; i < InventoryEntries.Length; i++)
			{
				if (list.Count > i)
				{
					ItemDefinition item = Registry.GetItem(list[i]);
					InventoryEntries[i].Find("Image").GetComponent<Image>().sprite = item.Icon;
					InventoryEntries[i].Find("Title").GetComponent<Text>().text = dictionary[list[i]] + "x " + item.Name;
					InventoryEntries[i].gameObject.SetActive(value: true);
				}
				else
				{
					InventoryEntries[i].gameObject.SetActive(value: false);
				}
			}
			CustomerTitleLabel.text = "Assigned Customers (" + dealer.AssignedCustomers.Count + "/" + 6 + ")";
			for (int j = 0; j < CustomerEntries.Length; j++)
			{
				if (dealer.AssignedCustomers.Count > j)
				{
					Customer customer = dealer.AssignedCustomers[j];
					CustomerEntries[j].Find("Mugshot").GetComponent<Image>().sprite = customer.NPC.MugshotSprite;
					CustomerEntries[j].Find("Name").GetComponent<Text>().text = customer.NPC.fullName;
					Button component = CustomerEntries[j].Find("Remove").GetComponent<Button>();
					component.onClick.RemoveAllListeners();
					component.onClick.AddListener(delegate
					{
						RemoveCustomer(customer);
					});
					CustomerEntries[j].gameObject.SetActive(value: true);
				}
				else
				{
					CustomerEntries[j].gameObject.SetActive(value: false);
				}
			}
			BackButton.interactable = dealers.IndexOf(dealer) > 0;
			NextButton.interactable = dealers.IndexOf(dealer) < dealers.Count - 1;
			AssignCustomerButton.gameObject.SetActive(dealer.AssignedCustomers.Count < 6);
			NoDealersLabel.gameObject.SetActive(value: false);
			Content.gameObject.SetActive(value: true);
		}

		private void AddDealer(Dealer dealer)
		{
			if (!dealers.Contains(dealer))
			{
				dealers.Add(dealer);
				dealers = dealers.OrderBy((Dealer d) => d.FirstName).ToList();
			}
		}

		private void AddCustomer(Customer customer)
		{
			SelectedDealer.SendAddCustomer(customer.NPC.ID);
			if (customer.OfferedContractInfo != null)
			{
				Console.Log("Expiring...");
				customer.ExpireOffer();
			}
			SetDisplayedDealer(SelectedDealer);
		}

		private void RemoveCustomer(Customer customer)
		{
			SelectedDealer.SendRemoveCustomer(customer.NPC.ID);
			SetDisplayedDealer(SelectedDealer);
		}

		private void BackPressed()
		{
			int num = dealers.IndexOf(SelectedDealer);
			if (num > 0)
			{
				SetDisplayedDealer(dealers[num - 1]);
			}
		}

		private void NextPressed()
		{
			int num = dealers.IndexOf(SelectedDealer);
			if (num < dealers.Count - 1)
			{
				SetDisplayedDealer(dealers[num + 1]);
			}
		}

		public void AssignCustomer()
		{
			CustomerSelector.Open();
		}
	}
}
