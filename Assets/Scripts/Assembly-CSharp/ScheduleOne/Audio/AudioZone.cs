using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class AudioZone : Zone
	{
		[Serializable]
		public class Track
		{
			public AudioSourceController Source;

			[Range(0.01f, 2f)]
			public float Volume = 1f;

			public int StartTime;

			public int EndTime;

			public int FadeTime = 60;

			private float timeVolMultiplier;

			private int fadeInStart;

			private int fadeInEnd;

			private int fadeOutStart;

			private int fadeOutEnd;

			private int fadeInStartMinSum;

			private int fadeInEndMinSum;

			private int fadeOutStartMinSum;

			private int fadeOutEndMinSum;

			public void Init()
			{
				fadeInStart = TimeManager.AddMinutesTo24HourTime(StartTime, -FadeTime / 2);
				fadeInEnd = TimeManager.AddMinutesTo24HourTime(StartTime, FadeTime / 2);
				fadeOutStart = TimeManager.AddMinutesTo24HourTime(EndTime, -FadeTime / 2);
				fadeOutEnd = TimeManager.AddMinutesTo24HourTime(EndTime, FadeTime / 2);
				fadeInStartMinSum = TimeManager.GetMinSumFrom24HourTime(fadeInStart);
				fadeInEndMinSum = TimeManager.GetMinSumFrom24HourTime(fadeInEnd);
				fadeOutStartMinSum = TimeManager.GetMinSumFrom24HourTime(fadeOutStart);
				fadeOutEndMinSum = TimeManager.GetMinSumFrom24HourTime(fadeOutEnd);
			}

			public void Update(float multiplier)
			{
				float num = Volume * multiplier * timeVolMultiplier;
				Source.SetVolume(num);
				if (num > 0f)
				{
					if (!Source.isPlaying)
					{
						Source.Play();
					}
				}
				else if (Source.isPlaying)
				{
					Source.Stop();
				}
			}

			public void UpdateTimeMultiplier(int time)
			{
				int minSumFrom24HourTime = TimeManager.GetMinSumFrom24HourTime(time);
				if (TimeManager.IsGivenTimeWithinRange(time, fadeInEnd, fadeOutStart))
				{
					timeVolMultiplier = 1f;
				}
				else if (TimeManager.IsGivenTimeWithinRange(time, fadeInStart, fadeInEnd))
				{
					timeVolMultiplier = (float)(minSumFrom24HourTime - fadeInStartMinSum) / (float)(fadeInEndMinSum - fadeInStartMinSum);
				}
				else if (TimeManager.IsGivenTimeWithinRange(time, fadeOutStart, fadeOutEnd))
				{
					timeVolMultiplier = 1f - (float)(minSumFrom24HourTime - fadeOutStartMinSum) / (float)(fadeOutEndMinSum - fadeOutStartMinSum);
				}
				else
				{
					timeVolMultiplier = 0f;
				}
			}
		}

		public const float VOLUME_CHANGE_RATE = 1f;

		public const float ROLLOFF_SCALE = 0.5f;

		[Header("Settings")]
		[Range(1f, 200f)]
		public float MaxDistance = 100f;

		public List<Track> Tracks = new List<Track>();

		public Dictionary<AudioZoneModifierVolume, float> Modifiers = new Dictionary<AudioZoneModifierVolume, float>();

		protected float CurrentVolumeMultiplier = 1f;

		public float VolumeModifier { get; set; }

		private void Start()
		{
			foreach (Track track in Tracks)
			{
				track.Init();
			}
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void Update()
		{
			VolumeModifier = Mathf.MoveTowards(VolumeModifier, GetFalloffFactor(base.LocalPlayerDistance), 1f * Time.deltaTime);
			CurrentVolumeMultiplier = Mathf.MoveTowards(CurrentVolumeMultiplier, GetTotalVolumeMultiplier(), 1f * Time.deltaTime);
			foreach (Track track in Tracks)
			{
				track.Update(VolumeModifier * CurrentVolumeMultiplier);
			}
		}

		private float GetTotalVolumeMultiplier()
		{
			float num = 1f;
			foreach (KeyValuePair<AudioZoneModifierVolume, float> modifier in Modifiers)
			{
				num *= modifier.Value;
			}
			return num;
		}

		private void MinPass()
		{
			foreach (Track track in Tracks)
			{
				track.UpdateTimeMultiplier(NetworkSingleton<TimeManager>.Instance.CurrentTime);
			}
		}

		public void AddModifier(AudioZoneModifierVolume modifier, float value)
		{
			if (!Modifiers.ContainsKey(modifier))
			{
				Modifiers.Add(modifier, value);
			}
			Modifiers[modifier] = value;
		}

		public void RemoveModifier(AudioZoneModifierVolume modifier)
		{
			if (Modifiers.ContainsKey(modifier))
			{
				Modifiers.Remove(modifier);
			}
		}

		private float GetFalloffFactor(float distance)
		{
			if (distance > MaxDistance)
			{
				return 0f;
			}
			return 1f / (1f + 0.5f * distance);
		}
	}
}
