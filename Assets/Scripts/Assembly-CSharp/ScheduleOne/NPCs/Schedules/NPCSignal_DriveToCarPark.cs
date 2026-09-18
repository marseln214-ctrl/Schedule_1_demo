using System.Collections;
using FishNet;
using ScheduleOne.Map;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.AI;
using UnityEngine;

namespace ScheduleOne.NPCs.Schedules
{
	public class NPCSignal_DriveToCarPark : NPCSignal
	{
		public ParkingLot ParkingLot;

		public LandVehicle Vehicle;

		[Header("Parking Settings")]
		public bool OverrideParkingType;

		public EParkingAlignment ParkingType;

		private bool isAtDestination;

		private float timeInVehicle;

		private float timeAtDestination;

		private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted;

		public new string ActionName => "Drive to car park";

		public override string GetName()
		{
			if (ParkingLot == null)
			{
				return ActionName + " (No Parking Lot)";
			}
			return ActionName + " (" + ParkingLot.gameObject.name + ")";
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			priority = 12;
		}

		public override void Started()
		{
			base.Started();
			isAtDestination = false;
			CheckValidForStart();
		}

		public override void End()
		{
			base.End();
			if (npc.CurrentVehicle != null)
			{
				npc.ExitVehicle();
			}
		}

		public override void LateStarted()
		{
			base.LateStarted();
			isAtDestination = false;
			CheckValidForStart();
		}

		private void CheckValidForStart()
		{
			if (Vehicle.CurrentParkingLot == ParkingLot)
			{
				End();
			}
		}

		public override void Interrupt()
		{
			base.Interrupt();
			Park();
			if (InstanceFinder.IsServer)
			{
				if (npc.IsInVehicle)
				{
					Vehicle.Agent.StopNavigating();
					npc.ExitVehicle();
				}
				else
				{
					npc.Movement.Stop();
				}
			}
		}

		public override void Resume()
		{
			base.Resume();
			isAtDestination = false;
			CheckValidForStart();
		}

		public override void Skipped()
		{
			base.Skipped();
			Park();
		}

		public override void ResumeFailed()
		{
			base.ResumeFailed();
			Park();
		}

		public override void JumpTo()
		{
			base.JumpTo();
			isAtDestination = false;
		}

		public override void ActiveUpdate()
		{
			base.ActiveUpdate();
			if (npc.IsInVehicle)
			{
				timeInVehicle += Time.deltaTime;
			}
			else
			{
				timeInVehicle = 0f;
			}
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (npc.IsInVehicle && npc.CurrentVehicle.CurrentParkingLot == ParkingLot)
			{
				timeAtDestination += Time.deltaTime;
				if (timeAtDestination > 1f)
				{
					End();
				}
			}
			else
			{
				timeAtDestination = 0f;
			}
			if (isAtDestination)
			{
				return;
			}
			if (npc.IsInVehicle)
			{
				if (Vehicle.isParked)
				{
					if (timeInVehicle > 1f)
					{
						Vehicle.ExitPark_Networked(null, Vehicle.CurrentParkingLot.UseExitPoint);
					}
				}
				else if (!Vehicle.Agent.AutoDriving)
				{
					Vehicle.Agent.Navigate(ParkingLot.EntryPoint.position, null, DriveCallback);
				}
			}
			else if ((!npc.Movement.IsMoving || Vector3.Distance(npc.Movement.CurrentDestination, GetWalkDestination()) > 1f) && npc.Movement.CanMove())
			{
				if (npc.Movement.CanGetTo(GetWalkDestination(), 2f))
				{
					SetDestination(GetWalkDestination());
					return;
				}
				npc.EnterVehicle(null, Vehicle);
				Console.LogWarning("NPC " + npc.name + " was unable to reach vehicle " + Vehicle.name + " and was teleported to it.");
				Debug.DrawLine(npc.transform.position, GetWalkDestination(), Color.red, 10f);
			}
		}

		protected override void WalkCallback(NPCMovement.WalkResult result)
		{
			base.WalkCallback(result);
			if (base.IsActive && InstanceFinder.IsServer && (result == NPCMovement.WalkResult.Success || result == NPCMovement.WalkResult.Partial))
			{
				npc.EnterVehicle(null, Vehicle);
			}
		}

		private Vector3 GetWalkDestination()
		{
			if (!Vehicle.IsVisible && Vehicle.CurrentParkingLot != null && Vehicle.CurrentParkingLot.HiddenVehicleAccessPoint != null)
			{
				return Vehicle.CurrentParkingLot.HiddenVehicleAccessPoint.position;
			}
			return Vehicle.driverEntryPoint.position;
		}

		private void DriveCallback(VehicleAgent.ENavigationResult result)
		{
			if (base.IsActive)
			{
				isAtDestination = true;
				if (InstanceFinder.IsServer)
				{
					Park();
					StartCoroutine(Wait());
				}
			}
			IEnumerator Wait()
			{
				yield return new WaitForSeconds(1f);
				End();
			}
		}

		private void Park()
		{
			if (InstanceFinder.IsServer)
			{
				int randomFreeSpotIndex = ParkingLot.GetRandomFreeSpotIndex();
				EParkingAlignment alignment = EParkingAlignment.FrontToKerb;
				if (randomFreeSpotIndex != -1)
				{
					alignment = (OverrideParkingType ? ParkingType : ParkingLot.ParkingSpots[randomFreeSpotIndex].Alignment);
				}
				Vehicle.Park(null, new ParkData
				{
					lotGUID = ParkingLot.GUID,
					alignment = alignment,
					spotIndex = randomFreeSpotIndex
				}, network: true);
			}
		}

		private EParkingAlignment GetParkingType()
		{
			if (OverrideParkingType)
			{
				return ParkingType;
			}
			return ParkingLot.GetRandomFreeSpot().Alignment;
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
			}
		}

		public override void NetworkInitialize__Late()
		{
			if (!NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize__LateScheduleOne_002ENPCs_002ESchedules_002ENPCSignal_DriveToCarParkAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize__Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize__Late();
		}

		public override void Awake()
		{
			NetworkInitialize___Early();
			base.Awake();
			NetworkInitialize__Late();
		}
	}
}
