using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Map
{
	public class ScheduledMaterialChange : MonoBehaviour
	{
		private enum EOnState
		{
			Undecided = 0,
			On = 1,
			Off = 2
		}

		public MeshRenderer[] Renderers;

		public int MaterialIndex;

		[Header("Settings")]
		public bool Enabled = true;

		public Material OutsideTimeRangeMaterial;

		public Material InsideTimeRangeMaterial;

		public int TimeRangeMin;

		public int TimeRangeMax;

		public int TimeRangeShift;

		public int TimeRangeRandomization;

		[Range(0f, 1f)]
		public float TurnOnChance = 1f;

		private bool appliedInsideTimeRange;

		private EOnState onState;

		private int randomShift;

		protected virtual void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(Tick));
			SetMaterial(insideTimeRange: false);
			appliedInsideTimeRange = false;
			randomShift = UnityEngine.Random.Range(-TimeRangeRandomization, TimeRangeRandomization);
			Tick();
		}

		protected virtual void Tick()
		{
			if (!Enabled && appliedInsideTimeRange)
			{
				SetMaterial(insideTimeRange: false);
			}
			int min = TimeManager.AddMinutesTo24HourTime(TimeRangeMin, TimeRangeShift + randomShift);
			int max = TimeManager.AddMinutesTo24HourTime(TimeRangeMax, TimeRangeShift + randomShift);
			if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(min, max))
			{
				if (onState == EOnState.Undecided)
				{
					onState = ((!(UnityEngine.Random.Range(0f, 1f) > TurnOnChance)) ? EOnState.On : EOnState.Off);
				}
			}
			else
			{
				onState = EOnState.Undecided;
			}
			if (NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(min, max) && onState == EOnState.On)
			{
				if (!appliedInsideTimeRange)
				{
					SetMaterial(insideTimeRange: true);
				}
			}
			else if (appliedInsideTimeRange)
			{
				SetMaterial(insideTimeRange: false);
			}
		}

		private void SetMaterial(bool insideTimeRange)
		{
			if (Renderers != null && Renderers.Length != 0)
			{
				appliedInsideTimeRange = insideTimeRange;
				Material material = Renderers[0].materials[MaterialIndex];
				material = (insideTimeRange ? InsideTimeRangeMaterial : OutsideTimeRangeMaterial);
				MeshRenderer[] renderers = Renderers;
				foreach (MeshRenderer obj in renderers)
				{
					Material[] materials = obj.materials;
					materials[MaterialIndex] = material;
					obj.materials = materials;
				}
			}
		}
	}
}
