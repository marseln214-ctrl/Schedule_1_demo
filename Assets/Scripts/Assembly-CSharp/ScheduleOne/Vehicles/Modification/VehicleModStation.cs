using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Vehicles.Modification
{
	public class VehicleModStation : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		protected Transform vehiclePosition;

		[SerializeField]
		protected OrbitCamera orbitCam;

		public LandVehicle currentVehicle { get; protected set; }

		public bool isOpen => currentVehicle != null;

		public void Open(LandVehicle vehicle)
		{
			orbitCam.Enable();
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.canMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
			currentVehicle = vehicle;
			vehicle.transform.rotation = vehiclePosition.rotation;
			vehicle.transform.position = vehiclePosition.position;
			vehicle.transform.position -= vehicle.transform.InverseTransformPoint(vehicle.boundingBox.transform.position);
			vehicle.transform.position += Vector3.up * vehicle.boundingBox.transform.localScale.y * 0.5f;
			Singleton<VehicleModMenu>.Instance.Open(currentVehicle);
		}

		protected virtual void Update()
		{
			if (isOpen && GameInput.GetButtonDown(GameInput.ButtonCode.Escape))
			{
				Close();
			}
		}

		public void Close()
		{
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
			orbitCam.Disable();
			Singleton<VehicleModMenu>.Instance.Close();
			currentVehicle = null;
		}
	}
}
