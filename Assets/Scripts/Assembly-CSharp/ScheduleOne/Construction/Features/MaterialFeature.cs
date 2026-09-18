using System;
using System.Collections.Generic;
using ScheduleOne.UI.Construction.Features;
using UnityEngine;

namespace ScheduleOne.Construction.Features
{
	public class MaterialFeature : OptionListFeature
	{
		[Serializable]
		public class NamedMaterial
		{
			public string matName;

			public Color buttonColor;

			public Material mat;

			public float price = 100f;
		}

		[Header("References")]
		[SerializeField]
		protected List<MeshRenderer> materialTargets = new List<MeshRenderer>();

		[Header("Material settings")]
		public List<NamedMaterial> materials = new List<NamedMaterial>();

		private bool NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EMaterialFeatureAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EMaterialFeatureAssembly_002DCSharp_002Edll_Excuted;

		public override void SelectOption(int optionIndex)
		{
			base.SelectOption(optionIndex);
			ApplyMaterial(materials[optionIndex]);
		}

		private void ApplyMaterial(NamedMaterial mat)
		{
			for (int i = 0; i < materialTargets.Count; i++)
			{
				materialTargets[i].material = mat.mat;
			}
		}

		protected override List<FI_OptionList.Option> GetOptions()
		{
			List<FI_OptionList.Option> list = new List<FI_OptionList.Option>();
			for (int i = 0; i < materials.Count; i++)
			{
				list.Add(new FI_OptionList.Option(materials[i].matName, materials[i].buttonColor, materials[i].price));
			}
			return list;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EMaterialFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EMaterialFeatureAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EMaterialFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EMaterialFeatureAssembly_002DCSharp_002Edll_Excuted = true;
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
