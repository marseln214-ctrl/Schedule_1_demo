using System;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class DateTimeData : SaveData
	{
		public int Year;

		public int Month;

		public int Day;

		public int Hour;

		public int Minute;

		public int Second;

		public DateTimeData(DateTime date)
		{
			Year = date.Year;
			Month = date.Month;
			Day = date.Day;
			Hour = date.Hour;
			Minute = date.Minute;
			Second = date.Second;
		}

		public DateTime GetDateTime()
		{
			return new DateTime(Year, Month, Day, Hour, Minute, Second);
		}
	}
}
