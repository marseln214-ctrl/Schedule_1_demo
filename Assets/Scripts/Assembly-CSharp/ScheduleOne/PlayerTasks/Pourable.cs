using System;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Trash;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScheduleOne.PlayerTasks
{
	[RequireComponent(typeof(Accelerometer))]
	public class Pourable : Draggable
	{
		public Action onInitialPour;

		[Header("Pourable settings")]
		public bool Unlimited;

		public float StartQuantity = 10f;

		public float PourRate_L = 0.25f;

		public float AngleFromUpToPour = 90f;

		[Tooltip("Multiplier for pour rate when pourable is shaken up and down")]
		public float ShakeBoostRate = 1.35f;

		public bool AffectsCoverage;

		[Header("Particles")]
		public float ParticleMinMultiplier = 0.8f;

		public float ParticleMaxMultiplier = 1.5f;

		[Header("Pourable References")]
		public ParticleSystem[] PourParticles;

		public Transform PourPoint;

		public AudioSourceController PourLoop;

		[Header("Trash")]
		public TrashItem TrashItem;

		[HideInInspector]
		public Pot TargetPot;

		public float currentQuantity;

		protected bool hasPoured;

		protected bool autoSetCurrentQuantity = true;

		private float[] particleMinSizes;

		private float[] particleMaxSizes;

		private AverageAcceleration accelerometer;

		public bool IsPouring { get; protected set; }

		public float NormalizedPourRate { get; private set; }

		protected virtual void Start()
		{
			if (autoSetCurrentQuantity)
			{
				currentQuantity = StartQuantity;
			}
			accelerometer = GetComponent<AverageAcceleration>();
			if (accelerometer == null)
			{
				accelerometer = base.gameObject.AddComponent<AverageAcceleration>();
			}
			particleMinSizes = new float[PourParticles.Length];
			particleMaxSizes = new float[PourParticles.Length];
			for (int i = 0; i < PourParticles.Length; i++)
			{
				particleMinSizes[i] = PourParticles[i].main.startSize.constantMin;
				particleMaxSizes[i] = PourParticles[i].main.startSize.constantMax;
			}
		}

		protected override void Update()
		{
			base.Update();
		}

		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			UpdatePouring();
		}

		protected virtual void UpdatePouring()
		{
			float num = Vector3.Angle(Vector3.up, PourPoint.forward);
			IsPouring = num > AngleFromUpToPour && CanPour();
			NormalizedPourRate = 0f;
			if (IsPouring && currentQuantity > 0f)
			{
				float num3 = (NormalizedPourRate = (0.3f + 0.7f * (num - AngleFromUpToPour) / (180f - AngleFromUpToPour)) * GetShakeBoost());
				if (PourLoop != null)
				{
					PourLoop.VolumeMultiplier = num3 - 0.3f;
					if (!PourLoop.isPlaying)
					{
						PourLoop.Play();
					}
				}
				PourAmount(PourRate_L * num3 * Time.deltaTime);
				for (int i = 0; i < PourParticles.Length; i++)
				{
					ParticleSystem.MainModule main = PourParticles[i].main;
					float min = ParticleMinMultiplier * num3 * particleMinSizes[i];
					float max = ParticleMaxMultiplier * num3 * particleMaxSizes[i];
					main.startSize = new ParticleSystem.MinMaxCurve(min, max);
				}
				if (!PourParticles[0].isEmitting && currentQuantity > 0f)
				{
					for (int j = 0; j < PourParticles.Length; j++)
					{
						PourParticles[j].Play();
					}
				}
			}
			else
			{
				if (PourLoop != null && PourLoop.isPlaying)
				{
					PourLoop.Stop();
				}
				if (PourParticles[0].isEmitting)
				{
					for (int k = 0; k < PourParticles.Length; k++)
					{
						PourParticles[k].Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
					}
				}
			}
			if (currentQuantity == 0f && PourParticles[0].isEmitting)
			{
				for (int l = 0; l < PourParticles.Length; l++)
				{
					PourParticles[l].Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
				}
			}
		}

		private float GetShakeBoost()
		{
			return Mathf.Lerp(1f, ShakeBoostRate, Mathf.Clamp(accelerometer.Acceleration.y / 0.75f, 0f, 1f));
		}

		protected virtual void PourAmount(float amount)
		{
			if (!Unlimited)
			{
				currentQuantity = Mathf.Clamp(currentQuantity - amount, 0f, StartQuantity);
			}
			if (AffectsCoverage && IsPourPointOverPot())
			{
				TargetPot.SoilCover.QueuePour(PourPoint.position + PourPoint.forward * 0.05f);
			}
			if (!hasPoured)
			{
				if (onInitialPour != null)
				{
					onInitialPour();
				}
				hasPoured = true;
			}
		}

		protected bool IsPourPointOverPot()
		{
			Vector3 position = PourPoint.position;
			position.y = TargetPot.transform.position.y;
			return Vector3.Distance(position, TargetPot.transform.position) < TargetPot.PotRadius;
		}

		protected virtual bool CanPour()
		{
			return true;
		}
	}
}
