using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class ParkingSpot : MonoBehaviour
	{
		private ParkingLot ParentLot;

		public Transform AlignmentPoint;

		public EParkingAlignment Alignment;

		[SerializeField]
		private LandVehicle OccupantVehicle_Readonly;

		public LandVehicle OccupantVehicle { get; protected set; }

		private void Awake()
		{
			Init();
			if (ParentLot == null)
			{
				Debug.LogError("ParkingSpot has not parent ParkingLot!");
			}
		}

		private void Init()
		{
			if (ParentLot == null)
			{
				ParentLot = GetComponentInParent<ParkingLot>();
			}
			if (ParentLot == null)
			{
				Debug.LogError("ParkingSpot has not parent ParkingLot!");
			}
			ParentLot.ParkingSpots.Add(this);
		}

		public void SetOccupant(LandVehicle vehicle)
		{
			OccupantVehicle = vehicle;
			OccupantVehicle_Readonly = OccupantVehicle;
		}
	}
}
