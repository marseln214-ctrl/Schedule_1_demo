using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management.UI;
using ScheduleOne.Tools;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Management
{
	public class ManagementInterface : Singleton<ManagementInterface>
	{
		[Serializable]
		public class ConfigurableTypePanel
		{
			public EConfigurableType Type;

			public ConfigPanel Panel;
		}

		public const float PANEL_SLIDE_TIME = 0.1f;

		[Header("References")]
		public TextMeshProUGUI NothingSelectedLabel;

		public TextMeshProUGUI DifferentTypesSelectedLabel;

		public RectTransform PanelContainer;

		public ClipboardScreen MainScreen;

		public ScheduleOne.UI.Management.ItemSelector ItemSelectorScreen;

		public NPCSelector NPCSelector;

		public ScheduleOne.UI.Management.ObjectSelector ObjectSelector;

		public RecipeSelector RecipeSelectorScreen;

		[SerializeField]
		protected ConfigurableTypePanel[] ConfigPanelPrefabs;

		public List<IConfigurable> Configurables = new List<IConfigurable>();

		private bool areConfigurablesUniform;

		private ConfigPanel loadedPanel;

		public ManagementClipboard_Equippable EquippedClipboard { get; protected set; }

		protected override void Start()
		{
			base.Start();
		}

		public void Open(List<IConfigurable> configurables, ManagementClipboard_Equippable _equippedClipboard)
		{
			Configurables = new List<IConfigurable>();
			Configurables.AddRange(configurables);
			EquippedClipboard = _equippedClipboard;
			areConfigurablesUniform = true;
			if (Configurables.Count > 1)
			{
				for (int i = 0; i < Configurables.Count - 1; i++)
				{
					if (Configurables[i].ConfigurableType != Configurables[i + 1].ConfigurableType)
					{
						areConfigurablesUniform = false;
						break;
					}
				}
			}
			UpdateMainLabels();
			InitializeConfigPanel();
			Singleton<InputPromptsCanvas>.Instance.LoadModule("backonly_rightclick");
		}

		public void Close(bool preserveState = false)
		{
			if (ItemSelectorScreen.IsOpen)
			{
				ItemSelectorScreen.Close();
			}
			if (RecipeSelectorScreen.IsOpen)
			{
				RecipeSelectorScreen.Close();
			}
			if (Singleton<InputPromptsCanvas>.Instance.currentModuleLabel == "exitonly")
			{
				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			}
			DestroyConfigPanel();
		}

		private void UpdateMainLabels()
		{
			NothingSelectedLabel.gameObject.SetActive(Configurables.Count == 0);
			DifferentTypesSelectedLabel.gameObject.SetActive(!areConfigurablesUniform);
		}

		private void InitializeConfigPanel()
		{
			if (areConfigurablesUniform && Configurables.Count != 0)
			{
				ConfigPanel configPanelPrefab = GetConfigPanelPrefab(Configurables[0].ConfigurableType);
				loadedPanel = UnityEngine.Object.Instantiate(configPanelPrefab, PanelContainer).GetComponent<ConfigPanel>();
				loadedPanel.Bind(Configurables.Select((IConfigurable x) => x.Configuration).ToList());
			}
		}

		private void DestroyConfigPanel()
		{
			if (loadedPanel != null)
			{
				UnityEngine.Object.Destroy(loadedPanel.gameObject);
				loadedPanel = null;
			}
		}

		public ConfigPanel GetConfigPanelPrefab(EConfigurableType type)
		{
			return ConfigPanelPrefabs.FirstOrDefault((ConfigurableTypePanel x) => x.Type == type).Panel;
		}
	}
}
