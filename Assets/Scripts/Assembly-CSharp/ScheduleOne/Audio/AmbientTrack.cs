using System.Collections.Generic;
using EasyButtons;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Audio
{
	public class AmbientTrack : MonoBehaviour
	{
		public const float MIN_TIME_BETWEEN_AMBIENT_TRACKS = 540f;

		public static AmbientTrack LastPlayedTrack;

		public static bool TrackQueued;

		public List<MusicTrack> Tracks = new List<MusicTrack>();

		public int MinTime;

		public int MaxTime;

		public float Chance = 0.3f;

		private int startTime;

		private bool playTrack;

		private bool trackRandomized;

		private void Awake()
		{
			for (int i = 0; i < Tracks.Count; i++)
			{
				int index = Random.Range(i, Tracks.Count);
				MusicTrack value = Tracks[index];
				Tracks[index] = Tracks[i];
				Tracks[i] = value;
			}
		}

		[Button]
		public void ForcePlay()
		{
			LastPlayedTrack = this;
			MusicPlayer.TimeSinceLastAmbientTrack = 0f;
			playTrack = false;
			TrackQueued = false;
			Tracks[0].Enable();
			Tracks.Add(Tracks[0]);
			Tracks.RemoveAt(0);
		}

		public void Stop()
		{
			Tracks[0].Disable();
			Tracks[0].Stop();
		}

		private void Update()
		{
			if (!NetworkSingleton<TimeManager>.InstanceExists)
			{
				trackRandomized = false;
				TrackQueued = false;
				return;
			}
			int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
			if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(MinTime, MaxTime))
			{
				if (!trackRandomized)
				{
					playTrack = Random.value < Chance && MusicPlayer.TimeSinceLastAmbientTrack > 540f && LastPlayedTrack != this && !TrackQueued && Tracks.Count > 0 && Time.timeSinceLevelLoad > 20f;
					startTime = TimeManager.AddMinutesTo24HourTime(currentTime, Random.Range(0, 120));
					if (playTrack)
					{
						Console.Log("Will play " + Tracks[0].TrackName + " at " + startTime);
						TrackQueued = true;
						MusicPlayer.TimeSinceLastAmbientTrack = 0f;
					}
					trackRandomized = true;
				}
				if (playTrack && !Tracks[0].Enabled && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(startTime, MaxTime))
				{
					LastPlayedTrack = this;
					MusicPlayer.TimeSinceLastAmbientTrack = 0f;
					playTrack = false;
					TrackQueued = false;
					Tracks[0].Enable();
					Tracks.Add(Tracks[0]);
					Tracks.RemoveAt(0);
				}
				return;
			}
			trackRandomized = false;
			playTrack = false;
			foreach (MusicTrack track in Tracks)
			{
				track.Disable();
			}
		}
	}
}
