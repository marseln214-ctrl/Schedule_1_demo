using ScheduleOne.Audio;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class PourableModule : ItemModule
	{
		[Header("Settings")]
		public string LiquidType = "Liquid";

		public float PourRate = 0.2f;

		public float AngleFromUpToPour = 90f;

		public bool OnlyEmptyOverFillable = true;

		public float LiquidCapacity_L = 0.25f;

		public Color LiquidColor;

		public float DefaultLiquid_L = 1f;

		[Header("References")]
		public ParticleSystem[] PourParticles;

		public Transform PourPoint;

		public LiquidContainer LiquidContainer;

		public Draggable Draggable;

		public DraggableConstraint DraggableConstraint;

		public AudioSourceController PourSound;

		[Header("Particles")]
		public Color PourParticlesColor;

		public float ParticleMinMultiplier = 0.8f;

		public float ParticleMaxMultiplier = 1.5f;

		private float[] particleMinSizes;

		private float[] particleMaxSizes;

		private Fillable activeFillable;

		private float timeSinceFillableHit = 10f;

		public bool IsPouring { get; protected set; }

		public float NormalizedPourRate { get; private set; }

		public float LiquidLevel { get; protected set; } = 1f;


		public float NormalizedLiquidLevel => LiquidLevel / LiquidCapacity_L;

		protected virtual void Start()
		{
			particleMinSizes = new float[PourParticles.Length];
			particleMaxSizes = new float[PourParticles.Length];
			for (int i = 0; i < PourParticles.Length; i++)
			{
				particleMinSizes[i] = PourParticles[i].main.startSize.constantMin;
				particleMaxSizes[i] = PourParticles[i].main.startSize.constantMax;
				ParticleSystem.CollisionModule collision = PourParticles[i].collision;
				LayerMask collidesWith = collision.collidesWith;
				collidesWith = (int)collidesWith | (1 << LayerMask.NameToLayer("Task"));
				collision.collidesWith = collidesWith;
				collision.sendCollisionMessages = true;
				PourParticles[i].gameObject.AddComponent<ParticleCollisionDetector>().onCollision.AddListener(ParticleCollision);
			}
			if (LiquidContainer != null)
			{
				SetLiquidLevel(DefaultLiquid_L);
			}
		}

		public override void ActivateModule(StationItem item)
		{
			base.ActivateModule(item);
			if (DraggableConstraint != null)
			{
				DraggableConstraint.SetContainer(item.transform.parent);
			}
			if (Draggable != null)
			{
				Draggable.ClickableEnabled = true;
			}
		}

		protected virtual void FixedUpdate()
		{
			if (base.IsModuleActive)
			{
				UpdatePouring();
				UpdatePourSound();
				if (timeSinceFillableHit > 0.25f)
				{
					activeFillable = null;
				}
				timeSinceFillableHit += Time.fixedDeltaTime;
			}
		}

		protected virtual void UpdatePouring()
		{
			float num = Vector3.Angle(Vector3.up, PourPoint.forward);
			IsPouring = num > AngleFromUpToPour && CanPour();
			NormalizedPourRate = 0f;
			if (IsPouring && NormalizedLiquidLevel > 0f)
			{
				float num3 = (NormalizedPourRate = 0.3f + 0.7f * (num - AngleFromUpToPour) / (180f - AngleFromUpToPour));
				PourAmount(num3 * PourRate * Time.deltaTime);
				for (int i = 0; i < PourParticles.Length; i++)
				{
					ParticleSystem.MainModule main = PourParticles[i].main;
					float num4 = 1f;
					if (LiquidContainer != null)
					{
						num4 = Mathf.Clamp(LiquidContainer.CurrentLiquidLevel, 0.3f, 1f);
					}
					float min = ParticleMinMultiplier * num3 * particleMinSizes[i] * num4;
					float max = ParticleMaxMultiplier * num3 * particleMaxSizes[i] * num4;
					main.startSize = new ParticleSystem.MinMaxCurve(min, max);
					main.startColor = PourParticlesColor;
				}
				if (!PourParticles[0].isEmitting && NormalizedLiquidLevel > 0f)
				{
					for (int j = 0; j < PourParticles.Length; j++)
					{
						PourParticles[j].Play();
					}
				}
			}
			else if (PourParticles[0].isEmitting)
			{
				for (int k = 0; k < PourParticles.Length; k++)
				{
					PourParticles[k].Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
				}
			}
			if (NormalizedLiquidLevel == 0f && PourParticles[0].isEmitting)
			{
				for (int l = 0; l < PourParticles.Length; l++)
				{
					PourParticles[l].Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
				}
			}
		}

		private void UpdatePourSound()
		{
			if (PourSound == null)
			{
				return;
			}
			if (NormalizedPourRate > 0f)
			{
				PourSound.VolumeMultiplier = NormalizedPourRate;
				if (!PourSound.isPlaying)
				{
					PourSound.Play();
				}
			}
			else if (PourSound.isPlaying)
			{
				PourSound.Stop();
			}
		}

		public virtual void ChangeLiquidLevel(float change)
		{
			LiquidLevel = Mathf.Clamp(LiquidLevel + change, 0f, LiquidCapacity_L);
			if (LiquidContainer != null)
			{
				LiquidContainer.SetLiquidLevel(NormalizedLiquidLevel);
			}
		}

		public virtual void SetLiquidLevel(float level)
		{
			LiquidLevel = Mathf.Clamp(level, 0f, LiquidCapacity_L);
			if (LiquidContainer != null)
			{
				LiquidContainer.SetLiquidLevel(NormalizedLiquidLevel);
			}
		}

		protected virtual void PourAmount(float amount)
		{
			Physics.RaycastAll(PourPoint.position, Vector3.down, 1f, 1 << LayerMask.NameToLayer("Task"));
			if (!OnlyEmptyOverFillable || (activeFillable != null && activeFillable.FillableEnabled))
			{
				ChangeLiquidLevel(0f - amount);
				if (activeFillable != null)
				{
					activeFillable.AddLiquid(LiquidType, amount, LiquidColor);
				}
			}
		}

		private void ParticleCollision(GameObject other)
		{
			Fillable componentInParent = other.GetComponentInParent<Fillable>();
			if (componentInParent != null && componentInParent.enabled)
			{
				timeSinceFillableHit = 0f;
				activeFillable = componentInParent;
			}
		}

		protected virtual bool CanPour()
		{
			return true;
		}
	}
}
