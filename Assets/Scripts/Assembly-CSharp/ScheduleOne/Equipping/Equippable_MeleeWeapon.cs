using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Equipping
{
	public class Equippable_MeleeWeapon : Equippable_AvatarViewmodel
	{
		[Header("Basic Settings")]
		public EImpactType ImpactType;

		public float Range = 1.25f;

		public float HitRadius = 0.2f;

		[Header("Timing")]
		public float MaxLoadTime = 1f;

		public float MinCooldown = 0.1f;

		public float MaxCooldown = 0.2f;

		public float MinHitDelay = 0.1f;

		public float MaxHitDelay = 0.2f;

		[Header("Damage")]
		public float MinDamage = 20f;

		public float MaxDamage = 60f;

		public float MinForce = 100f;

		public float MaxForce = 300f;

		[Header("Stamina Settings")]
		public float MinStaminaCost = 10f;

		public float MaxStaminaCost = 40f;

		[Header("Sound")]
		public AudioSourceController WhooshSound;

		public float WhooshSoundPitch = 1f;

		public AudioSourceController ImpactSound;

		[Header("Animation")]
		public string SwingAnimationTrigger;

		private float load;

		private float remainingCooldown;

		private Coroutine hitRoutine;

		private bool loadQueued;

		private bool clickReleased;

		public bool IsLoading => load > 0f;

		public bool IsAttacking { get; private set; }

		protected override void Update()
		{
			base.Update();
			if (!Singleton<PauseMenu>.Instance.IsPaused)
			{
				UpdateInput();
				UpdateCooldown();
			}
		}

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
		}

		public override void Unequip()
		{
			base.Unequip();
			PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", 0f);
		}

		private void UpdateCooldown()
		{
			if (remainingCooldown > 0f && !IsLoading && !IsAttacking)
			{
				remainingCooldown -= Time.deltaTime;
				remainingCooldown = Mathf.Clamp(remainingCooldown, 0f, MaxCooldown);
			}
		}

		private void UpdateInput()
		{
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				if (load == 0f)
				{
					if (!GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && (!loadQueued || !GameInput.GetButton(GameInput.ButtonCode.PrimaryClick)))
					{
						return;
					}
					if (CanStartLoading())
					{
						StartLoad();
					}
					else if (clickReleased)
					{
						loadQueued = true;
					}
				}
				if (load >= 0.0001f)
				{
					load += Time.deltaTime;
					if (load < MaxLoadTime)
					{
						PlayerSingleton<PlayerMovement>.Instance.ChangeStamina((0f - (MaxStaminaCost - MinStaminaCost)) * Time.deltaTime / MaxLoadTime);
					}
					else
					{
						PlayerSingleton<PlayerMovement>.Instance.ChangeStamina(-1E-07f);
					}
				}
				clickReleased = false;
				Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", Mathf.Clamp01(load / MaxLoadTime));
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", Mathf.Clamp01(load / MaxLoadTime));
				if (IsLoading && PlayerSingleton<PlayerMovement>.Instance.CurrentStaminaReserve <= 0f)
				{
					Release();
				}
			}
			else
			{
				clickReleased = true;
				loadQueued = false;
				if (load > 0f)
				{
					Release();
				}
			}
		}

		private bool CanStartLoading()
		{
			if (remainingCooldown > 0f)
			{
				return false;
			}
			if (IsAttacking)
			{
				return false;
			}
			if (!base.equipAnimDone)
			{
				return false;
			}
			if (PlayerSingleton<PlayerMovement>.Instance.CurrentStaminaReserve < MinStaminaCost)
			{
				return false;
			}
			if (GameManager.IS_TUTORIAL)
			{
				return false;
			}
			return true;
		}

		private void StartLoad()
		{
			loadQueued = false;
			load = 0.001f;
			PlayerSingleton<PlayerMovement>.Instance.ChangeStamina(0f - MinStaminaCost);
			Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", 0f);
			PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", 0f);
		}

		private void Release()
		{
			loadQueued = false;
			float num = Mathf.Clamp01(load / MaxLoadTime);
			remainingCooldown = Mathf.Lerp(MinCooldown, MaxCooldown, num);
			Hit(num);
			PlayerSingleton<PlayerMovement>.Instance.SetResidualVelocity(Player.Local.transform.forward, Mathf.Lerp(0f, 300f, num), Mathf.Lerp(0.05f, 0.15f, num));
			if (num >= 1f)
			{
				Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger("Release_Heavy");
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetTrigger("Release_Heavy");
			}
			else
			{
				Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger("Release_Light");
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetTrigger("Release_Light");
			}
			if (SwingAnimationTrigger != string.Empty)
			{
				Player.Local.SendAnimationTrigger(SwingAnimationTrigger);
			}
			load = 0f;
		}

		private void Hit(float power)
		{
			IsAttacking = true;
			WhooshSound.VolumeMultiplier = Mathf.Lerp(0.4f, 1f, power);
			WhooshSound.PitchMultiplier = Mathf.Lerp(1f, 0.8f, power) * WhooshSoundPitch;
			WhooshSound.Play();
			hitRoutine = StartCoroutine(HitRoutine());
			IEnumerator HitRoutine()
			{
				yield return new WaitForSeconds(Mathf.Lerp(MinHitDelay, MaxHitDelay, power));
				ExecuteHit(power);
				IsAttacking = false;
				yield return new WaitForEndOfFrame();
				Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", 0f);
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", 0f);
			}
		}

		private void ExecuteHit(float power)
		{
			if (!PlayerSingleton<PlayerCamera>.Instance.LookRaycast(Range, out var hit, Singleton<CombatManager>.Instance.MeleeLayerMask, includeTriggers: true, HitRadius))
			{
				return;
			}
			IDamageable componentInParent = hit.collider.GetComponentInParent<IDamageable>();
			if (componentInParent != null)
			{
				float impactDamage = Mathf.Lerp(MinDamage, MaxDamage, power);
				float impactForce = Mathf.Lerp(MinForce, MaxForce, power);
				Impact impact = new Impact(hit, hit.point, PlayerSingleton<PlayerCamera>.Instance.transform.forward, impactForce, impactDamage, ImpactType, Player.Local, Random.Range(int.MinValue, int.MaxValue));
				Console.Log("Hit " + componentInParent?.ToString() + " with " + impactDamage + " damage and " + impactForce + " force.");
				componentInParent.SendImpact(impact);
				Singleton<FXManager>.Instance.CreateImpactFX(impact);
				ImpactSound.Play();
				PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(Mathf.Lerp(0.1f, 0.4f, power), 0.2f);
				if (componentInParent is NPC)
				{
					Player.Local.VisualState.ApplyState("melee_attack", PlayerVisualState.EVisualState.Brandishing, 1.5f);
				}
			}
		}
	}
}
