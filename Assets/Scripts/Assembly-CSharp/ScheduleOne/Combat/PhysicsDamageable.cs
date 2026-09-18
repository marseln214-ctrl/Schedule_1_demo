using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Combat
{
	public class PhysicsDamageable : MonoBehaviour, IDamageable
	{
		public const int VELOCITY_HISTORY_LENGTH = 4;

		public Rigidbody Rb;

		public float ForceMultiplier = 1f;

		private List<int> impactHistory = new List<int>();

		public Action<Impact> onImpacted;

		private List<Vector3> velocityHistory = new List<Vector3>();

		public Vector3 averageVelocity { get; private set; } = Vector3.zero;


		public void OnValidate()
		{
			if (Rb == null)
			{
				Rb = GetComponent<Rigidbody>();
			}
		}

		public virtual void SendImpact(Impact impact)
		{
			ReceiveImpact(impact);
		}

		public virtual void ReceiveImpact(Impact impact)
		{
			if (!impactHistory.Contains(impact.ImpactID))
			{
				impactHistory.Add(impact.ImpactID);
				if (onImpacted != null)
				{
					onImpacted(impact);
				}
				if (Rb != null)
				{
					Rb.AddForceAtPosition(-impact.Hit.normal * impact.ImpactForce * ForceMultiplier, impact.Hit.point, ForceMode.Impulse);
				}
			}
		}
	}
}
