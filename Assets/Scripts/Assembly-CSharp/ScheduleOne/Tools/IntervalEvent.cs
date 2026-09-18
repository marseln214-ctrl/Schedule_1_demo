using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	public class IntervalEvent : MonoBehaviour
	{
		public float Interval = 1f;

		public UnityEvent Event;

		public void Start()
		{
			InvokeRepeating("Execute", Interval, Interval);
		}

		private void Execute()
		{
			if (Event != null)
			{
				Event.Invoke();
			}
		}
	}
}
