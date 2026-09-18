using System.Collections;
using ScheduleOne.Audio;
using UnityEngine;

namespace ScheduleOne.VoiceOver
{
	public class PoliceChatterVO : VOEmitter
	{
		public AudioSourceController StartBeep;

		public AudioSourceController StartEndBeep;

		public AudioSourceController Static;

		private Coroutine chatterRoutine;

		public override void Play(EVOLineType lineType)
		{
			if (lineType == EVOLineType.PoliceChatter)
			{
				PlayChatter();
			}
			else
			{
				base.Play(lineType);
			}
		}

		private void PlayChatter()
		{
			if (chatterRoutine != null)
			{
				StopCoroutine(chatterRoutine);
			}
			chatterRoutine = StartCoroutine(Play());
			IEnumerator Play()
			{
				StartBeep.Play();
				Static.Play();
				yield return new WaitForSeconds(0.25f);
				base.Play(EVOLineType.PoliceChatter);
				yield return new WaitForSeconds(0.1f);
				yield return new WaitUntil(() => !audioSourceController.isPlaying);
				StartEndBeep.Play();
				Static.Stop();
				chatterRoutine = null;
			}
		}
	}
}
