using System;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class WeedAppearanceSettings
	{
		public Color32 MainColor;

		public Color32 SecondaryColor;

		public Color32 LeafColor;

		public Color32 StemColor;

		public WeedAppearanceSettings(Color32 mainColor, Color32 secondaryColor, Color32 leafColor, Color32 stemColor)
		{
			MainColor = mainColor;
			SecondaryColor = secondaryColor;
			LeafColor = leafColor;
			StemColor = stemColor;
		}

		public WeedAppearanceSettings()
		{
		}

		public bool IsUnintialized()
		{
			if (!(MainColor == Color.clear) && !(SecondaryColor == Color.clear) && !(LeafColor == Color.clear))
			{
				return StemColor == Color.clear;
			}
			return true;
		}
	}
}
