using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.Misc
{
	public class ManholeCover : MonoBehaviour
	{
		public ParticleSystem SteamParticles;

		public Gradient SteamColor;

		public AnimationCurve SteamAlpha;

		private void Start()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void MinPass()
		{
			Color startColor = SteamColor.Evaluate((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 1440f);
			startColor.a = SteamAlpha.Evaluate((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 1440f);
			SteamParticles.startColor = startColor;
		}
	}
}
