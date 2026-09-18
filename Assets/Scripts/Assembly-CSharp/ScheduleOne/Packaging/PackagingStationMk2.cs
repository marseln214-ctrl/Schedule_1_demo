using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.UI;
using ScheduleOne.Variables;

namespace ScheduleOne.Packaging
{
	public class PackagingStationMk2 : PackagingStation
	{
		public PackagingTool PackagingTool;

		private bool NetworkInitialize___EarlyScheduleOne_002EPackaging_002EPackagingStationMk2Assembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EPackaging_002EPackagingStationMk2Assembly_002DCSharp_002Edll_Excuted;

		public override void StartTask()
		{
			new PackageProductTaskMk2(this);
			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("PackagingStationMk2TutorialDone"))
			{
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("PackagingStationMk2TutorialDone", true.ToString());
				Singleton<TaskManagerUI>.Instance.PackagingStationMK2TutorialDone.Open();
			}
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EPackaging_002EPackagingStationMk2Assembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EPackaging_002EPackagingStationMk2Assembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EPackaging_002EPackagingStationMk2Assembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EPackaging_002EPackagingStationMk2Assembly_002DCSharp_002Edll_Excuted = true;
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
