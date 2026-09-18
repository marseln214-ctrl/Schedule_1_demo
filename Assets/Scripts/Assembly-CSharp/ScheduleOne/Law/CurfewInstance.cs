using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Law
{
	[Serializable]
	public class CurfewInstance
	{
		public static CurfewInstance ActiveInstance;

		[Range(1f, 10f)]
		public int IntensityRequirement = 5;

		[HideInInspector]
		public bool shouldDisable;

		public bool Enabled { get; protected set; }

		public void Evaluate(bool ignoreSleepReq = false)
		{
			if (!Enabled && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement && (NetworkSingleton<TimeManager>.Instance.SleepInProgress || ignoreSleepReq))
			{
				Enable();
			}
		}

		private void MinPass()
		{
			if (Enabled)
			{
				if (Singleton<LawController>.Instance.LE_Intensity < IntensityRequirement)
				{
					shouldDisable = true;
				}
				if (shouldDisable && NetworkSingleton<TimeManager>.Instance.SleepInProgress)
				{
					Disable();
				}
			}
		}

		public void Enable()
		{
			ActiveInstance = this;
			Enabled = true;
			shouldDisable = false;
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
			NetworkSingleton<CurfewManager>.Instance.Enable(null);
		}

		public void Disable()
		{
			Enabled = false;
			shouldDisable = false;
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			if (ActiveInstance == this)
			{
				NetworkSingleton<CurfewManager>.Instance.Disable();
			}
		}
	}
}
