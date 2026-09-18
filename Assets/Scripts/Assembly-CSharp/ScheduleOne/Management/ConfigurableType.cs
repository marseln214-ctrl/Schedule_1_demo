namespace ScheduleOne.Management
{
	public static class ConfigurableType
	{
		public static string GetTypeName(EConfigurableType type)
		{
			switch (type)
			{
			case EConfigurableType.Pot:
				return "Pot";
			case EConfigurableType.PackagingStation:
				return "Packaging Station";
			case EConfigurableType.LabOven:
				return "Lab Oven";
			case EConfigurableType.Botanist:
				return "Botanist";
			case EConfigurableType.Packager:
				return "Packager";
			case EConfigurableType.ChemistryStation:
				return "Chemistry Station";
			case EConfigurableType.Chemist:
				return "Chemist";
			case EConfigurableType.Cauldron:
				return "Cauldron";
			case EConfigurableType.Cleaner:
				return "Cleaner";
			case EConfigurableType.BrickPress:
				return "Brick Press";
			case EConfigurableType.MixingStation:
				return "Mixing Station";
			default:
				Console.LogError("Unknown Configurable Type: " + type);
				return string.Empty;
			}
		}
	}
}
