using System;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Tools
{
	public class CountdownText : MonoBehaviour
	{
		public TextMeshProUGUI TimeLabel;

		[Header("Date Setting")]
		public int Year = 2025;

		public int Month = 3;

		public int Day = 24;

		public int Hour = 16;

		public int Minute;

		public int Second;

		private DateTime targetPDTDate;

		private void Start()
		{
			TimeZoneInfo sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
			DateTime dateTime = new DateTime(Year, Month, Day, Hour, Minute, Second, DateTimeKind.Unspecified);
			targetPDTDate = TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
		}

		private void Update()
		{
			UpdateCountdown();
		}

		private void UpdateCountdown()
		{
			DateTime utcNow = DateTime.UtcNow;
			TimeSpan timeSpan = targetPDTDate - utcNow;
			if (timeSpan.TotalSeconds > 0.0)
			{
				TimeLabel.text = FormatTime(timeSpan);
			}
			else
			{
				TimeLabel.text = "Now available!";
			}
		}

		private string FormatTime(TimeSpan timeSpan)
		{
			return timeSpan.Days + " days, " + timeSpan.Hours + " hours, " + timeSpan.Minutes + " minutes";
		}
	}
}
