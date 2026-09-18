using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	public class Sensor : MonoBehaviour
	{
		public Collider obstruction;

		public float obstructionDistance;

		public const float checkRate = 0.33f;

		[Header("Settings")]
		[SerializeField]
		protected float minDetectionRange = 3f;

		[SerializeField]
		protected float maxDetectionRange = 12f;

		public float checkRadius = 1f;

		public LayerMask checkMask;

		private LandVehicle vehicle;

		[HideInInspector]
		public float calculatedDetectionRange;

		private RaycastHit hit;

		private List<RaycastHit> hits = new List<RaycastHit>();

		protected virtual void Start()
		{
			vehicle = GetComponentInParent<LandVehicle>();
			InvokeRepeating("Check", 0f, 0.33f);
		}

		public void Check()
		{
			if (NetworkSingleton<TimeManager>.Instance.SleepInProgress || vehicle.Agent.KinematicMode)
			{
				return;
			}
			if (vehicle != null)
			{
				calculatedDetectionRange = Mathf.Lerp(minDetectionRange, maxDetectionRange, Mathf.Clamp01(vehicle.speed_Kmh / vehicle.TopSpeed));
			}
			else
			{
				calculatedDetectionRange = maxDetectionRange;
			}
			Vector3 origin = base.transform.position - base.transform.forward * checkRadius;
			hits = Physics.SphereCastAll(origin, checkRadius, base.transform.forward, calculatedDetectionRange, checkMask, QueryTriggerInteraction.Collide).ToList();
			hits = hits.OrderBy((RaycastHit x) => Vector3.Distance(base.transform.position, x.point)).ToList();
			bool flag = false;
			for (int i = 0; i < hits.Count; i++)
			{
				if (vehicle != null && hits[i].collider.transform.IsChildOf(vehicle.transform))
				{
					continue;
				}
				VehicleObstacle componentInParent = hits[i].collider.transform.GetComponentInParent<VehicleObstacle>();
				LandVehicle componentInParent2 = hits[i].collider.transform.GetComponentInParent<LandVehicle>();
				NPC componentInParent3 = hits[i].collider.transform.GetComponentInParent<NPC>();
				Player componentInParent4 = hits[i].collider.transform.GetComponentInParent<Player>();
				if (componentInParent != null)
				{
					if (!componentInParent.twoSided && Vector3.Angle(-componentInParent.transform.forward, base.transform.forward) > 90f)
					{
						continue;
					}
				}
				else if (!(componentInParent2 != null) && !(componentInParent3 != null))
				{
					_ = componentInParent4 != null;
				}
				flag = true;
				hit = hits[i];
				break;
			}
			if (flag)
			{
				obstruction = hit.collider;
				obstructionDistance = Vector3.Distance(base.transform.position, hit.point);
			}
			else
			{
				obstruction = null;
				obstructionDistance = float.MaxValue;
			}
		}
	}
}
