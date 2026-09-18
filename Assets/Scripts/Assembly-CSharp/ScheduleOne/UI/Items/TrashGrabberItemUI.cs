using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.WateringCan;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Items
{
	public class TrashGrabberItemUI : ItemUI
	{
		public TextMeshProUGUI ValueLabel;

		protected TrashGrabberInstance trashGrabberInstance;

		public override void Setup(ItemInstance item)
		{
			trashGrabberInstance = item as TrashGrabberInstance;
			base.Setup(item);
		}

		public override void UpdateUI()
		{
			if (!Destroyed)
			{
				ValueLabel.text = Mathf.FloorToInt(Mathf.Clamp01((float)trashGrabberInstance.GetTotalSize() / 20f) * 100f) + "%";
				base.UpdateUI();
			}
		}
	}
}
