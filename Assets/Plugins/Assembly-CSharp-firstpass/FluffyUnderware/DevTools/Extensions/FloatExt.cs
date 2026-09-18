using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class FloatExt
	{
		public static bool IsBetween0And1(this float v)
		{
			if (v >= 0f)
			{
				return v <= 1f;
			}
			return false;
		}

		public static bool IsBetween(this float v, float a, float b)
		{
			if (!(v >= a) || !(v <= b))
			{
				if (v >= b)
				{
					return v <= a;
				}
				return false;
			}
			return true;
		}

		public static float Repeat(this float v, float min, float max)
		{
			return min + Mathf.Repeat(v - min, max - min);
		}
	}
}
