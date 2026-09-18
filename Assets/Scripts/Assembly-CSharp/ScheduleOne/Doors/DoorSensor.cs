using System.Collections.Generic;
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Doors
{
	[RequireComponent(typeof(Rigidbody))]
	public class DoorSensor : MonoBehaviour
	{
		public const float ActivationDistance = 30f;

		public EDoorSide DetectorSide = EDoorSide.Exterior;

		public DoorController Door;

		private List<Collider> exclude = new List<Collider>();

		private Collider collider;

		private void Awake()
		{
			collider = GetComponent<Collider>();
			InvokeRepeating("UpdateCollider", 0f, 1f);
		}

		private void UpdateCollider()
		{
			if (!(PlayerSingleton<PlayerCamera>.Instance == null))
			{
				float distance = Vector3.Distance(PlayerSingleton<PlayerCamera>.Instance.transform.position, base.transform.position);
				if (InstanceFinder.IsServer)
				{
					Player.GetClosestPlayer(base.transform.position, out distance);
				}
				collider.enabled = distance < 30f;
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if (!exclude.Contains(other))
			{
				NPC componentInParent = other.GetComponentInParent<NPC>();
				if (componentInParent != null && componentInParent.IsConscious && !componentInParent.Avatar.Ragdolled && componentInParent.CanOpenDoors)
				{
					Door.NPCVicinityDetected(DetectorSide);
				}
				else if (other.GetComponentInParent<Player>() != null)
				{
					Door.PlayerVicinityDetected(DetectorSide);
				}
				else
				{
					exclude.Add(other);
				}
			}
		}
	}
}
