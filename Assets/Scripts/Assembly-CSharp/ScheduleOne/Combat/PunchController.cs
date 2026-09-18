using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dragging;
using ScheduleOne.FX;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Combat
{
	public class PunchController : MonoBehaviour
	{
		public const float MAX_PUNCH_LOAD = 1f;

		public const float MIN_COOLDOWN = 0.1f;

		public const float MAX_COOLDOWN = 0.2f;

		public const float PUNCH_RANGE = 1.25f;

		public const float PUNCH_DEBOUNCE = 0.1f;

		[Header("Settings")]
		public Vector3 ViewmodelAvatarOffset = new Vector3(0f, 0f, 0f);

		public float MinPunchDamage = 20f;

		public float MaxPunchDamage = 60f;

		public float MinPunchForce = 100f;

		public float MaxPunchForce = 300f;

		[Header("Stamina Settings")]
		public float MinStaminaCost = 10f;

		public float MaxStaminaCost = 40f;

		[Header("References")]
		public AudioSourceController PunchSound;

		public RuntimeAnimatorController PunchAnimator;

		private float punchLoad;

		private float remainingCooldown;

		private Player player;

		private Coroutine punchRoutine;

		private bool itemEquippedLastFrame;

		private float timeSincePunchingEnabled;

		public bool PunchingEnabled { get; set; } = true;


		public bool IsLoading => punchLoad > 0f;

		public bool IsPunching { get; private set; }

		private void Awake()
		{
			player = GetComponentInParent<Player>();
		}

		private void Start()
		{
			PlayerSingleton<PlayerInventory>.Instance.onPreItemEquipped.AddListener(delegate
			{
				SetPunchingEnabled(enabled: false);
			});
		}

		private void Update()
		{
			SetPunchingEnabled(ShouldBeEnabled());
			if (PunchingEnabled && !(timeSincePunchingEnabled < 0.1f))
			{
				UpdateInput();
				UpdateCooldown();
				itemEquippedLastFrame = PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped;
			}
		}

		private void LateUpdate()
		{
			if (PunchingEnabled)
			{
				timeSincePunchingEnabled += Time.deltaTime;
			}
			else
			{
				timeSincePunchingEnabled = 0f;
			}
		}

		private void UpdateCooldown()
		{
			if (remainingCooldown > 0f && !IsLoading && !IsPunching)
			{
				remainingCooldown -= Time.deltaTime;
				remainingCooldown = Mathf.Clamp(remainingCooldown, 0f, 0.2f);
			}
		}

		private void UpdateInput()
		{
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
			{
				if (punchLoad == 0f)
				{
					if (!CanStartLoading() || !GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
					{
						return;
					}
					StartLoad();
				}
				punchLoad += Time.deltaTime;
				Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", punchLoad / 1f);
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", punchLoad / 1f);
				if (punchLoad < 1f)
				{
					PlayerSingleton<PlayerMovement>.Instance.ChangeStamina((0f - (MaxStaminaCost - MinStaminaCost)) * Time.deltaTime / 1f);
				}
				else
				{
					PlayerSingleton<PlayerMovement>.Instance.ChangeStamina(-1E-07f);
				}
				if (IsLoading && PlayerSingleton<PlayerMovement>.Instance.CurrentStaminaReserve <= 0f)
				{
					Release();
				}
			}
			else if (punchLoad > 0f)
			{
				Release();
			}
		}

		private bool CanStartLoading()
		{
			if (remainingCooldown > 0f)
			{
				return false;
			}
			if (IsPunching)
			{
				return false;
			}
			if (PlayerSingleton<PlayerMovement>.Instance.CurrentStaminaReserve < MinStaminaCost)
			{
				return false;
			}
			if (itemEquippedLastFrame)
			{
				return false;
			}
			return true;
		}

		private void StartLoad()
		{
			PlayerSingleton<PlayerMovement>.Instance.ChangeStamina(0f - MinStaminaCost);
			Singleton<ViewmodelAvatar>.Instance.SetVisibility(isVisible: true);
			Singleton<ViewmodelAvatar>.Instance.SetOffset(ViewmodelAvatarOffset);
			Singleton<ViewmodelAvatar>.Instance.SetAnimatorController(PunchAnimator);
			Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", 0f);
			PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", 0f);
		}

		private void Release()
		{
			float num = Mathf.Clamp01(punchLoad / 1f);
			Punch(num);
			PlayerSingleton<PlayerMovement>.Instance.SetResidualVelocity(player.transform.forward, Mathf.Lerp(0f, 300f, num), Mathf.Lerp(0.05f, 0.15f, num));
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
			punchLoad = 0f;
		}

		private void Punch(float power)
		{
			IsPunching = true;
			PunchSound.VolumeMultiplier = Mathf.Lerp(0.4f, 1f, power);
			PunchSound.PitchMultiplier = Mathf.Lerp(1f, 0.8f, power);
			PunchSound.Play();
			player.SendPunch();
			punchRoutine = StartCoroutine(PunchRoutine());
			IEnumerator PunchRoutine()
			{
				yield return new WaitForSeconds(0.1f);
				ExecuteHit(power);
				yield return new WaitForSeconds(0.3f);
				remainingCooldown = Mathf.Lerp(0.1f, 0.2f, power);
				IsPunching = false;
				Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", 0f);
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", 0f);
				Singleton<ViewmodelAvatar>.Instance.SetVisibility(isVisible: false);
			}
		}

		private void ExecuteHit(float power)
		{
			if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(1.25f, out var hit, Singleton<CombatManager>.Instance.MeleeLayerMask, includeTriggers: true, 0.3f))
			{
				IDamageable componentInParent = hit.collider.GetComponentInParent<IDamageable>();
				if (componentInParent != null)
				{
					float impactDamage = Mathf.Lerp(MinPunchDamage, MaxPunchDamage, power);
					float impactForce = Mathf.Lerp(MinPunchForce, MaxPunchForce, power);
					Impact impact = new Impact(hit, hit.point, PlayerSingleton<PlayerCamera>.Instance.transform.forward, impactForce, impactDamage, EImpactType.Punch, player, Random.Range(int.MinValue, int.MaxValue));
					Console.Log("Hit " + componentInParent?.ToString() + " with " + impactDamage + " damage and " + impactForce + " force.");
					componentInParent.SendImpact(impact);
					Singleton<FXManager>.Instance.CreateImpactFX(impact);
					PlayerSingleton<PlayerCamera>.Instance.StartCameraShake(Mathf.Lerp(0.1f, 0.4f, power), 0.2f);
				}
			}
		}

		private void SetPunchingEnabled(bool enabled)
		{
			if (PunchingEnabled == enabled)
			{
				return;
			}
			PunchingEnabled = enabled;
			if (!PunchingEnabled)
			{
				punchLoad = 0f;
				Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Load", 0f);
				Singleton<ViewmodelAvatar>.Instance.SetVisibility(isVisible: false);
				PlayerSingleton<PlayerCamera>.Instance.Animator.SetFloat("Load", 0f);
				if (punchRoutine != null)
				{
					StopCoroutine(punchRoutine);
					remainingCooldown = 0.1f;
					IsPunching = false;
					punchRoutine = null;
				}
			}
		}

		private bool ShouldBeEnabled()
		{
			if (!PlayerSingleton<PlayerInventory>.InstanceExists || !PlayerSingleton<PlayerCamera>.InstanceExists || Player.Local == null || !Singleton<PauseMenu>.InstanceExists)
			{
				return false;
			}
			if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
			{
				return false;
			}
			if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount > 0)
			{
				return false;
			}
			if (Singleton<PauseMenu>.Instance.IsPaused)
			{
				return false;
			}
			if (Player.Local.CurrentVehicle != null)
			{
				return false;
			}
			if (!Player.Local.Health.IsAlive)
			{
				return false;
			}
			if (NetworkSingleton<DragManager>.Instance.IsDragging)
			{
				return false;
			}
			return true;
		}
	}
}
