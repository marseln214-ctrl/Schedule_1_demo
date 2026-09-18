using System;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Audio
{
	[RequireComponent(typeof(Rigidbody))]
	public class ImpactSoundEntity : MonoBehaviour
	{
		public enum EMaterial
		{
			Wood = 0,
			HollowMetal = 1,
			Cardboard = 2,
			Glass = 3,
			Plastic = 4,
			Basketball = 5,
			SmallHollowMetal = 6,
			PlasticBag = 7,
			Punch = 8,
			BaseballBat = 9
		}

		public const float MIN_IMPACT_MOMENTUM = 4f;

		public const float COOLDOWN = 0.25f;

		public EMaterial Material;

		private float lastImpactTime;

		private Rigidbody rb;

		public void Awake()
		{
			PhysicsDamageable component = GetComponent<PhysicsDamageable>();
			if (component != null)
			{
				component.onImpacted = (Action<Impact>)Delegate.Combine(component.onImpacted, new Action<Impact>(OnImpacted));
			}
			rb = GetComponent<Rigidbody>();
		}

		private void OnImpacted(Impact impact)
		{
			if (!(Vector3.SqrMagnitude(impact.Hit.point - PlayerSingleton<PlayerCamera>.Instance.transform.position) > 1600f) && !(Time.time - lastImpactTime < 0.25f))
			{
				float impactForce = impact.ImpactForce;
				if (!(impactForce < 4f))
				{
					Singleton<SFXManager>.Instance.PlayImpactSound(Material, impact.Hit.point, impactForce);
				}
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!(Time.time - lastImpactTime < 0.25f) && PlayerSingleton<PlayerCamera>.InstanceExists && !(Vector3.SqrMagnitude(collision.contacts[0].point - PlayerSingleton<PlayerCamera>.Instance.transform.position) > 1600f))
			{
				Rigidbody rigidbody = collision.rigidbody;
				float magnitude = collision.relativeVelocity.magnitude;
				float num = rb.mass;
				if (rigidbody != null)
				{
					num = Mathf.Min(num, rigidbody.mass);
				}
				magnitude *= num;
				if (!(magnitude < 4f))
				{
					lastImpactTime = Time.time;
					Singleton<SFXManager>.Instance.PlayImpactSound(Material, collision.contacts[0].point, magnitude);
				}
			}
		}
	}
}
