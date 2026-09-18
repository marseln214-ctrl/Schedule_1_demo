using ScheduleOne.Equipping;
using ScheduleOne.PlayerTasks.Tasks;

namespace ScheduleOne.ObjectScripts.Soil
{
	public class Equippable_Soil : Equippable_Pourable
	{
		public override string InteractionLabel { get; set; } = "Pour soil";


		protected override bool CanPour(Pot pot, out string reason)
		{
			if (pot.SoilLevel >= pot.SoilCapacity)
			{
				reason = "Pot already full";
				return false;
			}
			if (!string.IsNullOrEmpty(pot.SoilID) && pot.SoilID != itemInstance.ID)
			{
				reason = "Soil type mismatch";
				return false;
			}
			return base.CanPour(pot, out reason);
		}

		protected override void StartPourTask(Pot pot)
		{
			new PourSoilTask(pot, itemInstance, PourablePrefab);
		}
	}
}
