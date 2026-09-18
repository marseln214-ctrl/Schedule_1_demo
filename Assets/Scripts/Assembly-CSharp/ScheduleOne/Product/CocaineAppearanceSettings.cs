using System;
using UnityEngine;

namespace ScheduleOne.Product
{
	[Serializable]
	public class CocaineAppearanceSettings
	{
		public Color32 MainColor;

		public Color32 SecondaryColor;

		public CocaineAppearanceSettings(Color32 mainColor, Color32 secondaryColor)
		{
			MainColor = mainColor;
			SecondaryColor = secondaryColor;
		}

		public CocaineAppearanceSettings()
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
