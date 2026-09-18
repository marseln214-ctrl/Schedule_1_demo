using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class StartLoopMusicTrack : MusicTrack
	{
		public AudioSourceController LoopSound;

		protected override void Awake()
		{
			base.Awake();
			AutoFadeOut = false;
			LoopSound.AudioSource.loop = true;
		}

		public override void Update()
		{
			base.Update();
			if (base.IsPlaying)
			{
				if (!Controller.AudioSource.isPlaying && !LoopSound.isPlaying)
				{
					LoopSound.Play();
				}
				LoopSound.VolumeMultiplier = volumeMultiplier * VolumeMultiplier;
			}
			else
			{
				LoopSound.VolumeMultiplier = volumeMultiplier * VolumeMultiplier;
				if (LoopSound.VolumeMultiplier == 0f)
				{
					LoopSound.AudioSource.Stop();
				}
			}
		}

		public override void Play()
		{
			base.Play();
			Singleton<CoroutineService>.Instance.StartCoroutine(WaitForStart());
			IEnumerator WaitForStart()
			{
				while (true)
				{
					if (!base.IsPlaying)
					{
						yield break;
					}
					if (Controller.AudioSource.clip.length - Controller.AudioSource.time <= Time.deltaTime)
					{
						break;
					}
					yield return new WaitForEndOfFrame();
				}
				Console.Log("Starting loop for " + TrackName);
				LoopSound.Play();
			}
		}
	}
}
