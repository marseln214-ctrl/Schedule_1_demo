using ScheduleOne.ItemFramework;
using ScheduleOne.Product;

namespace ScheduleOne.StationFramework
{
	public class LiquidMeth_StationItem : StationItem
	{
		public LiquidMethVisuals Visuals;

		public override void Initialize(StorableItemDefinition itemDefinition)
		{
			base.Initialize(itemDefinition);
			LiquidMethDefinition liquidMethDefinition = itemDefinition as LiquidMethDefinition;
			if (Visuals != null)
			{
				Visuals.Setup(liquidMethDefinition);
			}
			GetModule<CookableModule>().LiquidColor = liquidMethDefinition.CookableLiquidColor;
			GetModule<CookableModule>().SolidColor = liquidMethDefinition.CookableSolidColor;
			GetModule<PourableModule>().LiquidColor = liquidMethDefinition.LiquidVolumeColor;
			GetModule<PourableModule>().PourParticlesColor = liquidMethDefinition.PourParticlesColor;
		}
	}
}
