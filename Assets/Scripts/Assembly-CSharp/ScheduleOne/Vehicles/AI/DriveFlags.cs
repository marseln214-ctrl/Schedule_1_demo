using System;

namespace ScheduleOne.Vehicles.AI
{
	[Serializable]
	public class DriveFlags
	{
		public enum EObstacleMode
		{
			Default = 0,
			IgnoreAll = 1,
			IgnoreOnlySquishy = 2
		}

		public bool OverrideSpeed;

		public float OverriddenSpeed = 50f;

		public float OverriddenReverseSpeed = 10f;

		public float SpeedLimitMultiplier = 1f;

		public bool IgnoreTrafficLights;

		public bool UseRoads = true;

		public bool StuckDetection = true;

		public EObstacleMode ObstacleMode;

		public bool AutoBrakeAtDestination = true;

		public bool TurnBasedSpeedReduction = true;

		public void ResetFlags()
		{
			OverrideSpeed = false;
			OverriddenSpeed = 50f;
			OverriddenReverseSpeed = 10f;
			SpeedLimitMultiplier = 1f;
			IgnoreTrafficLights = false;
			UseRoads = true;
			StuckDetection = true;
			ObstacleMode = EObstacleMode.Default;
			AutoBrakeAtDestination = true;
			TurnBasedSpeedReduction = true;
		}
	}
}
