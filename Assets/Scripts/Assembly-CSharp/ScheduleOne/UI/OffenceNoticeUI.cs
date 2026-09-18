using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI
{
	public class OffenceNoticeUI : Singleton<OffenceNoticeUI>
	{
		[Header("References")]
		[SerializeField]
		protected GameObject container;

		[SerializeField]
		protected List<Text> charges = new List<Text>();

		[SerializeField]
		protected List<Text> penalties = new List<Text>();

		public void ShowOffenceNotice(Offense offence)
		{
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			for (int i = 0; i < charges.Count; i++)
			{
				if (i < offence.charges.Count)
				{
					string text = "- ";
					if (offence.charges[i].quantity > 1)
					{
						text = "- " + offence.charges[i].quantity + "x ";
					}
					charges[i].text = text + offence.charges[i].chargeName;
					charges[i].enabled = true;
				}
				else
				{
					charges[i].enabled = false;
				}
			}
			for (int j = 0; j < penalties.Count; j++)
			{
				if (j < offence.penalties.Count)
				{
					string text2 = "- ";
					penalties[j].text = text2 + offence.penalties[j];
					penalties[j].enabled = true;
				}
				else
				{
					penalties[j].enabled = false;
				}
			}
			container.gameObject.SetActive(value: true);
		}

		protected void Update()
		{
			if (container.activeSelf && GameInput.GetButtonDown(GameInput.ButtonCode.Escape))
			{
				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
				container.gameObject.SetActive(value: false);
				PlayerSingleton<PlayerMovement>.Instance.canMove = true;
				PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
				Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
				PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.2f);
			}
		}
	}
}
