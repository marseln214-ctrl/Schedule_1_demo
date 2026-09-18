using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks
{
	public class PourWaterTask : PourOntoTargetTask
	{
		public const float NORMALIZED_FILL_PER_TARGET = 0.2f;

		public static bool hintShown;

		protected override bool UseCoverage => true;

		protected override bool FailOnEmpty => false;

		protected override Pot.ECameraPosition CameraPosition => Pot.ECameraPosition.BirdsEye;

		public PourWaterTask(Pot _pot, ItemInstance _itemInstance, Pourable _pourablePrefab)
			: base(_pot, _itemInstance, _pourablePrefab)
		{
			base.CurrentInstruction = "Pour water over target";
			removeItemAfterInitialPour = false;
			pourable.GetComponent<FunctionalWateringCan>().Setup(_itemInstance as WateringCanInstance);
			pourable.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			pot.SoilCover.ConfigureAppearance(Color.black, 0.6f);
			if (NetworkSingleton<GameManager>.Instance.IsTutorial && !hintShown)
			{
				hintShown = true;
				Singleton<HintDisplay>.Instance.ShowHint_20s("While dragging an item, press <Input_Left> or <Input_Right> to rotate it.");
			}
		}

		public override void StopTask()
		{
			pot.PushWaterDataToServer();
			base.StopTask();
		}

		public override void TargetReached()
		{
			pot.ChangeWaterAmount(0.2f * pot.WaterCapacity);
			pot.PushWaterDataToServer();
			if (pot.NormalizedWaterLevel >= 0.975f)
			{
				Success();
				float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("WateredPotsCount");
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("WateredPotsCount", (value + 1f).ToString());
			}
			base.TargetReached();
		}
	}
}
