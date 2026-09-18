using UnityEngine;

namespace ScheduleOne.ItemFramework
{
	public static class ItemQuality
	{
		public const float Heavenly_Threshold = 0.9f;

		public const float Premium_Threshold = 0.7f;

		public const float Standard_Threshold = 0.4f;

		public const float Poor_Threshold = 0.2f;

		public static Color Heavenly_Color = new Color32(byte.MaxValue, 200, 50, byte.MaxValue);

		public static Color Premium_Color = new Color32(225, 75, byte.MaxValue, byte.MaxValue);

		public static Color Standard_Color = new Color32(100, 190, byte.MaxValue, byte.MaxValue);

		public static Color Poor_Color = new Color32(80, 145, 50, byte.MaxValue);

		public static Color Trash_Color = new Color32(125, 50, 50, byte.MaxValue);

		public static EQuality GetQuality(float qualityScalar)
		{
			if (qualityScalar > 0.9f)
			{
				return EQuality.Heavenly;
			}
			if (qualityScalar > 0.7f)
			{
				return EQuality.Premium;
			}
			if (qualityScalar > 0.4f)
			{
				return EQuality.Standard;
			}
			if (qualityScalar > 0.2f)
			{
				return EQuality.Poor;
			}
			return EQuality.Trash;
		}

		public static Color GetColor(EQuality quality)
		{
			switch (quality)
			{
			case EQuality.Heavenly:
				return Heavenly_Color;
			case EQuality.Premium:
				return Premium_Color;
			case EQuality.Standard:
				return Standard_Color;
			case EQuality.Poor:
				return Poor_Color;
			case EQuality.Trash:
				return Trash_Color;
			default:
				Console.LogWarning("Quality color not found!");
				return Color.white;
			}
		}
	}
}
