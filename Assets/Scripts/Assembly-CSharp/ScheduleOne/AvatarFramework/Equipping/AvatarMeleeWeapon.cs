using System;
using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping
{
	public class AvatarMeleeWeapon : AvatarWeapon
	{
		[Serializable]
		public class MeleeAttack
		{
			public float RangeMultiplier = 1f;

			public float DamageMultiplier = 1f;

			public string AnimationTrigger = string.Empty;

			public float DamageDelay = 0.4f;

			public float AttackSoundDelay;

			public AudioClip[] AttackClips;

			public AudioClip[] HitClips;
		}

		public const float GruntChance = 0.4f;

		[Header("References")]
		public AudioSourceController AttackSound;

		public AudioSourceController HitSound;

		[Header("Melee Weapon settings")]
		public float AttackRange = 1.5f;

		public float AttackRadius = 0.25f;

		public float Damage = 25f;

		public MeleeAttack[] Attacks;

		private Coroutine attackRoutine;

		public override void Unequip()
		{
			if (attackRoutine != null)
			{
				StopCoroutine(attackRoutine);
				attackRoutine = null;
			}
			base.Unequip();
		}

		public override void Attack()
		{
			base.Attack();
			MeleeAttack attack = Attacks[UnityEngine.Random.Range(0, Attacks.Length)];
			NPC npc = avatar.GetComponentInParent<NPC>();
			avatar.Anim.ResetTrigger(attack.AnimationTrigger);
			avatar.Anim.SetTrigger(attack.AnimationTrigger);
			attackRoutine = StartCoroutine(AttackRoutine());
			IEnumerator AttackRoutine()
			{
				yield return new WaitForSeconds(attack.AttackSoundDelay);
				if (attack.AttackClips.Length != 0)
				{
					AttackSound.AudioSource.clip = attack.AttackClips[UnityEngine.Random.Range(0, attack.AttackClips.Length)];
					AttackSound.Play();
				}
				if (UnityEngine.Random.value < 0.4f && npc != null)
				{
					npc.PlayVO(EVOLineType.Grunt);
				}
				yield return new WaitForSeconds(attack.DamageDelay - attack.AttackSoundDelay);
				Vector3 centerPoint = avatar.CenterPoint;
				Vector3 forward = avatar.transform.forward;
				if (Physics.Raycast(centerPoint, forward, out var hitInfo, AttackRange, 1 << LayerMask.NameToLayer("Player")))
				{
					Player componentInParent = hitInfo.collider.GetComponentInParent<Player>();
					if (componentInParent != null)
					{
						componentInParent.Health.TakeDamage(Damage * attack.DamageMultiplier);
						if (attack.HitClips.Length != 0)
						{
							HitSound.AudioSource.clip = attack.HitClips[UnityEngine.Random.Range(0, attack.HitClips.Length)];
							HitSound.transform.position = hitInfo.point;
							HitSound.PlayOneShot(duplicateAudioSource: true);
						}
					}
				}
			}
		}
	}
}
