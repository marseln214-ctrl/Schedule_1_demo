using ScheduleOne.ObjectScripts.WateringCan;
using ScheduleOne.PlayerTasks;

namespace ScheduleOne.ObjectScripts
{
	public class FunctionalWateringCan : Pourable
	{
		public WateringCanVisuals Visuals;

		private WateringCanInstance itemInstance;

		public void Setup(WateringCanInstance instance)
		{
			itemInstance = instance;
			autoSetCurrentQuantity = false;
			currentQuantity = itemInstance.CurrentFillAmount;
			Visuals.SetFillLevel(itemInstance.CurrentFillAmount / 15f);
			base.Rb.isKinematic = false;
		}

		protected override void PourAmount(float amount)
		{
			itemInstance.ChangeFillAmount(0f - amount);
			Visuals.SetFillLevel(itemInstance.CurrentFillAmount / 15f);
			base.PourAmount(amount);
		}
	}
}
