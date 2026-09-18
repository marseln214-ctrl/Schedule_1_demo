using System;

namespace ScheduleOne.Persistence.Datas
{
	[Serializable]
	public class MoneyData : SaveData
	{
		public float OnlineBalance;

		public float Networth;

		public MoneyData(float onlineBalance, float netWorth)
		{
			OnlineBalance = onlineBalance;
			Networth = netWorth;
		}
	}
}
