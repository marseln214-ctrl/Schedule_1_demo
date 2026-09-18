using System;
using ScheduleOne.Persistence.Datas;

namespace ScheduleOne.GameTime
{
	[Serializable]
	public struct GameDateTime
	{
		public int elapsedDays;

		public int time;

		public GameDateTime(int _elapsedDays, int _time)
		{
			elapsedDays = _elapsedDays;
			time = _time;
		}

		public GameDateTime(int _minSum)
		{
			elapsedDays = _minSum / 1440;
			int minSum = _minSum % 1440;
			if (_minSum < 0)
			{
				minSum = -_minSum % 1440;
			}
			time = TimeManager.Get24HourTimeFromMinSum(minSum);
		}

		public GameDateTime(GameDateTimeData data)
		{
			elapsedDays = data.ElapsedDays;
			time = data.Time;
		}

		public int GetMinSum()
		{
			return elapsedDays * 1440 + TimeManager.GetMinSumFrom24HourTime(time);
		}

		public GameDateTime AddMins(int mins)
		{
			return new GameDateTime(GetMinSum() + mins);
		}

		public static GameDateTime operator +(GameDateTime a, GameDateTime b)
		{
			return new GameDateTime(a.GetMinSum() + b.GetMinSum());
		}

		public static GameDateTime operator -(GameDateTime a, GameDateTime b)
		{
			return new GameDateTime(a.GetMinSum() - b.GetMinSum());
		}

		public static bool operator >(GameDateTime a, GameDateTime b)
		{
			return a.GetMinSum() > b.GetMinSum();
		}

		public static bool operator <(GameDateTime a, GameDateTime b)
		{
			return a.GetMinSum() < b.GetMinSum();
		}
	}
}
