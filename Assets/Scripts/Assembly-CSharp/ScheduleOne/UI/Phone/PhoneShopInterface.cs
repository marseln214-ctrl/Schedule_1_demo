using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone
{
	public class PhoneShopInterface : MonoBehaviour
	{
		[Serializable]
		public class Listing
		{
			public ItemDefinition Item;

			public float Price;

			public Listing(ItemDefinition item, float price)
			{
				Item = item;
				Price = price;
			}
		}

		[Serializable]
		public class CartEntry
		{
			public Listing Listing;

			public int Quantity;

			public CartEntry(Listing listing, int quantity)
			{
				Listing = listing;
				Quantity = quantity;
			}
		}

		public RectTransform EntryPrefab;

		public Color ValidAmountColor;

		public Color InvalidAmountColor;

		[Header("References")]
		public GameObject Container;

		public Text TitleLabel;

		public Text SubtitleLabel;

		public RectTransform EntryContainer;

		public Text OrderTotalLabel;

		public Text OrderLimitLabel;

		public Text DebtLabel;

		public Button ConfirmButton;

		public GameObject ItemLimitContainer;

		public Text ItemLimitLabel;

		private List<RectTransform> _entries = new List<RectTransform>();

		private List<Listing> _items = new List<Listing>();

		private List<CartEntry> _cart = new List<CartEntry>();

		private float orderLimit;

		private Action<List<CartEntry>, float> orderConfirmedCallback;

		private MSGConversation conversation;

		public bool IsOpen { get; private set; }

		private void Start()
		{
			GameInput.RegisterExitListener(Exit, 4);
			ConfirmButton.onClick.AddListener(ConfirmOrderPressed);
			ItemLimitContainer.gameObject.SetActive(value: false);
			Close();
		}

		public void Open(string title, string subtitle, MSGConversation _conversation, List<Listing> listings, float _orderLimit, float debt, Action<List<CartEntry>, float> _orderConfirmedCallback)
		{
			IsOpen = true;
			TitleLabel.text = title;
			SubtitleLabel.text = subtitle;
			OrderLimitLabel.text = MoneyManager.FormatAmount(_orderLimit);
			DebtLabel.text = MoneyManager.FormatAmount(debt);
			orderLimit = _orderLimit;
			conversation = _conversation;
			MSGConversation mSGConversation = conversation;
			mSGConversation.onMessageRendered = (Action)Delegate.Combine(mSGConversation.onMessageRendered, new Action(Close));
			orderConfirmedCallback = _orderConfirmedCallback;
			_items.Clear();
			_items.AddRange(listings);
			foreach (Listing entry in listings)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(EntryPrefab, EntryContainer);
				rectTransform.Find("Icon").GetComponent<Image>().sprite = entry.Item.Icon;
				rectTransform.Find("Name").GetComponent<Text>().text = entry.Item.Name;
				rectTransform.Find("Price").GetComponent<Text>().text = MoneyManager.FormatAmount(entry.Price);
				rectTransform.Find("Quantity").GetComponent<Text>().text = "0";
				StorableItemDefinition storableItemDefinition = entry.Item as StorableItemDefinition;
				if (!storableItemDefinition.RequiresLevelToPurchase || NetworkSingleton<LevelManager>.Instance.GetFullRank() >= storableItemDefinition.RequiredRank)
				{
					rectTransform.Find("Quantity/Remove").GetComponent<Button>().onClick.AddListener(delegate
					{
						ChangeListingQuantity(entry, -1);
					});
					rectTransform.Find("Quantity/Add").GetComponent<Button>().onClick.AddListener(delegate
					{
						ChangeListingQuantity(entry, 1);
					});
					rectTransform.Find("Locked").gameObject.SetActive(value: false);
				}
				else
				{
					rectTransform.Find("Locked/Title").GetComponent<Text>().text = "Unlocks at " + storableItemDefinition.RequiredRank.ToString();
					rectTransform.Find("Locked").gameObject.SetActive(value: true);
				}
				_entries.Add(rectTransform);
			}
			CartChanged();
			Container.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			IsOpen = false;
			_items.Clear();
			_cart.Clear();
			if (conversation != null)
			{
				MSGConversation mSGConversation = conversation;
				mSGConversation.onMessageRendered = (Action)Delegate.Remove(mSGConversation.onMessageRendered, new Action(Close));
			}
			foreach (RectTransform entry in _entries)
			{
				UnityEngine.Object.Destroy(entry.gameObject);
			}
			_entries.Clear();
			Container.gameObject.SetActive(value: false);
		}

		public void Exit(ExitAction action)
		{
			if (!action.used && IsOpen)
			{
				action.used = true;
				Close();
			}
		}

		private void ChangeListingQuantity(Listing listing, int change)
		{
			CartEntry cartEntry = _cart.Find((CartEntry e) => e.Listing.Item.ID == listing.Item.ID);
			if (cartEntry == null)
			{
				cartEntry = new CartEntry(listing, 0);
				_cart.Add(cartEntry);
			}
			cartEntry.Quantity = Mathf.Clamp(cartEntry.Quantity + change, 0, 99);
			_entries[_items.IndexOf(listing)].Find("Quantity").GetComponent<Text>().text = cartEntry.Quantity.ToString();
			CartChanged();
		}

		private void CartChanged()
		{
			UpdateOrderTotal();
			ConfirmButton.interactable = CanConfirmOrder();
		}

		private void ConfirmOrderPressed()
		{
			orderConfirmedCallback(_cart, GetOrderTotal(out var _));
			Close();
		}

		private bool CanConfirmOrder()
		{
			int itemCount;
			float orderTotal = GetOrderTotal(out itemCount);
			if (orderTotal > 0f)
			{
				return orderTotal <= orderLimit;
			}
			return false;
		}

		private void UpdateOrderTotal()
		{
			int itemCount;
			float orderTotal = GetOrderTotal(out itemCount);
			OrderTotalLabel.text = MoneyManager.FormatAmount(orderTotal);
			OrderTotalLabel.color = ((orderTotal <= orderLimit) ? ValidAmountColor : InvalidAmountColor);
			ItemLimitLabel.text = itemCount + "/" + 20;
			ItemLimitLabel.color = ((itemCount <= 20) ? Color.black : InvalidAmountColor);
		}

		private float GetOrderTotal(out int itemCount)
		{
			float num = 0f;
			itemCount = 0;
			foreach (CartEntry item in _cart)
			{
				num += item.Listing.Price * (float)item.Quantity;
				itemCount += item.Quantity;
			}
			return num;
		}
	}
}
