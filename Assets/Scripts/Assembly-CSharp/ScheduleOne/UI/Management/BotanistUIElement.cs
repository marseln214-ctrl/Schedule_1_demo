using ScheduleOne.Employees;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class BotanistUIElement : WorldspaceUIElement
	{
		[Header("References")]
		public Image SupplyIcon;

		public GameObject NoSupply;

		public TextMeshProUGUI SupplyLabel;

		public RectTransform[] PotRects;

		public Botanist AssignedBotanist { get; protected set; }

		public void Initialize(Botanist bot)
		{
			AssignedBotanist = bot;
			AssignedBotanist.Configuration.onChanged.AddListener(RefreshUI);
			TitleLabel.text = bot.fullName;
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			BotanistConfiguration botanistConfiguration = AssignedBotanist.Configuration as BotanistConfiguration;
			NoSupply.gameObject.SetActive(botanistConfiguration.Supplies.SelectedObject == null);
			if (botanistConfiguration.Supplies.SelectedObject != null)
			{
				SupplyIcon.sprite = botanistConfiguration.Supplies.SelectedObject.ItemInstance.Icon;
				SupplyIcon.gameObject.SetActive(value: true);
			}
			else
			{
				SupplyIcon.gameObject.SetActive(value: false);
			}
			for (int i = 0; i < PotRects.Length; i++)
			{
				if (botanistConfiguration.Pots.SelectedObjects.Count > i)
				{
					PotRects[i].Find("Icon").GetComponent<Image>().sprite = botanistConfiguration.Pots.SelectedObjects[i].ItemInstance.Icon;
					PotRects[i].Find("Icon").gameObject.SetActive(value: true);
				}
				else
				{
					PotRects[i].Find("Icon").gameObject.SetActive(value: false);
				}
			}
		}
	}
}
