using ScheduleOne.ItemFramework;
using ScheduleOne.Product;

namespace ScheduleOne.Equipping
{
	public class LiquidMeth_Equippable : Equippable_Viewmodel
	{
		public LiquidMethVisuals Visuals;

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			LiquidMethDefinition def = item.Definition as LiquidMethDefinition;
			if (Visuals != null)
			{
				Visuals.Setup(def);
			}
		}
	}
}
