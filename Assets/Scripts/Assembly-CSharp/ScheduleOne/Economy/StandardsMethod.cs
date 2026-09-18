using ScheduleOne.ItemFramework;

namespace ScheduleOne.Economy
{
	public static class StandardsMethod
	{
		public static string GetName(this ECustomerStandard property)
		{
			return property switch
			{
				ECustomerStandard.VeryLow => "Very Low", 
				ECustomerStandard.Low => "Low", 
				ECustomerStandard.Moderate => "Moderate", 
				ECustomerStandard.High => "High", 
				ECustomerStandard.VeryHigh => "Very High", 
				_ => "Standard", 
			};
		}

		public static EQuality GetCorrespondingQuality(this ECustomerStandard property)
		{
			return property switch
			{
				ECustomerStandard.VeryLow => EQuality.Trash, 
				ECustomerStandard.Low => EQuality.Poor, 
				ECustomerStandard.Moderate => EQuality.Standard, 
				ECustomerStandard.High => EQuality.Premium, 
				ECustomerStandard.VeryHigh => EQuality.Heavenly, 
				_ => EQuality.Standard, 
			};
		}
	}
}
