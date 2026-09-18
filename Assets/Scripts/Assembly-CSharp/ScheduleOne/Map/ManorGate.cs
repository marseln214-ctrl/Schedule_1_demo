using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class ManorGate : Gate
	{
		[Header("References")]
		public InteractableObject IntercomInt;

		public Light IntercomLight;

		public VehicleDetector ExteriorVehicleDetector;

		public PlayerDetector ExteriorPlayerDetector;

		public VehicleDetector InteriorVehicleDetector;

		public PlayerDetector InteriorPlayerDetector;

		private bool intercomActive;

		protected virtual void Start()
		{
			SetIntercomActive(active: false);
			SetEnterable(enterable: false);
			InvokeRepeating("UpdateDetection", 0f, 0.25f);
		}

		private void UpdateDetection()
		{
			bool flag = false;
			if (ExteriorVehicleDetector.AreAnyVehiclesOccupied())
			{
				flag = true;
			}
			if (ExteriorPlayerDetector.DetectedPlayers.Count > 0)
			{
				flag = true;
			}
			if (InteriorVehicleDetector.AreAnyVehiclesOccupied())
			{
				flag = true;
			}
			if (InteriorPlayerDetector.DetectedPlayers.Count > 0)
			{
				flag = true;
			}
			if (flag != base.IsOpen)
			{
				if (flag)
				{
					Open();
				}
				else
				{
					Close();
				}
			}
		}

		public void IntercomBuzzed()
		{
			SetIntercomActive(active: false);
		}

		public void SetEnterable(bool enterable)
		{
			ExteriorPlayerDetector.SetIgnoreNewCollisions(!enterable);
			ExteriorVehicleDetector.SetIgnoreNewCollisions(!enterable);
			ExteriorVehicleDetector.vehicles.Clear();
		}

		[Button]
		public void ActivateIntercom()
		{
			SetIntercomActive(active: true);
		}

		public void SetIntercomActive(bool active)
		{
			intercomActive = active;
			UpdateIntercom();
		}

		private void UpdateIntercom()
		{
			IntercomInt.SetInteractableState((!intercomActive) ? InteractableObject.EInteractableState.Disabled : InteractableObject.EInteractableState.Default);
			IntercomLight.enabled = intercomActive;
		}
	}
}
