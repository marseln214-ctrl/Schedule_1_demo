using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Doors
{
	public class SensorRollerDoors : RollerDoor
	{
		[Header("References")]
		public VehicleDetector Detector;

		public VehicleDetector ClipDetector;

		[Header("Settings")]
		public bool DetectPlayerOccupiedVehiclesOnly = true;

		protected virtual void Update()
		{
			if (!CanOpen())
			{
				if (base.IsOpen)
				{
					Close();
				}
			}
			else if (Detector.vehicles.Count > 0)
			{
				if (!DetectPlayerOccupiedVehiclesOnly || ClipDetector.vehicles.Count > 0)
				{
					Open();
					return;
				}
				for (int i = 0; i < Detector.vehicles.Count; i++)
				{
					if (Detector.vehicles[i].DriverPlayer != null)
					{
						Open();
						return;
					}
				}
				Close();
			}
			else
			{
				Close();
			}
		}
	}
}
