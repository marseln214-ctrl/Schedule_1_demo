using ScheduleOne.DevUtilities;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class Dealership : MonoBehaviour
	{
		public Transform[] SpawnPoints;

		public void SpawnVehicle(string vehicleCode)
		{
			Transform transform = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
			NetworkSingleton<VehicleManager>.Instance.SpawnVehicle(vehicleCode, transform.position, transform.rotation, playerOwned: true);
		}
	}
}
