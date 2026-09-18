using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class PotUIElement : WorldspaceUIElement
	{
		[Header("References")]
		public Image SeedIcon;

		public GameObject NoSeed;

		public Image Additive1Icon;

		public Image Additive2Icon;

		public Image Additive3Icon;

		public Pot AssignedPot { get; protected set; }

		public void Initialize(Pot pot)
		{
			AssignedPot = pot;
			AssignedPot.Configuration.onChanged.AddListener(RefreshUI);
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			PotConfiguration potConfiguration = AssignedPot.Configuration as PotConfiguration;
			NoSeed.gameObject.SetActive(potConfiguration.Seed.SelectedItem == null);
			SeedIcon.gameObject.SetActive(potConfiguration.Seed.SelectedItem != null);
			if (potConfiguration.Seed.SelectedItem != null)
			{
				SeedIcon.sprite = potConfiguration.Seed.SelectedItem.Icon;
			}
			if (potConfiguration.Additive1.SelectedItem != null)
			{
				Additive1Icon.sprite = potConfiguration.Additive1.SelectedItem.Icon;
				Additive1Icon.gameObject.SetActive(value: true);
			}
			else
			{
				Additive1Icon.gameObject.SetActive(value: false);
			}
			if (potConfiguration.Additive2.SelectedItem != null)
			{
				Additive2Icon.sprite = potConfiguration.Additive2.SelectedItem.Icon;
				Additive2Icon.gameObject.SetActive(value: true);
			}
			else
			{
				Additive2Icon.gameObject.SetActive(value: false);
			}
			if (potConfiguration.Additive3.SelectedItem != null)
			{
				Additive3Icon.sprite = potConfiguration.Additive3.SelectedItem.Icon;
				Additive3Icon.gameObject.SetActive(value: true);
			}
			else
			{
				Additive3Icon.gameObject.SetActive(value: false);
			}
			SetAssignedNPC(potConfiguration.AssignedBotanist.SelectedNPC);
		}
	}
}
