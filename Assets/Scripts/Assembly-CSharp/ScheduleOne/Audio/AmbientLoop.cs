using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Audio
{
	[RequireComponent(typeof(AudioSourceController))]
	public class AmbientLoop : MonoBehaviour
	{
		public const float MUSIC_FADE_MULTIPLIER = 0.3f;

		public const float MUSIC_FADE_TIME = 4f;

		public AnimationCurve VolumeCurve;

		public bool FadeDuringMusic = true;

		private AudioSourceController audioSourceController;

		private float musicScale = 1f;

		private void Start()
		{
			audioSourceController = GetComponent<AudioSourceController>();
			audioSourceController.Play();
		}

		private void Update()
		{
			if (FadeDuringMusic)
			{
				if (Singleton<MusicPlayer>.Instance.IsPlaying)
				{
					musicScale = Mathf.Lerp(musicScale, 0.3f, Time.deltaTime / 4f);
				}
				else
				{
					musicScale = Mathf.Lerp(musicScale, 1f, Time.deltaTime / 4f);
				}
			}
			else
			{
				musicScale = 1f;
			}
			float num = VolumeCurve.Evaluate((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 1440f);
			audioSourceController.VolumeMultiplier = num * musicScale;
		}
	}
}
