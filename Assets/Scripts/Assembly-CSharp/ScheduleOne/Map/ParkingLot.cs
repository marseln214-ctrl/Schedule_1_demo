using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class ParkingLot : MonoBehaviour, IGUIDRegisterable
	{
		[SerializeField]
		protected string BakedGUID = string.Empty;

		[Header("READONLY")]
		public List<ParkingSpot> ParkingSpots = new List<ParkingSpot>();

		[Header("Entry")]
		public Transform EntryPoint;

		public Transform HiddenVehicleAccessPoint;

		[Header("Exit")]
		public bool UseExitPoint;

		public EParkingAlignment ExitAlignment = EParkingAlignment.RearToKerb;

		public Transform ExitPoint;

		public VehicleDetector ExitPointVehicleDetector;

		public Guid GUID { get; protected set; }

		private void Awake()
		{
			if (ExitPoint != null && ExitPointVehicleDetector == null)
			{
				Console.LogWarning("ExitPoint specified but no ExitPointVehicleDetector!");
			}
			if (!GUIDManager.IsGUIDValid(BakedGUID))
			{
				Console.LogError(base.gameObject.name + "'s baked GUID is not valid! Bad.");
			}
			GUID = new Guid(BakedGUID);
			GUIDManager.RegisterObject(this);
		}

		public void SetGUID(Guid guid)
		{
			GUID = guid;
			GUIDManager.RegisterObject(this);
		}

		public ParkingSpot GetRandomFreeSpot()
		{
			List<ParkingSpot> freeParkingSpots = GetFreeParkingSpots();
			if (freeParkingSpots.Count == 0)
			{
				Console.Log("No free parking spots in " + base.gameObject.name + "!");
				return null;
			}
			return freeParkingSpots[UnityEngine.Random.Range(0, freeParkingSpots.Count)];
		}

		public int GetRandomFreeSpotIndex()
		{
			List<ParkingSpot> freeParkingSpots = GetFreeParkingSpots();
			if (freeParkingSpots.Count == 0)
			{
				return -1;
			}
			return ParkingSpots.IndexOf(freeParkingSpots[UnityEngine.Random.Range(0, freeParkingSpots.Count)]);
		}

		public List<ParkingSpot> GetFreeParkingSpots()
		{
			if (ParkingSpots == null || ParkingSpots.Count == 0)
			{
				return new List<ParkingSpot>();
			}
			return ParkingSpots.Where((ParkingSpot x) => x != null && x.OccupantVehicle == null).ToList();
		}
	}
}
