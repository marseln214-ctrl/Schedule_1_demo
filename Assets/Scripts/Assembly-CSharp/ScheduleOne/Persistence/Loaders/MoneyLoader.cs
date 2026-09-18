using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class MoneyLoader : Loader
	{
		public override void Load(string mainPath)
		{
			if (TryLoadFile(mainPath, out var contents))
			{
				MoneyData moneyData = null;
				try
				{
					moneyData = JsonUtility.FromJson<MoneyData>(contents);
				}
				catch (Exception ex)
				{
					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
				}
				if (moneyData != null)
				{
					NetworkSingleton<MoneyManager>.Instance.Load(moneyData);
				}
			}
		}
	}
}
