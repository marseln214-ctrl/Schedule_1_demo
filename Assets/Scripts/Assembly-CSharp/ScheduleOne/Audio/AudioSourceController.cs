using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Audio
{
	[RequireComponent(typeof(AudioSource))]
	public class AudioSourceController : MonoBehaviour
	{
		public bool DEBUG;

		public AudioSource AudioSource;

		[Header("Settings")]
		public EAudioType AudioType;

		[Range(0f, 1f)]
		public float DefaultVolume = 1f;

		public bool RandomizePitch;

		public float MinPitch = 0.9f;

		public float MaxPitch = 1.1f;

		[Range(0f, 2f)]
		public float VolumeMultiplier = 1f;

		[Range(0f, 2f)]
		public float PitchMultiplier = 1f;

		private bool paused;

		private bool isPlayingCached;

		private float basePitch = 1f;

		public float Volume { get; protected set; } = 1f;


		public bool isPlaying => AudioSource.isPlaying;

		private void Awake()
		{
			DoPauseStuff();
			basePitch = AudioSource.pitch;
			AudioSource.volume = 0f;
			if (AudioSource.playOnAwake)
			{
				isPlayingCached = true;
			}
		}

		private void Start()
		{
			SetVolume(DefaultVolume);
			Singleton<AudioManager>.Instance.onSettingsChanged.AddListener(ApplyVolume);
			if (AudioType != EAudioType.Music)
			{
				if (SceneManager.GetActiveScene().name == "Main")
				{
					AudioSource.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MainGameMixer;
				}
				else
				{
					AudioSource.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MenuMixer;
				}
			}
			else
			{
				AudioSource.outputAudioMixerGroup = Singleton<AudioManager>.Instance.MusicMixer;
			}
		}

		private void DoPauseStuff()
		{
			if (Singleton<PauseMenu>.InstanceExists)
			{
				Singleton<PauseMenu>.Instance.onPause.RemoveListener(Pause);
				Singleton<PauseMenu>.Instance.onPause.AddListener(Pause);
				Singleton<PauseMenu>.Instance.onResume.RemoveListener(Unpause);
				Singleton<PauseMenu>.Instance.onResume.AddListener(Pause);
			}
		}

		private void OnDestroy()
		{
			if (Singleton<AudioManager>.Instance != null)
			{
				Singleton<AudioManager>.Instance.onSettingsChanged.RemoveListener(ApplyVolume);
			}
		}

		private void OnValidate()
		{
			if (AudioSource == null)
			{
				AudioSource = GetComponent<AudioSource>();
			}
		}

		private void FixedUpdate()
		{
			if (isPlayingCached)
			{
				ApplyVolume();
				if (!AudioSource.isPlaying && !paused)
				{
					isPlayingCached = false;
				}
			}
		}

		private void Pause()
		{
			paused = true;
			AudioSource.Pause();
		}

		private void Unpause()
		{
			paused = false;
			AudioSource.UnPause();
		}

		public void SetVolume(float volume)
		{
			Volume = volume;
			ApplyVolume();
		}

		public void ApplyVolume()
		{
			if (Singleton<AudioManager>.InstanceExists)
			{
				if (DEBUG)
				{
					Debug.Log("Applying volume: " + Volume + " * " + Singleton<AudioManager>.Instance.GetVolume(AudioType) + " * " + VolumeMultiplier);
				}
				AudioSource.volume = Volume * Singleton<AudioManager>.Instance.GetVolume(AudioType) * VolumeMultiplier;
			}
		}

		public void ApplyPitch()
		{
			if (RandomizePitch)
			{
				AudioSource.pitch = Random.Range(MinPitch, MaxPitch) * PitchMultiplier;
			}
			else
			{
				AudioSource.pitch = basePitch * PitchMultiplier;
			}
		}

		public virtual void Play()
		{
			ApplyPitch();
			ApplyVolume();
			isPlayingCached = true;
			AudioSource.Play();
		}

		public virtual void PlayOneShot(bool duplicateAudioSource = false)
		{
			if (RandomizePitch)
			{
				AudioSource.pitch = Random.Range(MinPitch, MaxPitch) * PitchMultiplier;
			}
			ApplyVolume();
			if (duplicateAudioSource)
			{
				GameObject gameObject = Object.Instantiate(base.gameObject, NetworkSingleton<GameManager>.Instance.Temp);
				gameObject.transform.position = base.transform.position;
				gameObject.GetComponent<AudioSourceController>().PlayOneShot();
				if (AudioSource.clip != null)
				{
					Object.Destroy(gameObject, AudioSource.clip.length + 0.1f);
				}
				else
				{
					Object.Destroy(gameObject, 5f);
				}
			}
			else
			{
				AudioSource.PlayOneShot(AudioSource.clip, 1f);
			}
		}

		public void Stop()
		{
			AudioSource.Stop();
		}
	}
}
