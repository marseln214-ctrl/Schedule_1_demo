using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	[RequireComponent(typeof(Rigidbody))]
	public class VehicleDetector : MonoBehaviour
	{
		public List<LandVehicle> vehicles = new List<LandVehicle>();

		public LandVehicle closestVehicle;

		private bool ignoreExit;

		public bool IgnoreNewDetections { get; protected set; }

		private void Awake()
		{
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = base.gameObject.AddComponent<Rigidbody>();
			}
			rigidbody.isKinematic = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!IgnoreNewDetections)
			{
				LandVehicle componentInParent = other.GetComponentInParent<LandVehicle>();
				if (componentInParent != null && other == componentInParent.boundingBox && !vehicles.Contains(componentInParent))
				{
					vehicles.Add(componentInParent);
					SortVehicles();
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (!ignoreExit)
			{
				LandVehicle componentInParent = other.GetComponentInParent<LandVehicle>();
				if (componentInParent != null && other == componentInParent.boundingBox && vehicles.Contains(componentInParent))
				{
					vehicles.Remove(componentInParent);
					SortVehicles();
				}
			}
		}

		private void SortVehicles()
		{
			if (vehicles.Count > 1)
			{
				vehicles.OrderBy((LandVehicle x) => Vector3.Distance(base.transform.position, x.transform.position));
			}
			if (vehicles.Count > 0)
			{
				closestVehicle = vehicles[0];
			}
			else
			{
				closestVehicle = null;
			}
		}

		public void SetIgnoreNewCollisions(bool ignore)
		{
			IgnoreNewDetections = ignore;
			if (ignore)
			{
				return;
			}
			ignoreExit = true;
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].isTrigger)
				{
					componentsInChildren[i].enabled = false;
					componentsInChildren[i].enabled = true;
				}
			}
			ignoreExit = false;
		}

		public bool AreAnyVehiclesOccupied()
		{
			for (int i = 0; i < vehicles.Count; i++)
			{
				if (vehicles[i].isOccupied)
				{
					return true;
				}
			}
			return false;
		}
	}
}
