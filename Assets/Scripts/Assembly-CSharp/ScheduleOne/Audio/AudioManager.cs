using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Audio
{
	public class AudioManager : PersistentSingleton<AudioManager>
	{
		[Range(0f, 2f)]
		[SerializeField]
		protected float masterVolume = 1f;

		[Range(0f, 2f)]
		[SerializeField]
		protected float ambientVolume = 1f;

		[Range(0f, 2f)]
		[SerializeField]
		protected float footstepsVolume = 1f;

		[Range(0f, 2f)]
		[SerializeField]
		protected float fxVolume = 1f;

		[Range(0f, 2f)]
		[SerializeField]
		protected float uiVolume = 1f;

		[Range(0f, 2f)]
		[SerializeField]
		protected float musicVolume = 1f;

		[Range(0f, 2f)]
		[SerializeField]
		protected float voiceVolume = 1f;

		public UnityEvent onSettingsChanged = new UnityEvent();

		[Header("Generic Door Sounds")]
		public AudioSourceController DoorOpen;

		public AudioSourceController DoorClose;

		[Header("Mixers")]
		public AudioMixerGroup MainGameMixer;

		public AudioMixerGroup MenuMixer;

		public AudioMixerGroup MusicMixer;

		private float currentGameVolume = 1f;

		private const float minGameVolume = 0.0001f;

		private const float maxGameVolume = 0.0001f;

		private float gameVolumeMultiplier = 1f;

		public AudioMixerSnapshot DefaultSnapshot;

		public AudioMixerSnapshot DistortedSnapshot;

		public float MasterVolume => masterVolume;

		public float AmbientVolume => ambientVolume * masterVolume;

		public float UnscaledAmbientVolume => ambientVolume;

		public float FootstepsVolume => footstepsVolume * masterVolume;

		public float UnscaledFootstepsVolume => footstepsVolume;

		public float FXVolume => fxVolume * masterVolume;

		public float UnscaledFXVolume => fxVolume;

		public float UIVolume => uiVolume * masterVolume;

		public float UnscaledUIVolume => uiVolume;

		public float MusicVolume => musicVolume * masterVolume;

		public float UnscaledMusicVolume => musicVolume;

		public float VoiceVolume => voiceVolume * masterVolume * 0.6f;

		public float UnscaledVoiceVolume => voiceVolume;

		protected override void Awake()
		{
			base.Awake();
			SetGameVolume(0f);
		}

		protected override void Start()
		{
			base.Start();
			Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(delegate
			{
				SetDistorted(distorted: false, 0.5f);
			});
		}

		protected void Update()
		{
			if (SceneManager.GetActiveScene().name == "Main" && !Singleton<LoadingScreen>.Instance.IsOpen)
			{
				if (currentGameVolume < 1f)
				{
					SetGameVolume(currentGameVolume + Time.deltaTime * 1f);
				}
			}
			else if (currentGameVolume > 0f)
			{
				SetGameVolume(currentGameVolume - Time.deltaTime * 1f);
			}
		}

		public void SetGameVolumeMultipler(float value)
		{
			gameVolumeMultiplier = value;
			SetGameVolume(currentGameVolume);
		}

		public void SetDistorted(bool distorted, float transition = 5f)
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

		private void SetGameVolume(float value)
		{
			currentGameVolume = value;
			value = Mathf.Lerp(value * gameVolumeMultiplier, 0.0001f, 0.0001f);
			MainGameMixer.audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20f);
		}

		public float GetVolume(EAudioType audioType, bool scaled = true)
		{
			switch (audioType)
			{
			case EAudioType.Ambient:
				if (!scaled)
				{
					return UnscaledAmbientVolume;
				}
				return AmbientVolume;
			case EAudioType.Footsteps:
				if (!scaled)
				{
					return UnscaledFootstepsVolume;
				}
				return FootstepsVolume;
			case EAudioType.FX:
				if (!scaled)
				{
					return UnscaledFXVolume;
				}
				return FXVolume;
			case EAudioType.UI:
				if (!scaled)
				{
					return UnscaledUIVolume;
				}
				return UIVolume;
			case EAudioType.Music:
				if (!scaled)
				{
					return UnscaledMusicVolume;
				}
				return MusicVolume;
			case EAudioType.Voice:
				if (!scaled)
				{
					return UnscaledVoiceVolume;
				}
				return VoiceVolume;
			default:
				return 1f;
			}
		}

		public void SetMasterVolume(float volume)
		{
			masterVolume = volume;
		}

		public void SetVolume(EAudioType type, float volume)
		{
			switch (type)
			{
			case EAudioType.Ambient:
				ambientVolume = volume;
				break;
			case EAudioType.Footsteps:
				footstepsVolume = volume;
				break;
			case EAudioType.FX:
				fxVolume = volume;
				break;
			case EAudioType.UI:
				uiVolume = volume;
				break;
			case EAudioType.Music:
				musicVolume = volume;
				break;
			case EAudioType.Voice:
				voiceVolume = volume;
				break;
			}
		}
	}
}
