using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Variables;

namespace ScheduleOne.Property
{
	public class Sweatshop : Property
	{
		private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002ESweatshopAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EProperty_002ESweatshopAssembly_002DCSharp_002Edll_Excuted;

		protected override void Start()
		{
			base.Start();
			InvokeRepeating("UpdateVariables", 0f, 0.5f);
		}

		private void UpdateVariables()
		{
			if (!NetworkSingleton<VariableDatabase>.InstanceExists || !InstanceFinder.IsServer)
			{
				return;
			}
			Pot[] componentsInChildren = Container.GetComponentsInChildren<Pot>();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = Container.GetComponentsInChildren<PackagingStation>().Length;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].IsFilledWithSoil)
				{
					num++;
				}
				if (componentsInChildren[i].NormalizedWaterLevel > 0.2f)
				{
					num2++;
				}
				if (componentsInChildren[i].Plant != null)
				{
					num3++;
				}
				if ((bool)componentsInChildren[i].AppliedAdditives.Find((Additive x) => x.AdditiveName == "Speed Grow"))
				{
					num4++;
				}
			}
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Sweatshop_Pots", componentsInChildren.Length.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Sweatshop_PackagingStations", num5.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Sweatshop_MixingStations", Container.GetComponentsInChildren<MixingStation>().Length.ToString());
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002ESweatshopAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EProperty_002ESweatshopAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EProperty_002ESweatshopAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EProperty_002ESweatshopAssembly_002DCSharp_002Edll_Excuted = true;
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
