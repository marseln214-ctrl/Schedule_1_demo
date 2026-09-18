using System;
using ScheduleOne.Quests;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ScriptableObjects
{
	[Serializable]
	[CreateAssetMenu(fileName = "PhoneCallData", menuName = "ScriptableObjects/PhoneCallData", order = 1)]
	public class PhoneCallData : ScriptableObject
	{
		[Serializable]
		public class Stage
		{
			[TextArea(3, 10)]
			public string Text;

			public SystemTrigger[] OnStartTriggers;

			public SystemTrigger[] OnDoneTriggers;

			public void OnStageStart()
			{
				if (OnStartTriggers != null)
				{
					for (int i = 0; i < OnStartTriggers.Length; i++)
					{
						OnStartTriggers[i].Trigger();
					}
				}
			}

			public void OnStageEnd()
			{
				if (OnDoneTriggers != null)
				{
					for (int i = 0; i < OnDoneTriggers.Length; i++)
					{
						OnDoneTriggers[i].Trigger();
					}
				}
			}
		}

		public CallerID CallerID;

		public Stage[] Stages;

		public UnityEvent onCallCompleted;

		public void Completed()
		{
			if (onCallCompleted != null)
			{
				onCallCompleted.Invoke();
			}
		}
	}
}
