using System;
using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class ListingUI : MonoBehaviour
	{
		public static Color32 PriceLabelColor_Normal = new Color32(90, 185, 90, byte.MaxValue);

		public static Color32 PriceLabelColor_NoStock = new Color32(165, 70, 60, byte.MaxValue);

		[Header("References")]
		public Image Icon;

		public TextMeshProUGUI NameLabel;

		public TextMeshProUGUI PriceLabel;

		public GameObject LockedContainer;

		public Button BuyButton;

		public Button DropdownButton;

		public EventTrigger Trigger;

		public RectTransform DetailPanelAnchor;

		public RectTransform DropdownAnchor;

		public RectTransform TopDropdownAnchor;

		public Action hoverStart;

		public Action hoverEnd;

		public Action onClicked;

		public Action onDropdownClicked;

		public ShopListing Listing { get; protected set; }

		public virtual void Initialize(ShopListing listing)
		{
			Listing = listing;
			Icon.sprite = listing.Item.Icon;
			NameLabel.text = listing.Item.Name;
			UpdatePrice();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				HoverStart();
			});
			Trigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback.AddListener(delegate
			{
				HoverEnd();
			});
			Trigger.triggers.Add(entry2);
			listing.onQuantityChanged = (Action)Delegate.Combine(listing.onQuantityChanged, new Action(QuantityChanged));
			BuyButton.onClick.AddListener(Clicked);
			DropdownButton.onClick.AddListener(DropdownClicked);
			UpdateLockStatus();
		}

		public virtual RectTransform GetIconCopy(RectTransform parent)
		{
			return UnityEngine.Object.Instantiate(Icon.gameObject, parent).GetComponent<RectTransform>();
		}

		private void Clicked()
		{
			if (onClicked != null)
			{
				onClicked();
			}
		}

		private void DropdownClicked()
		{
			if (onDropdownClicked != null)
			{
				onDropdownClicked();
			}
		}

		private void HoverStart()
		{
			if (hoverStart != null)
			{
				hoverStart();
			}
		}

		private void HoverEnd()
		{
			if (hoverEnd != null)
			{
				hoverEnd();
			}
		}

		private void QuantityChanged()
		{
			UpdatePrice();
		}

		private void UpdatePrice()
		{
			if (Listing.IsInStock)
			{
				PriceLabel.text = MoneyManager.FormatAmount(Listing.Price);
				PriceLabel.color = PriceLabelColor_Normal;
			}
			else
			{
				PriceLabel.text = "Out of stock";
				PriceLabel.color = PriceLabelColor_NoStock;
			}
		}

		public void UpdateLockStatus()
		{
			LockedContainer.gameObject.SetActive(!Listing.Item.IsPurchasable);
		}
	}
}
