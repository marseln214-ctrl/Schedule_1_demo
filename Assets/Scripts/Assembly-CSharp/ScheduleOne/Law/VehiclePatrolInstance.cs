using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law
{
	[Serializable]
	public class VehiclePatrolInstance
	{
		public VehiclePatrolRoute Route;

		public int StartTime = 2000;

		[Range(1f, 10f)]
		public int IntensityRequirement = 5;

		public bool OnlyIfCurfewEnabled;

		private PoliceOfficer activeOfficer;

		private int latestStartTime;

		private bool startedThisCycle;

		private PoliceStation nearestStation => PoliceStation.GetClosestPoliceStation(Vector3.zero);

		public void Evaluate()
		{
			if (activeOfficer != null)
			{
				CheckEnd();
			}
			else
			{
				if (nearestStation.OfficerPool.Count == 0)
				{
					return;
				}
				latestStartTime = TimeManager.AddMinutesTo24HourTime(StartTime, 30);
				if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, latestStartTime) && (!OnlyIfCurfewEnabled || NetworkSingleton<CurfewManager>.Instance.IsEnabled))
				{
					if (!startedThisCycle && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement)
					{
						StartPatrol();
					}
				}
				else
				{
					startedThisCycle = false;
				}
			}
		}

		private void CheckEnd()
		{
			if (activeOfficer != null && !activeOfficer.VehiclePatrolBehaviour.Enabled)
			{
				activeOfficer = null;
			}
		}

		public void StartPatrol()
		{
			if (activeOfficer != null)
			{
				Console.LogWarning("StartPatrol called but patrol is already active.");
				return;
			}
			startedThisCycle = true;
			if (nearestStation.OfficerPool.Count != 0)
			{
				activeOfficer = Singleton<LawManager>.Instance.StartVehiclePatrol(Route);
			}
		}
	}
}
