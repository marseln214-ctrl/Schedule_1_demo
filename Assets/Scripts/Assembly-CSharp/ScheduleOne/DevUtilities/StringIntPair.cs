using System;

namespace ScheduleOne.DevUtilities
{
	[Serializable]
	public class StringIntPair
	{
		public string String;

		public int Int;

		public StringIntPair(string str, int i)
		{
			String = str;
			Int = i;
		}

		public StringIntPair()
		{
		}
	}
}
