using ScheduleOne.Money;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Shop
{
	public class CartEntry : MonoBehaviour
	{
		[Header("References")]
		public TextMeshProUGUI TitleLabel;

		public TextMeshProUGUI PriceLabel;

		public Button RemoveButton;

		public int Quantity { get; protected set; }

		public ShopListing Listing { get; protected set; }

		public void Initialize(Cart cart, ShopListing listing, int quantity)
		{
			Listing = listing;
			Quantity = quantity;
			RemoveButton.onClick.AddListener(delegate
			{
				cart.RemoveItem(listing);
			});
			UpdateTitle();
			UpdatePrice();
		}

		public void SetQuantity(int quantity)
		{
			Quantity = quantity;
			UpdateTitle();
			UpdatePrice();
		}

		protected virtual void UpdateTitle()
		{
			TitleLabel.text = Quantity + "x " + Listing.Item.Name;
		}

		private void UpdatePrice()
		{
			PriceLabel.text = MoneyManager.FormatAmount((float)Quantity * Listing.Price);
		}
	}
}
