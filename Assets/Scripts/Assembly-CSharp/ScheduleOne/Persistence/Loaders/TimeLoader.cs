using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class TimeLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents))
			{
				TimeData timeData = JsonUtility.FromJson<TimeData>(contents);
				if (timeData != null)
				{
					NetworkSingleton<TimeManager>.Instance.SetTime(timeData.TimeOfDay);
					NetworkSingleton<TimeManager>.Instance.SetElapsedDays(timeData.ElapsedDays);
					NetworkSingleton<TimeManager>.Instance.SetPlaytime(timeData.Playtime);
				}
			}
		}
	}
}
