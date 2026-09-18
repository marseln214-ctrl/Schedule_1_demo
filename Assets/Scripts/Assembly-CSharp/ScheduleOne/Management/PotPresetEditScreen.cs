using ScheduleOne.DevUtilities;
using ScheduleOne.Management.Presets;
using ScheduleOne.Management.Presets.Options;
using ScheduleOne.Management.SetterScreens;

namespace ScheduleOne.Management
{
	public class PotPresetEditScreen : PresetEditScreen
	{
		public GenericOptionUI SeedsUI;

		public GenericOptionUI AdditivesUI;

		private PotPreset castedPreset;

		protected override void Awake()
		{
			base.Awake();
			SeedsUI.Button.onClick.AddListener(SeedsUIClicked);
			AdditivesUI.Button.onClick.AddListener(AdditivesUIClicked);
		}

		protected virtual void Update()
		{
			if (base.isOpen)
			{
				UpdateUI();
			}
		}

		public override void Open(Preset preset)
		{
			base.Open(preset);
			castedPreset = (PotPreset)EditedPreset;
			UpdateUI();
		}

		private void UpdateUI()
		{
			SeedsUI.ValueLabel.text = castedPreset.Seeds.GetDisplayString();
			AdditivesUI.ValueLabel.text = castedPreset.Additives.GetDisplayString();
		}

		public void SeedsUIClicked()
		{
			Singleton<ItemSetterScreen>.Instance.Open((EditedPreset as PotPreset).Seeds);
		}

		public void AdditivesUIClicked()
		{
			Singleton<ItemSetterScreen>.Instance.Open((EditedPreset as PotPreset).Additives);
		}
	}
}
