using ScheduleOne.Employees;
using ScheduleOne.Management;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class ChemistUIElement : WorldspaceUIElement
	{
		[Header("References")]
		public Image[] StationsIcons;

		public Chemist AssignedChemist { get; protected set; }

		public void Initialize(Chemist chemist)
		{
			AssignedChemist = chemist;
			AssignedChemist.Configuration.onChanged.AddListener(RefreshUI);
			TitleLabel.text = chemist.fullName;
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			ChemistConfiguration chemistConfiguration = AssignedChemist.Configuration as ChemistConfiguration;
			for (int i = 0; i < StationsIcons.Length; i++)
			{
				if (chemistConfiguration.Stations.SelectedObjects.Count > i)
				{
					StationsIcons[i].sprite = chemistConfiguration.Stations.SelectedObjects[i].ItemInstance.Icon;
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
