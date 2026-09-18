using System;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.Tools
{
	[ExecuteInEditMode]
	public class WheelRotator : MonoBehaviour
	{
		public float Radius = 0.5f;

		public Transform Wheel;

		public bool Flip;

		public AudioSourceController Controller;

		public float AudioVolumeDivisor = 90f;

		public Vector3 RotationAxis = Vector3.up;

		[SerializeField]
		private Vector3 lastFramePosition = Vector3.zero;

		private void Start()
		{
			if (Controller != null)
			{
				Controller.AudioSource.time = UnityEngine.Random.Range(0f, Controller.AudioSource.clip.length);
			}
		}

		private void LateUpdate()
		{
			Vector3 position = base.transform.position;
			float num = Vector3.Distance(position, lastFramePosition);
			if (num > 0f)
			{
				float num2 = num / (MathF.PI * 2f * Radius) * 360f;
				Wheel.Rotate(RotationAxis, num2 * (Flip ? (-1f) : 1f));
				float num3 = num2 / Time.deltaTime;
				if (Controller != null)
				{
					Controller.VolumeMultiplier = num3 / AudioVolumeDivisor;
				}
			}
			lastFramePosition = position;
		}
	}
}
