using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class HeartbeatSoundController : MonoBehaviour
	{
		public AudioSourceController sound;

		public FloatSmoother VolumeController;

		public FloatSmoother PitchController;

		private void Awake()
		{
			VolumeController.Initialize();
			VolumeController.SetDefault(0f);
			PitchController.Initialize();
			PitchController.SetDefault(1f);
		}

		private void Update()
		{
			sound.VolumeMultiplier = VolumeController.CurrentValue;
			sound.PitchMultiplier = PitchController.CurrentValue;
			sound.ApplyPitch();
			if (sound.VolumeMultiplier > 0f)
			{
				if (!sound.isPlaying)
				{
					sound.Play();
				}
			}
			else if (sound.isPlaying)
			{
				sound.Stop();
			}
		}
	}
}
