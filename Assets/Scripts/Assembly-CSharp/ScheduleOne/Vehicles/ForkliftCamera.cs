using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class ForkliftCamera : VehicleCamera
	{
		[Header("Forklift References")]
		[SerializeField]
		protected Transform forkCamPos;

		[SerializeField]
		protected Light guidanceLight;

		protected bool forkliftCamActive;

		protected override void Update()
		{
			base.Update();
			forkliftCamActive = false;
			if (vehicle.localPlayerIsDriver && Input.GetKey(KeyCode.LeftShift))
			{
				forkliftCamActive = true;
			}
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
			guidanceLight.enabled = false;
			if (vehicle.localPlayerIsDriver && forkliftCamActive)
			{
				PlayerSingleton<PlayerCamera>.Instance.transform.position = forkCamPos.position;
				PlayerSingleton<PlayerCamera>.Instance.transform.rotation = forkCamPos.rotation;
				guidanceLight.enabled = true;
			}
		}
	}
}
