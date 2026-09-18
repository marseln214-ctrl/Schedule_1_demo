using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class VehicleHumanoidCollider : MonoBehaviour
	{
		public LandVehicle vehicle;

		private void Start()
		{
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
		}

		private void OnCollisionStay(Collision collision)
		{
			Debug.Log("Collision Stay: " + collision.collider.gameObject.name);
		}
	}
}
