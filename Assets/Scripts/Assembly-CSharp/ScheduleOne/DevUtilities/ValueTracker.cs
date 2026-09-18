using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class ValueTracker
	{
		public class Value
		{
			public float val;

			public float time;

			public Value(float val, float time)
			{
				this.val = val;
				this.time = time;
			}
		}

		private float historyDuration;

		private List<Value> valueHistory = new List<Value>();

		public ValueTracker(float HistoryDuration)
		{
			historyDuration = HistoryDuration;
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onUpdate = (Action)Delegate.Combine(instance.onUpdate, new Action(Update));
		}

		public void Destroy()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onUpdate = (Action)Delegate.Remove(instance.onUpdate, new Action(Update));
		}

		public void Update()
		{
			int num = 0;
			while (num < valueHistory.Count && Time.timeSinceLevelLoad - valueHistory[num].time > historyDuration)
			{
				valueHistory.RemoveAt(num);
				num--;
				num++;
			}
		}

		public void SubmitValue(float value)
		{
			valueHistory.Add(new Value(value, Time.timeSinceLevelLoad));
		}

		public float RecordedHistoryLength()
		{
			if (valueHistory.Count == 0)
			{
				return 0f;
			}
			return Time.timeSinceLevelLoad - valueHistory[0].time;
		}

		public float GetLowestValue()
		{
			return valueHistory.OrderBy((Value x) => x.val).FirstOrDefault()?.val ?? 0f;
		}

		public float GetAverageValue()
		{
			if (valueHistory.Count == 0)
			{
				return 0f;
			}
			float num = 0f;
			foreach (Value item in valueHistory)
			{
				num += item.val;
			}
			return num / (float)valueHistory.Count;
		}
	}
}
