using ScheduleOne.ItemFramework;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class IntegerItemUI : ItemUI
	{
		public Text ValueLabel;

		protected IntegerItemInstance integerItemInstance;

		public override void Setup(ItemInstance item)
		{
			integerItemInstance = item as IntegerItemInstance;
			base.Setup(item);
		}

		public override void UpdateUI()
		{
			if (!Destroyed)
			{
				ValueLabel.text = integerItemInstance.Value.ToString();
				base.UpdateUI();
			}
		}
	}
}
