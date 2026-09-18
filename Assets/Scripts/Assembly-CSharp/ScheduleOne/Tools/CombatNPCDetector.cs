using ScheduleOne.NPCs;
using ScheduleOne.Police;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	[RequireComponent(typeof(Rigidbody))]
	public class CombatNPCDetector : MonoBehaviour
	{
		public bool DetectOnlyInCombat;

		public UnityEvent onDetected;

		public float ContactTimeForDetection = 0.5f;

		private float contactTime;

		private float timeSinceLastContact = 100f;

		private void Awake()
		{
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = base.gameObject.AddComponent<Rigidbody>();
			}
			rigidbody.isKinematic = true;
		}

		private void FixedUpdate()
		{
			if (timeSinceLastContact < 0.1f)
			{
				contactTime += Time.fixedDeltaTime;
				if (contactTime >= ContactTimeForDetection)
				{
					contactTime = 0f;
					if (onDetected != null)
					{
						onDetected.Invoke();
					}
				}
			}
			else
			{
				contactTime = 0f;
			}
			timeSinceLastContact += Time.fixedDeltaTime;
		}

		private void OnTriggerStay(Collider other)
		{
			NPC componentInParent = other.GetComponentInParent<NPC>();
			if (componentInParent != null && (!DetectOnlyInCombat || componentInParent.behaviour.CombatBehaviour.Active))
			{
				timeSinceLastContact = 0f;
				return;
			}
			PoliceOfficer policeOfficer = componentInParent as PoliceOfficer;
			if (policeOfficer != null && (!DetectOnlyInCombat || policeOfficer.PursuitBehaviour.Active))
			{
				timeSinceLastContact = 0f;
			}
		}
	}
}
