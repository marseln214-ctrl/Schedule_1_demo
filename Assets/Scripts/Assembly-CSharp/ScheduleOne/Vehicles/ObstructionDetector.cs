using System.Collections.Generic;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	[RequireComponent(typeof(Rigidbody))]
	public class ObstructionDetector : MonoBehaviour
	{
		private LandVehicle vehicle;

		public List<LandVehicle> vehicles = new List<LandVehicle>();

		public List<NPC> npcs = new List<NPC>();

		public List<PlayerMovement> players = new List<PlayerMovement>();

		public List<VehicleObstacle> vehicleObstacles = new List<VehicleObstacle>();

		public float closestObstructionDistance;

		public float range;

		protected virtual void Awake()
		{
			vehicle = base.gameObject.GetComponentInParent<LandVehicle>();
			range = base.transform.Find("Collider").transform.localScale.z;
		}

		protected virtual void FixedUpdate()
		{
			closestObstructionDistance = float.MaxValue;
			for (int i = 0; i < vehicles.Count; i++)
			{
				if (Vector3.Distance(base.transform.position, vehicles[i].transform.position) < closestObstructionDistance)
				{
					closestObstructionDistance = Vector3.Distance(base.transform.position, vehicles[i].transform.position);
				}
			}
			for (int j = 0; j < npcs.Count; j++)
			{
				if (Vector3.Distance(base.transform.position, npcs[j].transform.position) < closestObstructionDistance)
				{
					closestObstructionDistance = Vector3.Distance(base.transform.position, npcs[j].transform.position);
				}
			}
			for (int k = 0; k < players.Count; k++)
			{
				if (Vector3.Distance(base.transform.position, players[k].transform.position) < closestObstructionDistance)
				{
					closestObstructionDistance = Vector3.Distance(base.transform.position, players[k].transform.position);
				}
			}
			for (int l = 0; l < vehicleObstacles.Count; l++)
			{
				if (Vector3.Distance(base.transform.position, vehicleObstacles[l].transform.position) < closestObstructionDistance)
				{
					closestObstructionDistance = Vector3.Distance(base.transform.position, vehicleObstacles[l].transform.position);
				}
			}
			vehicles.Clear();
			npcs.Clear();
			players.Clear();
			vehicleObstacles.Clear();
			_ = closestObstructionDistance;
			_ = float.MaxValue;
		}

		private void OnTriggerStay(Collider other)
		{
			LandVehicle componentInParent = other.GetComponentInParent<LandVehicle>();
			NPC componentInParent2 = other.GetComponentInParent<NPC>();
			PlayerMovement componentInParent3 = other.GetComponentInParent<PlayerMovement>();
			VehicleObstacle componentInParent4 = other.GetComponentInParent<VehicleObstacle>();
			if (componentInParent != null && componentInParent != vehicle && !vehicles.Contains(componentInParent))
			{
				vehicles.Add(componentInParent);
			}
			if (componentInParent2 != null && !npcs.Contains(componentInParent2))
			{
				npcs.Add(componentInParent2);
			}
			if (componentInParent3 != null && !players.Contains(componentInParent3))
			{
				players.Add(componentInParent3);
			}
			if (componentInParent4 != null && (componentInParent4.twoSided || Vector3.Angle(-componentInParent4.transform.forward, base.transform.forward) < 90f) && !vehicleObstacles.Contains(componentInParent4))
			{
				vehicleObstacles.Add(componentInParent4);
			}
		}
	}
}
