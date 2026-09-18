using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerTasks;

namespace ScheduleOne.Equipping
{
	public class Equippable_Additive : Equippable_Pourable
	{
		private AdditiveDefinition additiveDef;

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			additiveDef = itemInstance.Definition as AdditiveDefinition;
			InteractionLabel = "Apply " + additiveDef.Name;
		}

		protected override void StartPourTask(Pot pot)
		{
			new ApplyAdditiveToPot(pot, itemInstance, PourablePrefab);
		}

		protected override bool CanPour(Pot pot, out string reason)
		{
			if (pot.SoilLevel < pot.SoilCapacity)
			{
				reason = "No soil";
				return false;
			}
			if (pot.Plant == null)
			{
				reason = "No plant";
				return false;
			}
			if (pot.GetAdditive(additiveDef.AdditivePrefab.AdditiveName) != null)
			{
				reason = "Already contains " + additiveDef.AdditivePrefab.AdditiveName;
				return false;
			}
			return base.CanPour(pot, out reason);
		}
	}
}
