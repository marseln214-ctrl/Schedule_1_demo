using System.Collections.Generic;
using ScheduleOne.UI.Construction.Features;
using UnityEngine;

namespace ScheduleOne.Construction.Features
{
	public class GenericOptionListFeature : OptionListFeature
	{
		[Header("References")]
		[SerializeField]
		protected List<GenericOption> options = new List<GenericOption>();

		private GenericOption visibleOption;

		private GenericOption installedOption;

		private bool NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EGenericOptionListFeatureAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EGenericOptionListFeatureAssembly_002DCSharp_002Edll_Excuted;

		public override void Default()
		{
			for (int i = 0; i < options.Count; i++)
			{
				options[i].Uninstall();
			}
			PurchaseOption(defaultOptionIndex);
		}

		protected override List<FI_OptionList.Option> GetOptions()
		{
			List<FI_OptionList.Option> list = new List<FI_OptionList.Option>();
			foreach (GenericOption option in options)
			{
				list.Add(new FI_OptionList.Option(option.optionName, option.optionButtonColor, option.optionPrice));
			}
			return list;
		}

		public override void SelectOption(int optionIndex)
		{
			base.SelectOption(optionIndex);
			if (visibleOption != null && options[optionIndex] != visibleOption)
			{
				visibleOption.SetInvisible();
			}
			visibleOption = options[optionIndex];
			visibleOption.SetVisible();
		}

		public override void PurchaseOption(int optionIndex)
		{
			base.PurchaseOption(optionIndex);
			if (installedOption != null && options[optionIndex] != installedOption)
			{
				installedOption.Uninstall();
			}
			installedOption = options[optionIndex];
			installedOption.Install();
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EGenericOptionListFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EGenericOptionListFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EGenericOptionListFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EGenericOptionListFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
