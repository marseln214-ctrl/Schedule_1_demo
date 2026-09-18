namespace ScheduleOne.Economy
{
	public struct DealWindowInfo
	{
		public const int WINDOW_DURATION_MINS = 360;

		public const int WINDOW_COUNT = 4;

		public int StartTime;

		public int EndTime;

		public static readonly DealWindowInfo Morning = new DealWindowInfo(600, 1200);

		public static readonly DealWindowInfo Afternoon = new DealWindowInfo(1200, 1800);

		public static readonly DealWindowInfo Night = new DealWindowInfo(1800, 2400);

		public static readonly DealWindowInfo LateNight = new DealWindowInfo(0, 600);

		public DealWindowInfo(int startTime, int endTime)
		{
			StartTime = startTime;
			EndTime = endTime;
		}

		public static DealWindowInfo GetWindowInfo(EDealWindow window)
		{
			return window switch
			{
				EDealWindow.Morning => Morning, 
				EDealWindow.Afternoon => Afternoon, 
				EDealWindow.Night => Night, 
				EDealWindow.LateNight => LateNight, 
				_ => Morning, 
			};
		}

		public static EDealWindow GetWindow(int time)
		{
			if (time >= Morning.StartTime && time < Morning.EndTime)
			{
				return EDealWindow.Morning;
			}
			if (time >= Afternoon.StartTime && time < Afternoon.EndTime)
			{
				return EDealWindow.Afternoon;
			}
			if (time >= Night.StartTime && time < Night.EndTime)
			{
				return EDealWindow.Night;
			}
			return EDealWindow.LateNight;
		}
	}
}
