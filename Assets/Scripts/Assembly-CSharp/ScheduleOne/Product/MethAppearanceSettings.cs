using System;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class MethAppearanceSettings
	{
		public Color32 MainColor;

		public Color32 SecondaryColor;

		public MethAppearanceSettings(Color32 mainColor, Color32 secondaryColor)
		{
			MainColor = mainColor;
			SecondaryColor = secondaryColor;
		}

		public MethAppearanceSettings()
		{
		}

		public bool IsUnintialized()
		{
			if (!(MainColor == Color.clear))
			{
				return SecondaryColor == Color.clear;
			}
			return true;
		}
	}
}
