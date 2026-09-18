using UnityEngine;

namespace Funly.SkyStudio
{
	public static class ColorBlendingExtensions
	{
		public static Color Clear(this Color color)
		{
			return new Color(color.r, color.g, color.b, 0f);
		}
	}
}
