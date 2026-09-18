using UnityEngine;

namespace ScheduleOne.Audio
{
	[RequireComponent(typeof(AudioSourceController))]
	public class MusicTrack : MonoBehaviour
	{
		public bool Enabled;

		public string TrackName = "Track";

		public int Priority = 1;

		public float FadeInTime = 1f;

		public float FadeOutTime = 2f;

		public AudioSourceController Controller;

		public float VolumeMultiplier = 1f;

		public bool AutoFadeOut = true;

		protected float volumeMultiplier = 1f;

		public bool IsPlaying { get; private set; }

		private void OnValidate()
		{
			base.gameObject.name = TrackName + " (" + Priority + ")";
		}

		public void Enable()
		{
			Enabled = true;
		}

		public void Disable()
		{
			Enabled = false;
		}

		protected virtual void Awake()
		{
			volumeMultiplier = 0f;
		}

		public virtual void Update()
		{
			if (IsPlaying && Controller.AudioSource.time >= Controller.AudioSource.clip.length - FadeOutTime && AutoFadeOut)
			{
				Stop();
				Disable();
			}
			if (IsPlaying)
			{
				volumeMultiplier = Mathf.Min(volumeMultiplier + Time.deltaTime / FadeInTime, 1f);
				Controller.VolumeMultiplier = volumeMultiplier * VolumeMultiplier;
				return;
			}
			volumeMultiplier = Mathf.Max(volumeMultiplier - Time.deltaTime / FadeOutTime, 0f);
			Controller.VolumeMultiplier = volumeMultiplier * VolumeMultiplier;
			if (Controller.VolumeMultiplier == 0f)
			{
				Controller.AudioSource.Stop();
			}
		}

		public virtual void Play()
		{
			IsPlaying = true;
			Controller.Play();
		}

		public virtual void Stop()
		{
			IsPlaying = false;
		}
	}
}
