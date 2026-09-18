using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools
{
	public class DelayedUnityEvent : MonoBehaviour
	{
		public float Delay = 1f;

		public UnityEvent onDelayStart;

		public UnityEvent onDelayedExecute;

		public void Execute()
		{
			StartCoroutine(Wait());
			IEnumerator Wait()
			{
				if (onDelayStart != null)
				{
					onDelayStart.Invoke();
				}
				yield return new WaitForSeconds(Delay);
				if (onDelayedExecute != null)
				{
					onDelayedExecute.Invoke();
				}
			}
		}
	}
}
