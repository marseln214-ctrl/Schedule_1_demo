using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using TMPro;

namespace ScheduleOne.UI.Items
{
	public class ItemUI_Cash : ItemUI
	{
		protected CashInstance cashInstance;

		public TextMeshProUGUI AmountLabel;

		public override void Setup(ItemInstance item)
		{
			cashInstance = item as CashInstance;
			base.Setup(item);
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			if (!Destroyed)
			{
				SetDisplayedBalance(cashInstance.Balance);
			}
		}

		public void SetDisplayedBalance(float balance)
		{
			AmountLabel.text = MoneyManager.FormatAmount(balance);
		}
	}
}
