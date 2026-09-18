using ScheduleOne.ItemFramework;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Items
{
	public class ItemInfoContent : MonoBehaviour
	{
		[Header("Settings")]
		public float Height = 90f;

		[Header("References")]
		public TextMeshProUGUI NameLabel;

		public TextMeshProUGUI DescriptionLabel;

		public virtual void Initialize(ItemInstance instance)
		{
			NameLabel.text = instance.Name;
			DescriptionLabel.text = instance.Description;
		}

		public virtual void Initialize(ItemDefinition definition)
		{
			NameLabel.text = definition.Name;
			DescriptionLabel.text = definition.Description;
		}
	}
}
