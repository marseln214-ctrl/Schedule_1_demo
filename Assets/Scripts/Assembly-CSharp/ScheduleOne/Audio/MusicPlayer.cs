using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Audio;

namespace ScheduleOne.Audio
{
	public class MusicPlayer : PersistentSingleton<MusicPlayer>
	{
		public static float TimeSinceLastAmbientTrack = 100000f;

		public List<MusicTrack> Tracks = new List<MusicTrack>();

		public AudioMixerGroup MusicMixer;

		public AudioMixerSnapshot DefaultSnapshot;

		public AudioMixerSnapshot DistortedSnapshot;

		private MusicTrack _currentTrack;

		public bool IsPlaying
		{
			get
			{
				if (_currentTrack != null)
				{
					return _currentTrack.IsPlaying;
				}
				return false;
			}
		}

		public void OnValidate()
		{
			Tracks = new List<MusicTrack>(GetComponentsInChildren<MusicTrack>());
			for (int i = 0; i < Tracks.Count - 1; i++)
			{
				if (Tracks[i].Priority > Tracks[i + 1].Priority)
				{
					Tracks[i].transform.SetSiblingIndex(i + 1);
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			InvokeRepeating("UpdateTracks", 0f, 0.2f);
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(delegate
			{
				SetMusicDistorted(distorted: false, 0.5f);
			});
			DefaultSnapshot.TransitionTo(0.1f);
		}

		private void Update()
		{
			TimeSinceLastAmbientTrack += Time.unscaledDeltaTime;
		}

		public void SetMusicDistorted(bool distorted, float transition = 5f)
		{
			if (distorted)
			{
				DistortedSnapshot.TransitionTo(transition);
			}
			else
			{
				DefaultSnapshot.TransitionTo(transition);
			}
		}

		public void SetTrackEnabled(string trackName, bool enabled)
		{
			MusicTrack musicTrack = Tracks.Find((MusicTrack t) => t.TrackName == trackName);
			if (musicTrack == null)
			{
				Console.LogWarning("Music track not found: " + trackName);
			}
			else if (enabled)
			{
				musicTrack.Enable();
			}
			else
			{
				musicTrack.Disable();
			}
		}

		public void StopTrack(string trackName)
		{
			MusicTrack musicTrack = Tracks.Find((MusicTrack t) => t.TrackName == trackName);
			if (musicTrack == null)
			{
				Console.LogWarning("Music track not found: " + trackName);
			}
			else
			{
				musicTrack.Stop();
			}
		}

		public void StopAndDisableTracks()
		{
			foreach (MusicTrack track in Tracks)
			{
				track.Disable();
				track.Stop();
			}
		}

		private void UpdateTracks()
		{
			if (_currentTrack != null && !_currentTrack.IsPlaying)
			{
				_currentTrack = null;
			}
			MusicTrack musicTrack = null;
			foreach (MusicTrack track in Tracks)
			{
				if (track.Enabled && (musicTrack == null || track.Priority > musicTrack.Priority))
				{
					musicTrack = track;
				}
			}
			if (_currentTrack != musicTrack && musicTrack != null)
			{
				if (_currentTrack != null)
				{
					_currentTrack.Stop();
				}
				_currentTrack = musicTrack;
				if (_currentTrack != null)
				{
					_currentTrack.Play();
				}
			}
		}
	}
}
