using ScheduleOne.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Equipping
{
	public class AvatarWeapon : AvatarEquippable
	{
		[Header("Range settings")]
		public float MinUseRange;

		public float MaxUseRange = 1f;

		[Header("Cooldown settings")]
		public float CooldownDuration = 1f;

		[Header("Equipping")]
		public AudioClip[] EquipClips;

		public AudioSourceController EquipSound;

		public UnityEvent onSuccessfulHit;

		public float LastUseTime { get; private set; }

		public override void Equip(Avatar _avatar)
		{
			base.Equip(_avatar);
			if (EquipClips.Length != 0 && EquipSound != null)
			{
				EquipSound.AudioSource.clip = EquipClips[Random.Range(0, EquipClips.Length)];
				EquipSound.Play();
			}
		}

		public virtual void Attack()
		{
			LastUseTime = Time.time;
		}

		public virtual bool IsReadyToAttack()
		{
			return Time.time - LastUseTime > CooldownDuration;
		}
	}
}
