namespace LiquidVolumeFX
{
	public static class DetailExtensions
	{
		public static bool allowsRefraction(this DETAIL detail)
		{
			return detail != DETAIL.DefaultNoFlask;
		}

		public static bool usesFlask(this DETAIL detail)
		{
			if (detail != 0)
			{
				return detail == DETAIL.Default;
			}
			return true;
		}
	}
}
