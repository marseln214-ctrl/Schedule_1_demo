using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class SFXManager : Singleton<SFXManager>
	{
		[Serializable]
		public class ImpactType
		{
			public ImpactSoundEntity.EMaterial Material;

			public float MinVolume;

			public float MaxVolume;

			public float MinPitch;

			public float MaxPitch;

			public AudioClip[] Clips;
		}

		public const float MAX_PLAYER_DISTANCE = 40f;

		public const float SQR_MAX_PLAYER_DISTANCE = 1600f;

		public List<ImpactType> ImpactTypes = new List<ImpactType>();

		[SerializeField]
		private List<AudioSourceController> soundPool = new List<AudioSourceController>();

		private List<AudioSourceController> soundsInUse = new List<AudioSourceController>();

		public void PlayImpactSound(ImpactSoundEntity.EMaterial material, Vector3 position, float momentum)
		{
			if (!PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				return;
			}
			if (Vector3.Distance(position, PlayerSingleton<PlayerCamera>.Instance.transform.position) > 40f)
			{
				Console.LogWarning("Impact sound too far away");
				return;
			}
			ImpactType impactType = ImpactTypes.Find((ImpactType x) => x.Material == material);
			if (impactType == null)
			{
				Console.LogWarning("No impact type found for material: " + material);
				return;
			}
			AudioSourceController source = GetSource();
			if (source == null)
			{
				Console.LogWarning("No source available");
				return;
			}
			source.transform.position = position;
			float num = Mathf.Clamp01(momentum / 100f);
			source.PitchMultiplier = Mathf.Lerp(impactType.MaxPitch, impactType.MinPitch, num);
			source.VolumeMultiplier = Mathf.Lerp(impactType.MinVolume, impactType.MaxVolume, Mathf.Sqrt(num));
			source.AudioSource.clip = impactType.Clips[UnityEngine.Random.Range(0, impactType.Clips.Length)];
			source.Play();
			soundsInUse.Add(source);
			soundPool.Remove(source);
		}

		private void FixedUpdate()
		{
			for (int num = soundsInUse.Count - 1; num >= 0; num--)
			{
				if (!soundsInUse[num].isPlaying)
				{
					soundPool.Add(soundsInUse[num]);
					soundsInUse.RemoveAt(num);
				}
			}
		}

		private AudioSourceController GetSource()
		{
			if (soundPool.Count == 0)
			{
				Console.Log("No more sources available");
				return null;
			}
			return soundPool[0];
		}
	}
}
