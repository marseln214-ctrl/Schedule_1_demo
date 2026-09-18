using UnityEngine;

namespace ScheduleOne.Vehicles
{
	public class VehicleFX : MonoBehaviour
	{
		public ParticleSystem[] exhaustFX;

		public virtual void OnVehicleStart()
		{
			ParticleSystem[] array = exhaustFX;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}

		public virtual void OnVehicleStop()
		{
			ParticleSystem[] array = exhaustFX;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
		}
	}
}
