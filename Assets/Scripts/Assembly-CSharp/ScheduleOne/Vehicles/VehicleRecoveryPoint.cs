using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class VehicleRecoveryPoint : MonoBehaviour
	{
		public static List<VehicleRecoveryPoint> recoveryPoints = new List<VehicleRecoveryPoint>();

		protected virtual void Awake()
		{
			recoveryPoints.Add(this);
		}

		public static VehicleRecoveryPoint GetClosestRecoveryPoint(Vector3 pos)
		{
			VehicleRecoveryPoint vehicleRecoveryPoint = null;
			for (int i = 0; i < recoveryPoints.Count; i++)
			{
				if (vehicleRecoveryPoint == null || Vector3.Distance(recoveryPoints[i].transform.position, pos) < Vector3.Distance(vehicleRecoveryPoint.transform.position, pos))
				{
					vehicleRecoveryPoint = recoveryPoints[i];
				}
			}
			return vehicleRecoveryPoint;
		}
	}
}
