using EasyButtons;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class StartLoopStopAudio : MonoBehaviour
	{
		public AudioSourceController StartSound;

		public AudioSourceController LoopSound;

		public AudioSourceController StopSound;

		public bool FadeLoopIn;

		public bool FadeLoopOut;

		private float timeSinceStart;

		private float timeSinceStop;

		public bool Runnning { get; private set; }

		private void Update()
		{
			if (Runnning)
			{
				timeSinceStart += Time.deltaTime;
				if (FadeLoopIn)
				{
					LoopSound.VolumeMultiplier = Mathf.Lerp(0f, 1f, timeSinceStart / StartSound.AudioSource.clip.length);
				}
				else
				{
					LoopSound.VolumeMultiplier = 1f;
				}
				return;
			}
			timeSinceStop += Time.deltaTime;
			if (FadeLoopOut)
			{
				LoopSound.VolumeMultiplier = Mathf.Lerp(1f, 0f, timeSinceStop / StopSound.AudioSource.clip.length);
			}
			else
			{
				LoopSound.VolumeMultiplier = 0f;
			}
			if (LoopSound.isPlaying && LoopSound.VolumeMultiplier == 0f)
			{
				LoopSound.Stop();
			}
		}

		[Button]
		public void StartAudio()
		{
			if (!Runnning)
			{
				Runnning = true;
				timeSinceStart = 0f;
				LoopSound.Play();
				LoopSound.AudioSource.loop = true;
				StartSound.Play();
			}
		}

		[Button]
		public void StopAudio()
		{
			if (Runnning)
			{
				Runnning = false;
				timeSinceStop = 0f;
				StartSound.Stop();
				StopSound.Play();
			}
		}
	}
}
