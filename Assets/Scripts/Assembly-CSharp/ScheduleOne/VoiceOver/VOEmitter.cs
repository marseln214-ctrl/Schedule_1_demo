using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.VoiceOver
{
	[RequireComponent(typeof(AudioSourceController))]
	public class VOEmitter : MonoBehaviour
	{
		public const float PitchVariation = 0.05f;

		public VODatabase Database;

		[Range(0.5f, 2f)]
		public float PitchMultiplier = 1f;

		protected AudioSourceController audioSourceController;

		protected virtual void Awake()
		{
			audioSourceController = GetComponent<AudioSourceController>();
		}

		public virtual void Play(EVOLineType lineType)
		{
			AudioClip randomClip = Database.GetRandomClip(lineType);
			if (randomClip == null)
			{
				Console.LogError("No clip found for line type: " + lineType);
				return;
			}
			audioSourceController.AudioSource.clip = randomClip;
			audioSourceController.VolumeMultiplier = Database.VolumeMultiplier * Database.GetEntry(lineType).VolumeMultiplier;
			audioSourceController.PitchMultiplier = PitchMultiplier + Random.Range(-0.05f, 0.05f);
			audioSourceController.Play();
		}
	}
}
