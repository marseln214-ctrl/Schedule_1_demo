using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	public class RandomIntervalEvent : MonoBehaviour
	{
		public float MinInterval = 5f;

		public float MaxInterval = 10f;

		public bool ExecuteOnEnable;

		public UnityEvent OnInterval;

		private float nextInterval;

		private void OnEnable()
		{
			if (ExecuteOnEnable)
			{
				Execute();
			}
			nextInterval = Time.time + Random.Range(MinInterval, MaxInterval);
		}

		private void Update()
		{
			if (Time.time >= nextInterval)
			{
				Execute();
			}
		}

		private void Execute()
		{
			if (OnInterval != null)
			{
				OnInterval.Invoke();
			}
			nextInterval = Time.time + Random.Range(MinInterval, MaxInterval);
		}
	}
}
