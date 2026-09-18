using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(BoxCollider))]
	public class PlayerPusher : MonoBehaviour
	{
		private LandVehicle veh;

		[Header("Settings")]
		public float MinSpeedToPush = 3f;

		public float MaxPushSpeed = 20f;

		public float MinPushForce = 0.5f;

		public float MaxPushForce = 5f;

		private void Awake()
		{
			veh = GetComponentInParent<LandVehicle>();
			LayerUtility.SetLayerRecursively(base.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
		}

		private void OnTriggerStay(Collider other)
		{
			if (!(veh.speed_Kmh < MinSpeedToPush))
			{
				Player componentInParent = other.GetComponentInParent<Player>();
				if (componentInParent != null && componentInParent == Player.Local && componentInParent.CurrentVehicle == null)
				{
					Vector3 normalized = Vector3.Project((componentInParent.transform.position - base.transform.position).normalized, base.transform.right).normalized;
					float num = MinPushForce + Mathf.Clamp((veh.speed_Kmh - MinSpeedToPush) / MaxPushSpeed, 0f, 1f) * (MaxPushSpeed - MinPushForce);
					PlayerSingleton<PlayerMovement>.Instance.Controller.Move(normalized * num * Time.fixedDeltaTime);
				}
			}
		}
	}
}
