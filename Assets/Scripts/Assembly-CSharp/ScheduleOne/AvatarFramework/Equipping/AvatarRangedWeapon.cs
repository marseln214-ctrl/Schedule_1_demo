using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping
{
	public class AvatarRangedWeapon : AvatarWeapon
	{
		public static string[] RaycastLayers = new string[4] { "Default", "Vehicle", "Door", "Terrain" };

		[Header("Weapon Settings")]
		public int MagazineSize = -1;

		public float ReloadTime = 2f;

		public float MaxFireRate = 0.5f;

		public bool CanShootWhileMoving;

		public float EquipTime = 1f;

		public float RaiseTime = 1f;

		public float Damage = 35f;

		[Header("Accuracy")]
		public float HitChange_MinRange = 0.6f;

		public float HitChange_MaxRange = 0.1f;

		[Header("References")]
		public Transform MuzzlePoint;

		public AudioSourceController FireSound;

		[Header("Settings")]
		public string LoweredAnimationTrigger;

		public string RaisedAnimationTrigger;

		public string RecoilAnimationTrigger;

		private bool isReloading;

		private float timeEquipped;

		private float timeRaised;

		private float timeSinceLastShot = 1000f;

		private int currentAmmo;

		public bool IsRaised { get; protected set; }

		public override void Equip(Avatar _avatar)
		{
			base.Equip(_avatar);
			if (MagazineSize != -1)
			{
				currentAmmo = MagazineSize;
			}
		}

		public virtual void SetIsRaised(bool raised)
		{
			if (IsRaised != raised)
			{
				IsRaised = raised;
				timeRaised = 0f;
				if (IsRaised)
				{
					ResetTrigger(LoweredAnimationTrigger);
					SetTrigger(RaisedAnimationTrigger);
				}
				else
				{
					ResetTrigger(RaisedAnimationTrigger);
					SetTrigger(LoweredAnimationTrigger);
				}
			}
		}

		private void Update()
		{
			timeEquipped += Time.deltaTime;
			timeSinceLastShot += Time.deltaTime;
			if (IsRaised)
			{
				timeRaised += Time.deltaTime;
			}
		}

		public override void ReceiveMessage(string message, object data)
		{
			base.ReceiveMessage(message, data);
			if (message == "Shoot")
			{
				Shoot((Vector3)data);
			}
			if (message == "Lower")
			{
				SetIsRaised(raised: false);
			}
			if (message == "Raise")
			{
				SetIsRaised(raised: true);
			}
		}

		public bool CanShoot()
		{
			if ((currentAmmo > 0 || MagazineSize == -1) && timeEquipped > EquipTime && !isReloading && timeSinceLastShot > MaxFireRate)
			{
				return timeRaised > RaiseTime;
			}
			return false;
		}

		public virtual void Shoot(Vector3 endPoint)
		{
			timeSinceLastShot = 0f;
			if (RecoilAnimationTrigger != string.Empty)
			{
				ResetTrigger(RecoilAnimationTrigger);
				SetTrigger(RecoilAnimationTrigger);
			}
			Player componentInParent = GetComponentInParent<Player>();
			if (!(componentInParent != null) || !componentInParent.IsOwner)
			{
				currentAmmo--;
				FireSound.PlayOneShot(duplicateAudioSource: true);
				if (currentAmmo <= 0 && MagazineSize != -1)
				{
					StartCoroutine(Reload());
				}
			}
		}

		private IEnumerator Reload()
		{
			isReloading = true;
			yield return new WaitForSeconds(ReloadTime);
			currentAmmo = MagazineSize;
			isReloading = false;
		}

		public bool IsPlayerInLoS(Player target)
		{
			LayerMask layerMask = LayerMask.GetMask(RaycastLayers);
			if (Physics.Raycast(MuzzlePoint.position, (target.Avatar.CenterPoint - MuzzlePoint.position).normalized, out var hitInfo, Vector3.Distance(MuzzlePoint.position, target.Avatar.CenterPoint), layerMask) && (bool)hitInfo.collider.GetComponentInParent<Player>())
			{
				if (hitInfo.collider.GetComponentInParent<Player>() == target)
				{
					return true;
				}
				if (hitInfo.collider.GetComponentInParent<LandVehicle>() != null)
				{
					return hitInfo.collider.GetComponentInParent<LandVehicle>().DriverPlayer == target;
				}
				return false;
			}
			return true;
		}
	}
}
