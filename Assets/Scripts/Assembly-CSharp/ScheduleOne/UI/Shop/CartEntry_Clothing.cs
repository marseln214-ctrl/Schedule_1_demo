using ScheduleOne.Clothing;
using TMPro;

namespace ScheduleOne.UI.Shop
{
	public class CartEntry_Clothing : CartEntry
	{
		protected override void UpdateTitle()
		{
			base.UpdateTitle();
			if ((base.Listing.Item as ClothingDefinition).Colorable)
			{
				TextMeshProUGUI titleLabel = TitleLabel;
				titleLabel.text = titleLabel.text + " (" + (base.Listing as ClothingShopListing).Color.GetLabel() + ")";
			}
		}
	}
}
