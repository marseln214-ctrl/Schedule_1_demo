using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law
{
	[Serializable]
	public class SentryInstance
	{
		public SentryLocation Location;

		public int Members = 2;

		[Header("Timing")]
		public int StartTime = 2000;

		public int EndTime = 100;

		[Range(1f, 10f)]
		public int IntensityRequirement = 5;

		public bool OnlyIfCurfewEnabled;

		private List<PoliceOfficer> officers = new List<PoliceOfficer>();

		public void Evaluate()
		{
			if (Location.AssignedOfficers.Count <= 0 && officers.Count <= 0 && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && (!OnlyIfCurfewEnabled || NetworkSingleton<CurfewManager>.Instance.IsEnabled))
			{
				StartEntry();
			}
		}

		public void StartEntry()
		{
			if (Location.AssignedOfficers.Count > 0)
			{
				Console.LogWarning("StartEntry called but location already has active officers");
				return;
			}
			PoliceStation closestPoliceStation = PoliceStation.GetClosestPoliceStation(Location.transform.position);
			if (closestPoliceStation.OfficerPool.Count == 0)
			{
				return;
			}
			for (int i = 0; i < Members; i++)
			{
				PoliceOfficer policeOfficer = closestPoliceStation.PullOfficer();
				if (policeOfficer == null)
				{
					Console.LogWarning("Failed to pull officer from station");
					break;
				}
				policeOfficer.AssignToSentryLocation(Location);
				officers.Add(policeOfficer);
			}
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void MinPass()
		{
			if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime))
			{
				EndSentry();
			}
		}

		public void EndSentry()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			for (int i = 0; i < officers.Count; i++)
			{
				officers[i].UnassignFromSentryLocation();
			}
			officers.Clear();
		}
	}
}
