using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Law;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class PoliceStation : NPCEnterableBuilding
	{
		public enum EDispatchType
		{
			Auto = 0,
			UseVehicle = 1,
			OnFoot = 2
		}

		public static List<PoliceStation> PoliceStations = new List<PoliceStation>();

		public int VehicleLimit = 5;

		[Header("References")]
		public Transform SpawnPoint;

		public Transform[] VehicleSpawnPoints;

		public Transform[] PossessedVehicleSpawnPoints;

		[Header("Prefabs")]
		public LandVehicle[] PoliceVehiclePrefabs;

		public List<PoliceOfficer> OfficerPool = new List<PoliceOfficer>();

		[SerializeField]
		private List<LandVehicle> deployedVehicles = new List<LandVehicle>();

		public float TimeSinceLastDispatch { get; private set; }

		private int deployedVehicleCount => deployedVehicles.Where((LandVehicle v) => v != null).Count();

		protected override void Awake()
		{
			base.Awake();
			if (!PoliceStations.Contains(this))
			{
				PoliceStations.Add(this);
			}
			InvokeRepeating("CleanVehicleList", 0f, 5f);
		}

		private void OnDestroy()
		{
			if (PoliceStations.Contains(this))
			{
				PoliceStations.Remove(this);
			}
		}

		private void Update()
		{
			TimeSinceLastDispatch += Time.deltaTime;
		}

		private void CleanVehicleList()
		{
			for (int i = 0; i < deployedVehicles.Count; i++)
			{
				if (deployedVehicles[i] == null)
				{
					deployedVehicles.RemoveAt(i);
					i--;
				}
			}
		}

		public void Dispatch(int requestedOfficerCount, Player targetPlayer, EDispatchType type = EDispatchType.Auto, bool beginAsSighted = false)
		{
			if (!InstanceFinder.IsServer)
			{
				Console.LogWarning("Attempted to dispatch officers from a client, this is not allowed.");
			}
			else
			{
				if (requestedOfficerCount <= 0)
				{
					return;
				}
				if (requestedOfficerCount > 4)
				{
					Console.LogWarning("Attempted to dispatch more than 4 officers, this is not allowed.");
					return;
				}
				List<PoliceOfficer> list = new List<PoliceOfficer>();
				for (int i = 0; i < requestedOfficerCount; i++)
				{
					if (OfficerPool.Count > 0)
					{
						list.Add(PullOfficer());
					}
				}
				if (list.Count == 0)
				{
					Console.LogWarning("Attempted to dispatch officers, but there are no officers in the pool.");
					return;
				}
				bool flag = false;
				switch (type)
				{
				case EDispatchType.Auto:
					flag = Vector3.Distance(targetPlayer.CrimeData.LastKnownPosition, SpawnPoint.position) > LawManager.DISPATCH_VEHICLE_USE_THRESHOLD || targetPlayer.CurrentVehicle != null;
					break;
				case EDispatchType.UseVehicle:
					flag = true;
					break;
				}
				if (flag && deployedVehicleCount < VehicleLimit)
				{
					LandVehicle landVehicle = CreateVehicle();
					list[0].AssignedVehicle = landVehicle;
					list[0].EnterVehicle(null, landVehicle);
					for (int j = 0; j < list.Count; j++)
					{
						list[j].BeginVehiclePursuit_Networked(targetPlayer.NetworkObject, landVehicle.NetworkObject, beginAsSighted);
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					list[k].BeginFootPursuit_Networked(targetPlayer.NetworkObject);
				}
				TimeSinceLastDispatch = 0f;
			}
		}

		public PoliceOfficer PullOfficer()
		{
			if (OfficerPool.Count == 0)
			{
				Console.LogWarning("Attempted to pull an officer from the station, but there are no officers in the pool.");
				return null;
			}
			PoliceOfficer policeOfficer = OfficerPool[UnityEngine.Random.Range(0, OfficerPool.Count)];
			OfficerPool.Remove(policeOfficer);
			policeOfficer.Activate();
			return policeOfficer;
		}

		public LandVehicle CreateVehicle()
		{
			Transform target = VehicleSpawnPoints[0];
			for (int i = 0; i < VehicleSpawnPoints.Length; i++)
			{
				if (IsSpawnPointAvailable(VehicleSpawnPoints[i]))
				{
					target = VehicleSpawnPoints[i];
					break;
				}
			}
			LandVehicle landVehicle = PoliceVehiclePrefabs[UnityEngine.Random.Range(0, PoliceVehiclePrefabs.Length)];
			Tuple<Vector3, Quaternion> alignmentTransform = landVehicle.GetAlignmentTransform(target, EParkingAlignment.RearToKerb);
			LandVehicle landVehicle2 = NetworkSingleton<VehicleManager>.Instance.SpawnVehicle(landVehicle.VehicleCode, alignmentTransform.Item1, alignmentTransform.Item2, playerOwned: false);
			deployedVehicles.Add(landVehicle2);
			return landVehicle2;
			static bool IsSpawnPointAvailable(Transform spawnPoint)
			{
				Collider[] array = Physics.OverlapSphere(spawnPoint.position, 2f, 1 << LayerMask.NameToLayer("Vehicle"));
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].GetComponentInParent<LandVehicle>() != null)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override void NPCEnteredBuilding(NPC npc)
		{
			base.NPCEnteredBuilding(npc);
			if (npc is PoliceOfficer && !OfficerPool.Contains(npc as PoliceOfficer))
			{
				OfficerPool.Add(npc as PoliceOfficer);
			}
		}

		public override void NPCExitedBuilding(NPC npc)
		{
			base.NPCExitedBuilding(npc);
			if (npc is PoliceOfficer)
			{
				OfficerPool.Remove(npc as PoliceOfficer);
			}
		}

		public static PoliceStation GetClosestPoliceStation(Vector3 point)
		{
			return PoliceStations[0];
		}
	}
}
