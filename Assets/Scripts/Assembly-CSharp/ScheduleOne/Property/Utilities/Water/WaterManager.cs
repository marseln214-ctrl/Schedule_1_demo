using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Property.Utilities.Water
{
	public class WaterManager : Singleton<WaterManager>
	{
		[Header("Prefabs")]
		[SerializeField]
		protected GameObject waterPipePrefab;

		public static float pricePerL = 0.1f;

		private Dictionary<int, float> usageAtTime = new Dictionary<int, float>();

		private float usageThisMinute;

		protected override void Start()
		{
			base.Start();
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onMinutePass = (Action)Delegate.Combine(timeManager.onMinutePass, new Action(MinPass));
			TimeManager timeManager2 = NetworkSingleton<TimeManager>.Instance;
			timeManager2.onDayPass = (Action)Delegate.Combine(timeManager2.onDayPass, new Action(DayPass));
		}

		private void MinPass()
		{
			usageThisMinute = 0f;
		}

		private void DayPass()
		{
			usageAtTime.Clear();
		}

		public float GetTotalUsage()
		{
			float num = 0f;
			foreach (int key in usageAtTime.Keys)
			{
				num += usageAtTime[key];
			}
			return num;
		}

		public void ConsumeWater(float litres)
		{
			usageThisMinute += litres;
		}
	}
}
