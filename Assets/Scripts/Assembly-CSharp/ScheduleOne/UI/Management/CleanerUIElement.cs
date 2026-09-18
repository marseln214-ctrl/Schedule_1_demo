using ScheduleOne.Employees;
using ScheduleOne.Management;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class CleanerUIElement : WorldspaceUIElement
	{
		[Header("References")]
		public Image[] StationsIcons;

		public Cleaner AssignedCleaner { get; protected set; }

		public void Initialize(Cleaner cleaner)
		{
			AssignedCleaner = cleaner;
			AssignedCleaner.Configuration.onChanged.AddListener(RefreshUI);
			TitleLabel.text = cleaner.fullName;
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			CleanerConfiguration cleanerConfiguration = AssignedCleaner.Configuration as CleanerConfiguration;
			for (int i = 0; i < StationsIcons.Length; i++)
			{
				if (cleanerConfiguration.Bins.SelectedObjects.Count > i)
				{
					StationsIcons[i].sprite = cleanerConfiguration.Bins.SelectedObjects[i].ItemInstance.Icon;
					StationsIcons[i].enabled = true;
				}
				else
				{
					StationsIcons[i].enabled = false;
				}
			}
		}
	}
}
