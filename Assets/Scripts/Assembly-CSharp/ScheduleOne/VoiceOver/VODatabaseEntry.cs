using System;
using UnityEngine;

namespace ScheduleOne.VoiceOver
{
	[Serializable]
	public class VODatabaseEntry
	{
		public EVOLineType LineType;

		public AudioClip[] Clips;

		private AudioClip lastClip;

		public float VolumeMultiplier = 1f;

		public AudioClip GetRandomClip()
		{
			if (Clips.Length == 0)
			{
				return null;
			}
			AudioClip audioClip = Clips[UnityEngine.Random.Range(0, Clips.Length)];
			int num = 0;
			while (audioClip == lastClip && Clips.Length != 1 && num <= 5)
			{
				audioClip = Clips[UnityEngine.Random.Range(0, Clips.Length)];
				num++;
			}
			lastClip = audioClip;
			return audioClip;
		}
	}
}
