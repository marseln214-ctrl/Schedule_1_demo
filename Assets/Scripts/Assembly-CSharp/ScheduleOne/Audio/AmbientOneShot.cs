using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class AmbientOneShot : MonoBehaviour
	{
		public enum EPlayTime
		{
			All = 0,
			Day = 1,
			Night = 2
		}

		public AudioSourceController Audio;

		[Header("Settings")]
		[Range(0f, 1f)]
		public float Volume = 0.2f;

		[Range(0f, 1f)]
		public float ChancePerHour = 0.2f;

		public int CooldownTime = 60;

		public EPlayTime PlayTime;

		public float MinDistance = 20f;

		public float MaxDistance = 100f;

		private int timeSinceLastPlay;

		private void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(base.transform.position, MinDistance);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, MaxDistance);
		}

		private void MinPass()
		{
			timeSinceLastPlay++;
			if (timeSinceLastPlay >= CooldownTime && !NetworkSingleton<TimeManager>.Instance.SleepInProgress && (PlayTime != EPlayTime.Day || !NetworkSingleton<TimeManager>.Instance.IsNight) && (PlayTime != EPlayTime.Night || NetworkSingleton<TimeManager>.Instance.IsNight))
			{
				float num = Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
				if (!(num < MinDistance) && !(num > MaxDistance) && UnityEngine.Random.value < ChancePerHour / 60f)
				{
					Play();
				}
			}
		}

		private void Play()
		{
			timeSinceLastPlay = 0;
			Audio.SetVolume(Volume);
			Audio.Play();
		}
	}
}
