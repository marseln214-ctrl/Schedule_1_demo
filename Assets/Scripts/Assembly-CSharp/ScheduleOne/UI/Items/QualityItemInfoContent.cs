using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine.UI;

namespace ScheduleOne.UI.Items
{
	public class QualityItemInfoContent : ItemInfoContent
	{
		public Image Star;

		public TextMeshProUGUI QualityLabel;

		public override void Initialize(ItemInstance instance)
		{
			base.Initialize(instance);
			if (!(instance is QualityItemInstance qualityItemInstance))
			{
				Console.LogError("QualityItemInfoContent can only be used with QualityItemInstance!");
				return;
			}
			QualityLabel.text = qualityItemInstance.Quality.ToString();
			QualityLabel.color = ItemQuality.GetColor(qualityItemInstance.Quality);
			Star.color = ItemQuality.GetColor(qualityItemInstance.Quality);
		}
	}
}
