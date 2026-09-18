using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.Noise
{
	public class Listener : MonoBehaviour
	{
		public delegate void HearingEvent(NoiseEvent nEvent);

		public static List<Listener> listeners = new List<Listener>();

		[Header("Settings")]
		[Range(0.1f, 5f)]
		public float Sensitivity = 1f;

		public Transform HearingOrigin;

		public HearingEvent onNoiseHeard;

		public float SquaredHearingRange { get; protected set; }

		public void Awake()
		{
			SquaredHearingRange = Mathf.Pow(Sensitivity, 2f);
			if (HearingOrigin == null)
			{
				HearingOrigin = base.transform;
			}
		}

		public void OnEnable()
		{
			if (!listeners.Contains(this))
			{
				listeners.Add(this);
			}
		}

		public void OnDisable()
		{
			listeners.Remove(this);
		}

		public void Notify(NoiseEvent nEvent)
		{
			if (onNoiseHeard != null)
			{
				onNoiseHeard(nEvent);
			}
		}
	}
}
