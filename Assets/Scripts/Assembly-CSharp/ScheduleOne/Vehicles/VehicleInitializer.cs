using FishNet.Object;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Vehicles
{
	[RequireComponent(typeof(LandVehicle))]
	public class VehicleInitializer : NetworkBehaviour
	{
		public ParkingLot InitialParkingLot;

		private bool NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleInitializerAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleInitializerAssembly_002DCSharp_002Edll_Excuted;

		public override void OnStartServer()
		{
			base.OnStartServer();
			if (InitialParkingLot != null && !GetComponent<LandVehicle>().isParked)
			{
				int randomFreeSpotIndex = InitialParkingLot.GetRandomFreeSpotIndex();
				if (randomFreeSpotIndex != -1)
				{
					_ = InitialParkingLot.ParkingSpots[randomFreeSpotIndex].Alignment;
				}
			}
		}

		public virtual void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleInitializerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002EVehicles_002EVehicleInitializerAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public virtual void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleInitializerAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002EVehicles_002EVehicleInitializerAssembly_002DCSharp_002Edll_Excuted = true;
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}
	}
}
