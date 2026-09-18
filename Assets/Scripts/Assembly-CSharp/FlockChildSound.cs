using ScheduleOne.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlockChildSound : MonoBehaviour
{
	public AudioSourceController controller;

	public AudioClip[] _idleSounds;

	public float _idleSoundRandomChance = 0.05f;

	public AudioClip[] _flightSounds;

	public float _flightSoundRandomChance = 0.05f;

	public AudioClip[] _scareSounds;

	public float _pitchMin = 0.85f;

	public float _pitchMax = 1f;

	public float _volumeMin = 0.6f;

	public float _volumeMax = 0.8f;

	private FlockChild _flockChild;

	private AudioSource _audio;

	private bool _hasLanded;

	public void Start()
	{
		_flockChild = GetComponent<FlockChild>();
		_audio = GetComponent<AudioSource>();
		InvokeRepeating("PlayRandomSound", Random.value + 1f, 1f);
		if (_scareSounds.Length != 0)
		{
			InvokeRepeating("ScareSound", 1f, 0.01f);
		}
	}

	public void PlayRandomSound()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!_audio.isPlaying && _flightSounds.Length != 0 && _flightSoundRandomChance > Random.value && !_flockChild._landing)
		{
			if (controller != null)
			{
				controller.Play();
			}
		}
		else if (!_audio.isPlaying && _idleSounds.Length != 0 && _idleSoundRandomChance > Random.value && _flockChild._landing && controller != null)
		{
			controller.Play();
		}
	}

	public void ScareSound()
	{
		if (base.gameObject.activeInHierarchy && _hasLanded && !_flockChild._landing && _idleSoundRandomChance * 2f > Random.value)
		{
			_audio.clip = _scareSounds[Random.Range(0, _scareSounds.Length)];
			_audio.volume = Random.Range(_volumeMin, _volumeMax);
			_audio.PlayDelayed(Random.value * 0.2f);
			_hasLanded = false;
		}
	}
}
