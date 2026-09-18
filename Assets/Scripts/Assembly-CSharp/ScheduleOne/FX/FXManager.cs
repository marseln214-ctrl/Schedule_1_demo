using System.Collections;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.FX
{
	public class FXManager : Singleton<FXManager>
	{
		public AudioClip[] PunchImpactsClips;

		[Header("References")]
		public AudioSourceController[] ImpactSources;

		[Header("Particle Prefabs")]
		public GameObject PunchParticlePrefab;

		[Header("Trails")]
		public TrailRenderer BulletTrail;

		protected override void Start()
		{
			base.Start();
		}

		public void CreateImpactFX(Impact impact)
		{
			Console.Log("Creating impact FX");
			AudioClip impactSound = GetImpactSound(impact);
			if (impactSound != null)
			{
				PlayImpact(impactSound, impact.HitPoint, Mathf.Clamp01(impact.ImpactForce / 400f));
			}
			GameObject impactParticles = GetImpactParticles(impact);
			if (impactParticles != null)
			{
				PlayParticles(impactParticles, impact.HitPoint, Quaternion.LookRotation(impact.HitPoint));
			}
		}

		public void CreateBulletTrail(Vector3 start, Vector3 dir, float speed, float range, LayerMask mask)
		{
			TrailRenderer trail = Object.Instantiate(BulletTrail, NetworkSingleton<GameManager>.Instance.Temp);
			trail.transform.position = start;
			trail.transform.forward = dir;
			float maxDistance = range;
			if (Physics.Raycast(start, dir, out var hitInfo, range, mask))
			{
				maxDistance = hitInfo.distance;
			}
			Debug.DrawRay(start, dir * maxDistance, Color.red, 5f);
			StartCoroutine(Routine());
			IEnumerator Routine()
			{
				yield return new WaitForEndOfFrame();
				trail.transform.position = start + trail.transform.forward * maxDistance;
				yield return new WaitForSeconds(1f);
				Object.Destroy(trail.gameObject);
			}
		}

		private void PlayImpact(AudioClip clip, Vector3 position, float volume)
		{
			AudioSourceController source = GetSource();
			if (source == null)
			{
				Console.LogWarning("No available audio source controller found");
				return;
			}
			source.transform.position = position;
			source.AudioSource.clip = clip;
			source.VolumeMultiplier = volume;
			source.AudioSource.Play();
		}

		private void PlayParticles(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			Object.Destroy(Object.Instantiate(prefab, position, rotation), 2f);
		}

		private AudioClip GetImpactSound(Impact impact)
		{
			if (impact.Hit.collider.GetComponentInParent<NPC>() != null)
			{
				return GetRandomClip(PunchImpactsClips);
			}
			return null;
		}

		private GameObject GetImpactParticles(Impact impact)
		{
			if (impact.Hit.collider.GetComponentInParent<NPC>() != null)
			{
				return PunchParticlePrefab;
			}
			return null;
		}

		private AudioSourceController GetSource()
		{
			return ImpactSources.FirstOrDefault((AudioSourceController x) => !x.isPlaying);
		}

		private static AudioClip GetRandomClip(AudioClip[] clips)
		{
			return clips[Random.Range(0, clips.Length)];
		}
	}
}
