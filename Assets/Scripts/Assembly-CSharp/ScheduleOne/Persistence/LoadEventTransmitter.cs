using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Persistence
{
	public class LoadEventTransmitter : MonoBehaviour
	{
		public UnityEvent onLoadComplete;

		private void Start()
		{
			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(OnLoadComplete);
		}

		private void OnLoadComplete()
		{
			if (onLoadComplete != null)
			{
				onLoadComplete.Invoke();
			}
		}
	}
}
