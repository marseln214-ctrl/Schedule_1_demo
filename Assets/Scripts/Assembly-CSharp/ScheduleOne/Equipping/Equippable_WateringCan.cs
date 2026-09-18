using System;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.PlayerTasks.Tasks;

namespace ScheduleOne.Equipping
{
	public class Equippable_WateringCan : Equippable_Pourable
	{
		public WateringCanVisuals Visuals;

		private WateringCanInstance WCInstance;

		public override string InteractionLabel { get; set; } = "Pour water";


		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			WCInstance = item as WateringCanInstance;
			UpdateVisuals();
			item.onDataChanged = (Action)Delegate.Combine(item.onDataChanged, new Action(UpdateVisuals));
		}

		public override void Unequip()
		{
			base.Unequip();
			if (WCInstance != null)
			{
				WateringCanInstance wCInstance = WCInstance;
				wCInstance.onDataChanged = (Action)Delegate.Remove(wCInstance.onDataChanged, new Action(UpdateVisuals));
			}
		}

		private void UpdateVisuals()
		{
			if (WCInstance != null)
			{
				Visuals.SetFillLevel(WCInstance.CurrentFillAmount / 15f);
			}
		}

		protected override bool CanPour(Pot pot, out string reason)
		{
			if (pot.SoilLevel < pot.SoilCapacity)
			{
				reason = "No soil";
				return false;
			}
			if (pot.NormalizedWaterLevel >= 0.975f)
			{
				reason = string.Empty;
				return false;
			}
			if ((itemInstance as WateringCanInstance).CurrentFillAmount <= 0f)
			{
				reason = "Watering can empty";
				return false;
			}
			return base.CanPour(pot, out reason);
		}

		protected override void StartPourTask(Pot pot)
		{
			new PourWaterTask(pot, itemInstance, PourablePrefab);
		}
	}
}
