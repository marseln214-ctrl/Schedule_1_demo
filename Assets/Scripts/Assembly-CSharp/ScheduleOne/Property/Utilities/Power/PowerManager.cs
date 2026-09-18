using System;
using System.Collections.Generic;
using ScheduleOne.Construction;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Property.Utilities.Power
{
	public class PowerManager : Singleton<PowerManager>
	{
		[Header("Prefabs")]
		public GameObject powerLineSegmentPrefab;

		public static float pricePerkWh = 0.25f;

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

		public void ConsumePower(float kwh)
		{
			usageThisMinute += kwh;
		}

		public PowerLine CreatePowerLine(PowerNode nodeA, PowerNode nodeB, Property p)
		{
			if (!PowerLine.CanNodesBeConnected(nodeA, nodeB))
			{
				Console.LogWarning("Nodes can't be connected!");
				return null;
			}
			PowerLine component = Singleton<ConstructionManager>.Instance.CreateConstructable("Utilities/PowerLine/PowerLine").GetComponent<PowerLine>();
			component.transform.SetParent(p.Container.transform);
			component.InitializePowerLine(nodeA, nodeB);
			return component;
		}
	}
}
