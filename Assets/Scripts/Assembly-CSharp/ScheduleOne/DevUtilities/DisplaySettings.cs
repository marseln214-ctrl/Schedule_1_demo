using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	[Serializable]
	public struct DisplaySettings
	{
		public enum EDisplayMode
		{
			Windowed = 0,
			FullscreenWindow = 1,
			ExclusiveFullscreen = 2
		}

		public int ResolutionIndex;

		public EDisplayMode DisplayMode;

		public bool VSync;

		public int TargetFPS;

		public float UIScale;

		public float CameraBobbing;

		public static List<Resolution> GetResolutions()
		{
			Resolution[] resolutions = Screen.resolutions;
			RefreshRate refreshRateRatio = resolutions[resolutions.Length - 1].refreshRateRatio;
			float num = refreshRateRatio.numerator / refreshRateRatio.denominator;
			List<Resolution> list = new List<Resolution>();
			int i;
			for (i = 0; i < resolutions.Length; i++)
			{
				if (!list.Exists((Resolution x) => x.width == resolutions[i].width && x.height == resolutions[i].height))
				{
					Resolution item = resolutions[i];
					if ((float)(item.refreshRateRatio.numerator / item.refreshRateRatio.denominator) >= num - 0.1f)
					{
						list.Add(item);
					}
				}
			}
			return list;
		}
	}
}
