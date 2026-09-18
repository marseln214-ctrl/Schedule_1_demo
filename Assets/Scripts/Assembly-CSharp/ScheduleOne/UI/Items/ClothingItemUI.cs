using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class ClothingItemUI : ItemUI
	{
		public Image ClothingTypeIcon;

		public override void UpdateUI()
		{
			base.UpdateUI();
			ClothingInstance clothingInstance = itemInstance as ClothingInstance;
			if (itemInstance != null && (itemInstance.Definition as ClothingDefinition).Colorable)
			{
				IconImg.color = clothingInstance.Color.GetActualColor();
			}
			else
			{
				IconImg.color = Color.white;
			}
			if (itemInstance != null)
			{
				ClothingTypeIcon.sprite = Singleton<ClothingUtility>.Instance.GetSlotData((itemInstance.Definition as ClothingDefinition).Slot).Icon;
			}
			else
			{
				ClothingTypeIcon.sprite = null;
			}
		}
	}
}
