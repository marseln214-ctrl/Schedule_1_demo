namespace ScheduleOne.Property
{
	public class LaunderingOperation
	{
		public Business business;

		public float amount;

		public int minutesSinceStarted;

		public int completionTime_Minutes = 1440;

		public LaunderingOperation(Business _business, float _amount, int _minutesSinceStarted)
		{
			business = _business;
			amount = _amount;
			minutesSinceStarted = _minutesSinceStarted;
		}
	}
}
