using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerTasks.Tasks;
using UnityEngine;

namespace ScheduleOne.PlayerTasks
{
	public class ApplyAdditiveToPot : PourIntoPotTask
	{
		private AdditiveDefinition def;

		protected override bool UseCoverage => true;

		protected override Pot.ECameraPosition CameraPosition => Pot.ECameraPosition.BirdsEye;

		public ApplyAdditiveToPot(Pot _pot, ItemInstance _itemInstance, Pourable _pourablePrefab)
			: base(_pot, _itemInstance, _pourablePrefab)
		{
			def = _itemInstance.Definition as AdditiveDefinition;
			base.CurrentInstruction = "Cover soil with " + def.AdditivePrefab.AdditiveName + " (0%)";
			removeItemAfterInitialPour = false;
			pot.SoilCover.ConfigureAppearance((pourable as PourableAdditive).LiquidColor, 0.3f);
		}

		public override void Update()
		{
			base.Update();
			int num = Mathf.FloorToInt(pot.SoilCover.GetNormalizedProgress() * 100f);
			base.CurrentInstruction = "Cover soil with " + def.AdditivePrefab.AdditiveName + " (" + num + "%)";
		}

		protected override void FullyCovered()
		{
			base.FullyCovered();
			pot.SendAdditive((pourable as PourableAdditive).AdditiveDefinition.AdditivePrefab.AssetPath, initial: true);
			RemoveItem();
			Success();
		}
	}
}
