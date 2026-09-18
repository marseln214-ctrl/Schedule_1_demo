using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Equipping
{
	public class Equippable_RangedWeapon : Equippable_AvatarViewmodel
	{
		public const float NPC_AIM_DETECTION_RANGE = 10f;

		public int MagazineSize = 7;

		[Header("Aim Settings")]
		public float AimDuration = 0.2f;

		public float AimFOVReduction = 10f;

		public float FOVChangeDuration = 0.3f;

		[Header("Firing")]
		public AudioSourceController FireSound;

		public AudioSourceController EmptySound;

		public float FireCooldown = 0.3f;

		public string[] FireAnimTriggers;

		public float AccuracyChangeDuration = 0.6f;

		[Header("Raycasting")]
		public float Range = 40f;

		public float RayRadius = 0.05f;

		public LayerMask FireLayerMask;

		[Header("Spread")]
		public float MinSpread = 5f;

		public float MaxSpread = 15f;

		[Header("Damage")]
		public float Damage = 60f;

		public float ImpactForce = 300f;

		[Header("Reloading")]
		public bool CanReload = true;

		public bool IncrementalReload;

		public StorableItemDefinition Magazine;

		public float ReloadStartTime = 1.5f;

		public float ReloadIndividalTime;

		public float ReloadEndTime;

		public string ReloadStartAnimTrigger = "MagazineReload";

		public string ReloadIndividualAnimTrigger = string.Empty;

		public string ReloadEndAnimTrigger = string.Empty;

		public TrashItem ReloadTrash;

		[Header("Cocking")]
		public bool MustBeCocked;

		public float CockTime = 0.5f;

		public string CockAnimTrigger = "MagazineReload";

		[Header("Effects")]
		public float TracerSpeed = 50f;

		public UnityEvent onFire;

		public UnityEvent onReloadStart;

		public UnityEvent onReloadIndividual;

		public UnityEvent onReloadEnd;

		public UnityEvent onCockStart;

		private IntegerItemInstance weaponItem;

		private bool fovOverridden;

		private float aimVelocity;

		private Coroutine reloadRoutine;

		private bool shotQueued;

		private bool reloadQueued;

		private float timeSincePrimaryClick = 100f;

		public float Aim { get; private set; }

		public float Accuracy { get; private set; }

		public float TimeSinceFire { get; set; } = 1000f;


		public bool IsReloading { get; private set; }

		public bool IsCocked { get; private set; }

		public bool IsCocking { get; private set; }

		public int Ammo
		{
			get
			{
				if (weaponItem == null)
				{
					return 0;
				}
				return weaponItem.Value;
			}
		}

		private float aimFov => Singleton<Settings>.Instance.CameraFOV - AimFOVReduction;

		public override void Equip(ItemInstance item)
		{
			base.Equip(item);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			Singleton<InputPromptsCanvas>.Instance.LoadModule("gun");
			weaponItem = item as IntegerItemInstance;
			InvokeRepeating("CheckAimingAtNPC", 0f, 0.5f);
		}

		public override void Unequip()
		{
			base.Unequip();
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
			if (fovOverridden)
			{
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(FOVChangeDuration);
				PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
				fovOverridden = false;
			}
			if (reloadRoutine != null)
			{
				StopCoroutine(reloadRoutine);
			}
		}

		protected override void Update()
		{
			base.Update();
			UpdateInput();
			UpdateAnim();
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			TimeSinceFire += Time.deltaTime;
		}

		private void UpdateInput()
		{
		}

		private void UpdateAnim()
		{
			Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Aim", Aim);
		}

		private bool CanAim()
		{
			return true;
		}

		public virtual void Fire()
		{
		}

		public virtual void Reload()
		{
		}

		private bool IsReloadReady(bool ignoreTiming)
		{
			if (!CanReload)
			{
				return false;
			}
			if (IsReloading)
			{
				return false;
			}
			if (!GetMagazine(out var _))
			{
				return false;
			}
			if (weaponItem.Value >= MagazineSize)
			{
				return false;
			}
			if (TimeSinceFire < FireCooldown && !ignoreTiming)
			{
				return false;
			}
			if (!base.equipAnimDone && !ignoreTiming)
			{
				return false;
			}
			if (IsCocking)
			{
				return false;
			}
			return true;
		}

		protected virtual bool GetMagazine(out StorableItemInstance mag)
		{
			mag = null;
			for (int i = 0; i < PlayerSingleton<PlayerInventory>.Instance.hotbarSlots.Count; i++)
			{
				if (PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].Quantity != 0 && PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].ItemInstance.ID == Magazine.ID)
				{
					mag = PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].ItemInstance as StorableItemInstance;
					return true;
				}
			}
			return false;
		}

		private bool CanFire(bool checkAmmo = true)
		{
			if (TimeSinceFire < FireCooldown)
			{
				return false;
			}
			if (Aim < 0.1f)
			{
				return false;
			}
			if (!base.equipAnimDone)
			{
				return false;
			}
			if (checkAmmo && Ammo <= 0)
			{
				return false;
			}
			if (IsReloading)
			{
				return false;
			}
			if (IsCocking)
			{
				return false;
			}
			return true;
		}

		private bool CanCock()
		{
			if (IsCocked)
			{
				return false;
			}
			if (IsCocking)
			{
				return false;
			}
			if (weaponItem.Value <= 0)
			{
				return false;
			}
			if (!base.equipAnimDone)
			{
				return false;
			}
			if (IsReloading)
			{
				return false;
			}
			if (TimeSinceFire < FireCooldown)
			{
				return false;
			}
			return true;
		}

		private void Cock()
		{
			Console.Log("Cocking");
			shotQueued = false;
			IsCocking = true;
			StartCoroutine(CockRoutine());
			IEnumerator CockRoutine()
			{
				if (onCockStart != null)
				{
					onCockStart.Invoke();
				}
				Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(CockAnimTrigger);
				yield return new WaitForSeconds(CockTime);
				IsCocked = true;
				IsCocking = false;
			}
		}

		private float GetSpread()
		{
			return Mathf.Lerp(MaxSpread, MinSpread, Accuracy);
		}

		private void CheckAimingAtNPC()
		{
			if (Aim < 0.5f)
			{
				return;
			}
			RaycastHit[] array = Physics.SphereCastAll(new Ray(PlayerSingleton<PlayerCamera>.Instance.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.forward), 0.5f, 10f, FireLayerMask);
			List<NPC> list = new List<NPC>();
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				NPC componentInParent = raycastHit.collider.GetComponentInParent<NPC>();
				if (componentInParent != null && !list.Contains(componentInParent))
				{
					list.Add(componentInParent);
					if (componentInParent.awareness.VisionCone.IsPlayerVisible(Player.Local))
					{
						componentInParent.responses.RespondToAimedAt(Player.Local);
					}
				}
			}
		}
	}
}
