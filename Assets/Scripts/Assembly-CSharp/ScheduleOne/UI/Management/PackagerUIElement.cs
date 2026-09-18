using ScheduleOne.Employees;
using ScheduleOne.Management;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management
{
	public class PackagerUIElement : WorldspaceUIElement
	{
		[Header("References")]
		public RectTransform[] StationRects;

		public Packager AssignedPackager { get; protected set; }

		public void Initialize(Packager packager)
		{
			AssignedPackager = packager;
			AssignedPackager.Configuration.onChanged.AddListener(RefreshUI);
			TitleLabel.text = packager.fullName;
			RefreshUI();
			base.gameObject.SetActive(value: false);
		}

		protected virtual void RefreshUI()
		{
			PackagerConfiguration packagerConfiguration = AssignedPackager.Configuration as PackagerConfiguration;
			for (int i = 0; i < StationRects.Length; i++)
			{
				if (packagerConfiguration.Stations.SelectedObjects.Count > i)
				{
					StationRects[i].Find("Icon").GetComponent<Image>().sprite = packagerConfiguration.Stations.SelectedObjects[i].ItemInstance.Icon;
					StationRects[i].Find("Icon").gameObject.SetActive(value: true);
				}
				else
				{
					StationRects[i].Find("Icon").gameObject.SetActive(value: false);
				}
			}
		}
	}
}
