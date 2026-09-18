using ScheduleOne.ItemFramework;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class QualityItemUI : ItemUI
	{
		public Image QualityIcon;

		protected QualityItemInstance qualityItemInstance;

		public override void Setup(ItemInstance item)
		{
			qualityItemInstance = item as QualityItemInstance;
			base.Setup(item);
		}

		public override void UpdateUI()
		{
			if (!Destroyed)
			{
				QualityIcon.color = ItemQuality.GetColor(qualityItemInstance.Quality);
				base.UpdateUI();
			}
		}
	}
}
