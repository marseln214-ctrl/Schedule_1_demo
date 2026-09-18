using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Lighting
{
	[RequireComponent(typeof(ReflectionProbe))]
	public class ReflectionProbeUpdater : MonoBehaviour
	{
		public ReflectionProbe Probe;

		private static List<ReflectionProbe> renderQueue = new List<ReflectionProbe>();

		private static Coroutine RenderRoutine = null;

		private void OnValidate()
		{
			if (Probe == null)
			{
				Probe = GetComponent<ReflectionProbe>();
			}
		}

		private void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(UpdateProbe));
			UpdateProbe();
			if (RenderRoutine == null)
			{
				RenderRoutine = StartCoroutine(ProcessQueue());
			}
		}

		private void UpdateProbe()
		{
			if (!renderQueue.Contains(Probe))
			{
				renderQueue.Add(Probe);
			}
		}

		private IEnumerator ProcessQueue()
		{
			int renderDuration_Frames = 14;
			while (true)
			{
				if (renderQueue.Count > 0)
				{
					renderQueue[0].RenderProbe();
					renderQueue.RemoveAt(0);
				}
				for (int i = 0; i < renderDuration_Frames; i++)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}
}
