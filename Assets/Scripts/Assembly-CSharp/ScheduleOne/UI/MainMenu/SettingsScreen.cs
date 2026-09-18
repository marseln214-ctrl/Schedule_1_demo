using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu
{
	public class SettingsScreen : MainMenuScreen
	{
		[Serializable]
		public class SettingsCategory
		{
			public Button Button;

			public GameObject Panel;
		}

		public SettingsCategory[] Categories;

		public Button ApplyDisplayButton;

		public ConfirmDisplaySettings ConfirmDisplaySettings;

		protected override void Awake()
		{
			base.Awake();
			ApplyDisplayButton.onClick.AddListener(ApplyDisplaySettings);
			ApplyDisplayButton.gameObject.SetActive(value: false);
		}

		protected void Start()
		{
			for (int i = 0; i < Categories.Length; i++)
			{
				int index = i;
				Categories[i].Button.onClick.AddListener(delegate
				{
					ShowCategory(index);
				});
			}
			ShowCategory(0);
		}

		public void ShowCategory(int index)
		{
			for (int i = 0; i < Categories.Length; i++)
			{
				Categories[i].Button.interactable = i != index;
				Categories[i].Panel.SetActive(i == index);
			}
		}

		public void DisplayChanged()
		{
			ApplyDisplayButton.gameObject.SetActive(value: true);
		}

		private void ApplyDisplaySettings()
		{
			ApplyDisplayButton.gameObject.SetActive(value: false);
			DisplaySettings displaySettings = Singleton<ScheduleOne.DevUtilities.Settings>.Instance.DisplaySettings;
			DisplaySettings unappliedDisplaySettings = Singleton<ScheduleOne.DevUtilities.Settings>.Instance.UnappliedDisplaySettings;
			Singleton<ScheduleOne.DevUtilities.Settings>.Instance.ApplyDisplaySettings(unappliedDisplaySettings);
			ConfirmDisplaySettings.Open(displaySettings, unappliedDisplaySettings);
		}
	}
}
