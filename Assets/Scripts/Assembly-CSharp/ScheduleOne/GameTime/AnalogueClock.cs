using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.GameTime
{
	public class AnalogueClock : MonoBehaviour
	{
		public Transform MinHand;

		public Transform HourHand;

		public void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		public void MinPass()
		{
			int minSumFrom24HourTime = TimeManager.GetMinSumFrom24HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			float num = minSumFrom24HourTime % 60;
			float num2 = minSumFrom24HourTime / 60;
			float num3 = num / 60f * 360f;
			float z = num2 / 12f * 360f + num3 / 12f;
			MinHand.localEulerAngles = new Vector3(0f, 0f, num3);
			HourHand.localEulerAngles = new Vector3(0f, 0f, z);
		}
	}
}
