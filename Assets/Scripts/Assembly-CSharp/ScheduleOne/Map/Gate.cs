using EasyButtons;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class Gate : MonoBehaviour
	{
		public Transform Gate1;

		public Vector3 Gate1Open;

		public Vector3 Gate1Closed;

		public Transform Gate2;

		public Vector3 Gate2Open;

		public Vector3 Gate2Closed;

		public float OpenSpeed;

		public float Acceleration = 2f;

		[Header("Sound")]
		public AudioSourceController[] StartSounds;

		public AudioSourceController[] LoopSounds;

		public AudioSourceController[] StopSounds;

		private float Momentum;

		private float openDelta;

		public bool IsOpen { get; protected set; }

		private void Update()
		{
			Momentum = Mathf.MoveTowards(Momentum, 1f, Time.deltaTime * Acceleration);
			if (IsOpen)
			{
				openDelta += Time.deltaTime * OpenSpeed * Momentum;
			}
			else
			{
				openDelta -= Time.deltaTime * OpenSpeed * Momentum;
			}
			openDelta = Mathf.Clamp01(openDelta);
			if (openDelta <= 0.01f || openDelta >= 0.99f)
			{
				if (LoopSounds[0].isPlaying)
				{
					AudioSourceController[] loopSounds = LoopSounds;
					for (int i = 0; i < loopSounds.Length; i++)
					{
						loopSounds[i].Stop();
					}
					loopSounds = StopSounds;
					for (int i = 0; i < loopSounds.Length; i++)
					{
						loopSounds[i].Play();
					}
				}
			}
			else if (!LoopSounds[0].isPlaying && StartSounds[0].AudioSource.time >= StartSounds[0].AudioSource.clip.length * 0.5f)
			{
				AudioSourceController[] loopSounds = LoopSounds;
				for (int i = 0; i < loopSounds.Length; i++)
				{
					loopSounds[i].Play();
				}
			}
			Gate1.localPosition = Vector3.Lerp(Gate1Closed, Gate1Open, openDelta);
			Gate2.localPosition = Vector3.Lerp(Gate2Closed, Gate2Open, openDelta);
		}

		[Button]
		public void Open()
		{
			Momentum *= -1f;
			if (openDelta == 0f)
			{
				Momentum = 0f;
			}
			AudioSourceController[] startSounds = StartSounds;
			for (int i = 0; i < startSounds.Length; i++)
			{
				startSounds[i].Play();
			}
			IsOpen = true;
		}

		[Button]
		public void Close()
		{
			Momentum *= -1f;
			if (openDelta == 1f)
			{
				Momentum = 0f;
			}
			AudioSourceController[] startSounds = StartSounds;
			for (int i = 0; i < startSounds.Length; i++)
			{
				startSounds[i].Play();
			}
			IsOpen = false;
		}
	}
}
