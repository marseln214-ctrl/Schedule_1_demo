using System;
using EasyButtons;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.GameTime
{
	public class TutorialTimeController : MonoBehaviour
	{
		[Serializable]
		public struct KeyFrame
		{
			public int Time;

			public float SpeedMultiplier;

			public string Note;
		}

		public AnimationCurve TimeProgressionCurve;

		public KeyFrame[] KeyFrames;

		[SerializeField]
		private int currentKeyFrameIndex;

		private bool disabled;

		private void Awake()
		{
			TimeManager.onSleepStart = (Action)Delegate.Combine(TimeManager.onSleepStart, new Action(IncrementKeyframe));
		}

		private void OnDestroy()
		{
			TimeManager.onSleepStart = (Action)Delegate.Remove(TimeManager.onSleepStart, new Action(IncrementKeyframe));
		}

		private void Update()
		{
			if (!disabled)
			{
				KeyFrame keyFrame = KeyFrames[currentKeyFrameIndex];
				float time = Mathf.Clamp01(Mathf.InverseLerp(GetCurrentKeyFrameStart(), keyFrame.Time, NetworkSingleton<TimeManager>.Instance.CurrentTime));
				float timeProgressionMultiplier = TimeProgressionCurve.Evaluate(time) * keyFrame.SpeedMultiplier;
				NetworkSingleton<TimeManager>.Instance.TimeProgressionMultiplier = timeProgressionMultiplier;
			}
		}

		private int GetCurrentKeyFrameStart()
		{
			if (currentKeyFrameIndex > 0)
			{
				return KeyFrames[currentKeyFrameIndex - 1].Time;
			}
			return NetworkSingleton<TimeManager>.Instance.DefaultTime;
		}

		[Button]
		public void IncrementKeyframe()
		{
			Console.Log("Incrementing keyframe to " + (currentKeyFrameIndex + 1));
			currentKeyFrameIndex = Mathf.Clamp(currentKeyFrameIndex + 1, 0, KeyFrames.Length - 1);
		}

		public void Disable()
		{
			NetworkSingleton<TimeManager>.Instance.TimeProgressionMultiplier = 1f;
			base.enabled = false;
			disabled = true;
		}
	}
}
