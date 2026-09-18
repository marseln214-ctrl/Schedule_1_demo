using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.NPCs.Behaviour;
using UnityEngine;

namespace ScheduleOne.Law
{
	[Serializable]
	public class PatrolInstance
	{
		public FootPatrolRoute Route;

		public int Members = 2;

		public int StartTime = 2000;

		public int EndTime = 100;

		[Range(1f, 10f)]
		public int IntensityRequirement = 5;

		public bool OnlyIfCurfewEnabled;

		public PatrolGroup ActiveGroup { get; protected set; }

		public void Evaluate()
		{
			if (ActiveGroup == null && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && (!OnlyIfCurfewEnabled || NetworkSingleton<CurfewManager>.Instance.IsEnabled))
			{
				StartPatrol();
			}
		}

		public void StartPatrol()
		{
			if (ActiveGroup != null)
			{
				Console.LogWarning("StartPatrol called but patrol is already active.");
			}
			else if (PoliceStation.GetClosestPoliceStation(Vector3.zero).OfficerPool.Count != 0)
			{
				ActiveGroup = Singleton<LawManager>.Instance.StartFootpatrol(Route, Members);
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
			}
		}

		private void MinPass()
		{
			if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime))
			{
				EndPatrol();
			}
		}

		public void EndPatrol()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			if (ActiveGroup != null)
			{
				ActiveGroup.DisbandGroup();
				ActiveGroup = null;
			}
		}
	}
}
