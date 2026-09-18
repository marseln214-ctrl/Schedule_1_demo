using UnityEngine;

namespace ScheduleOne.Audio
{
	public class RandomizedAudioSourceController : AudioSourceController
	{
		public AudioClip[] Clips;

		public override void Play()
		{
			if (Clips.Length == 0)
			{
				Console.LogWarning("RandomizedAudioSourceController: No clips to play");
				return;
			}
			int num = Random.Range(0, Clips.Length);
			AudioSource.clip = Clips[num];
			base.Play();
		}

		public override void PlayOneShot(bool duplicateAudioSource = false)
		{
			if (Clips.Length == 0)
			{
				Console.LogWarning("RandomizedAudioSourceController: No clips to play");
				return;
			}
			int num = Random.Range(0, Clips.Length);
			AudioSource.clip = Clips[num];
			base.PlayOneShot(duplicateAudioSource);
		}
	}
}
