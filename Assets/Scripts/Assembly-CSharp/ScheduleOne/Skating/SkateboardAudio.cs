using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Skating
{
	public class SkateboardAudio : MonoBehaviour
	{
		public Skateboard Board;

		[Header("References")]
		public AudioSourceController JumpAudio;

		public AudioSourceController LandAudio;

		public AudioSourceController RollingAudio;

		public AudioSourceController WindAudio;

		private void Awake()
		{
			Board.OnJump.AddListener(PlayJump);
			Board.OnLand.AddListener(PlayLand);
		}

		private void Start()
		{
			if (Board.IsGrounded())
			{
				PlayLand();
			}
			RollingAudio.VolumeMultiplier = 0f;
			RollingAudio.Play();
			WindAudio.VolumeMultiplier = 0f;
			WindAudio.Play();
		}

		private void Update()
		{
			float num = Mathf.Clamp(Mathf.Abs(Board.CurrentSpeed_Kmh) / Board.TopSpeed_Kmh, 0f, 1.5f);
			float volumeMultiplier = num;
			if (Board.AirTime > 0.2f)
			{
				volumeMultiplier = 0f;
			}
			RollingAudio.VolumeMultiplier = volumeMultiplier;
			RollingAudio.AudioSource.pitch = Mathf.Lerp(0.75f, 1f, num);
			if (Board.IsOwner)
			{
				WindAudio.VolumeMultiplier = num;
				WindAudio.AudioSource.pitch = Mathf.Lerp(1.2f, 1.5f, num);
			}
			else
			{
				WindAudio.VolumeMultiplier = 0f;
			}
		}

		public void PlayJump(float force)
		{
			JumpAudio.VolumeMultiplier = Mathf.Lerp(0.5f, 1f, force);
			JumpAudio.Play();
		}

		public void PlayLand()
		{
			LandAudio.Play();
		}
	}
}
