using FishNet.Object;
using ScheduleOne.UI.Construction.Features;
using UnityEngine;

namespace ScheduleOne.Construction.Features
{
	public abstract class Feature : NetworkBehaviour
	{
		public string featureName = "Feature name";

		public Sprite featureIcon;

		public Transform featureIconLocation;

		public GameObject featureInterfacePrefab;

		public bool disableRoofDisibility;

		private bool NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EFeatureAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EFeatureAssembly_002DCSharp_002Edll_Excuted;

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			Awake_UserLogic_ScheduleOne_002EConstruction_002EFeatures_002EFeature_Assembly_002DCSharp_002Edll();
			NetworkInitialize__Late();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
		}

		public virtual FI_Base CreateInterface(Transform parent)
		{
			FI_Base component = Object.Instantiate(featureInterfacePrefab, parent).GetComponent<FI_Base>();
			component.Initialize(this);
			return component;
		}

		public abstract void Default();

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EConstruction_002EFeatures_002EFeatureAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EFeatureAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EConstruction_002EFeatures_002EFeatureAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		protected virtual void Awake_UserLogic_ScheduleOne_002EConstruction_002EFeatures_002EFeature_Assembly_002DCSharp_002Edll()
		{
		}
	}
}
