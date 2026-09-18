using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.DevUtilities
{
	public class PositionHistoryTracker : MonoBehaviour
	{
		[Tooltip("Frequency (in seconds) to record the position.")]
		public float recordingFrequency = 1f;

		[Tooltip("Duration (in seconds) to store the position history.")]
		public float historyDuration = 10f;

		public List<Vector3> positionHistory = new List<Vector3>();

		private float lastRecordTime;

		public float RecordedTime => (float)positionHistory.Count * recordingFrequency;

		private void Start()
		{
			lastRecordTime = Time.time;
		}

		private void Update()
		{
			if (Time.time - lastRecordTime >= recordingFrequency)
			{
				RecordPosition();
				lastRecordTime = Time.time;
			}
		}

		private void RecordPosition()
		{
			positionHistory.Add(base.transform.position);
			if ((float)positionHistory.Count * recordingFrequency > historyDuration)
			{
				positionHistory.RemoveAt(0);
			}
		}

		public Vector3 GetPositionXSecondsAgo(float secondsAgo)
		{
			int value = (int)(secondsAgo / recordingFrequency);
			value = Mathf.Clamp(value, 0, positionHistory.Count - 1);
			return positionHistory[value];
		}

		public void ClearHistory()
		{
			positionHistory.Clear();
		}
	}
}
