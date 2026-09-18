using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.WateringCan;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class ItemUI_WateringCan : ItemUI
	{
		protected WateringCanInstance wcInstance;

		public Text AmountLabel;

		public override void Setup(ItemInstance item)
		{
			wcInstance = item as WateringCanInstance;
			base.Setup(item);
		}

		public override void UpdateUI()
		{
			base.UpdateUI();
			if (!Destroyed && wcInstance != null)
			{
				AmountLabel.text = (float)Mathf.RoundToInt(wcInstance.CurrentFillAmount * 10f) / 10f + "L";
			}
		}
	}
}
