using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law
{
	[Serializable]
	public class CheckpointInstance
	{
		public const float MIN_ACTIVATION_DISTANCE = 50f;

		public CheckpointManager.ECheckpointLocation Location;

		public int Members = 2;

		public int StartTime = 800;

		public int EndTime = 2000;

		[Range(1f, 10f)]
		public int IntensityRequirement = 5;

		public bool OnlyIfCurfewEnabled;

		private RoadCheckpoint checkPoint;

		public RoadCheckpoint activeCheckpoint { get; protected set; }

		public void Evaluate()
		{
			if (checkPoint == null)
			{
				checkPoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(Location);
			}
			if (!(activeCheckpoint != null) && checkPoint.ActivationState != RoadCheckpoint.ECheckpointState.Enabled && Singleton<LawController>.Instance.LE_Intensity >= IntensityRequirement && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && (!OnlyIfCurfewEnabled || NetworkSingleton<CurfewManager>.Instance.IsEnabled) && DistanceRequirementsMet())
			{
				EnableCheckpoint();
			}
		}

		public void EnableCheckpoint()
		{
			if (activeCheckpoint != null)
			{
				Console.LogWarning("StartPatrol called but patrol is already active.");
			}
			else if (PoliceStation.GetClosestPoliceStation(Vector3.zero).OfficerPool.Count != 0)
			{
				activeCheckpoint = NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(Location);
				NetworkSingleton<CheckpointManager>.Instance.SetCheckpointEnabled(Location, enabled: true, Members);
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
			}
		}

		private bool DistanceRequirementsMet()
		{
			float distance;
			Player closestPlayer = Player.GetClosestPlayer(NetworkSingleton<CheckpointManager>.Instance.GetCheckpoint(Location).transform.position, out distance);
			if (NetworkSingleton<TimeManager>.Instance.SleepInProgress || closestPlayer == null || distance >= 50f)
			{
				return true;
			}
			return false;
		}

		private void MinPass()
		{
			if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(StartTime, EndTime) && DistanceRequirementsMet())
			{
				DisableCheckpoint();
			}
		}

		public void DisableCheckpoint()
		{
			if (!(activeCheckpoint == null))
			{
				NetworkSingleton<CheckpointManager>.Instance.SetCheckpointEnabled(Location, enabled: false, Members);
				activeCheckpoint = null;
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}
	}
}
