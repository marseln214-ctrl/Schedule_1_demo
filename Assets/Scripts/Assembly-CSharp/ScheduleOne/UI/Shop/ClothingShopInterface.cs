using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;

namespace ScheduleOne.UI.Shop
{
	public class ClothingShopInterface : ShopInterface
	{
		public ShopColorPicker ColorPicker;

		private ShopListing _selectedListing;

		protected override void Start()
		{
			base.Start();
			ColorPicker.onColorPicked.AddListener(ColorPicked);
		}

		public override void ListingClicked(ListingUI listingUI)
		{
			if (listingUI.Listing.Item.IsPurchasable)
			{
				if ((listingUI.Listing.Item as ClothingDefinition).Colorable)
				{
					_selectedListing = listingUI.Listing;
					ColorPicker.Open(listingUI.Listing.Item);
				}
				else
				{
					base.ListingClicked(listingUI);
				}
			}
		}

		protected override void Exit(ExitAction action)
		{
			if (!action.used)
			{
				if (ColorPicker.IsOpen)
				{
					action.used = true;
					ColorPicker.Close();
				}
				base.Exit(action);
			}
		}

		private void ColorPicked(EClothingColor color)
		{
			if (_selectedListing != null)
			{
				ClothingShopListing clothingShopListing = new ClothingShopListing();
				clothingShopListing.Item = _selectedListing.Item;
				clothingShopListing.Color = color;
				Cart.AddItem(clothingShopListing, 1);
				AddItemSound.Play();
			}
		}

		public override void HandoverItems()
		{
		}
	}
}
