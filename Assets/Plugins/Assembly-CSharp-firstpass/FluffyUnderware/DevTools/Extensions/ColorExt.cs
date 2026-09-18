using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class ColorExt
	{
		public static string ToHtml(this Color c)
		{
			Color32 color = c;
			return $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";
		}
	}
}
