using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Lighting;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Map.Infrastructure
{
	public class StreetLight : MonoBehaviour
	{
		public static Vector3 POWER_ORIGIN = new Vector3(150f, 0f, -150f);

		[Header("References")]
		[SerializeField]
		protected MeshRenderer LightRend;

		[SerializeField]
		protected Light Light;

		[SerializeField]
		protected VolumetricLightTracker BeamTracker;

		[Header("Materials")]
		public Material LightOnMat;

		public Material LightOffMat;

		[Header("Timing")]
		public int StartTime = 1800;

		public int EndTime = 600;

		public int StartTimeOffset;

		[Header("Settings")]
		public bool ShadowsEnabled = true;

		public float LightMaxDistance = 80f;

		public float SoftShadowsThreshold = 12f;

		public float HardShadowsThreshold = 36f;

		private bool isOn;

		protected virtual void Awake()
		{
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(UpdateState));
			if (BeamTracker != null)
			{
				BeamTracker.Override = true;
			}
			StartTimeOffset = (int)(Vector3.Distance(base.transform.position, POWER_ORIGIN) / 50f);
		}

		private void Start()
		{
			UpdateState();
		}

		protected virtual void UpdateState()
		{
			SetState(NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(TimeManager.AddMinutesTo24HourTime(StartTime, StartTimeOffset), TimeManager.AddMinutesTo24HourTime(EndTime, StartTimeOffset)));
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				UpdateShadows();
			}
		}

		private void OnDrawGizmos()
		{
		}

		private void SetState(bool on)
		{
			if (BeamTracker != null)
			{
				BeamTracker.Enabled = isOn;
			}
			float num = 0f;
			if (PlayerSingleton<PlayerCamera>.InstanceExists)
			{
				num = Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
			}
			if (num < LightMaxDistance * QualitySettings.lodBias)
			{
				Light.enabled = isOn;
			}
			else
			{
				Light.enabled = false;
			}
			if (on != isOn)
			{
				isOn = on;
				if (LightRend != null)
				{
					LightRend.material = (isOn ? LightOnMat : LightOffMat);
				}
			}
		}

		private void UpdateShadows()
		{
			if (!ShadowsEnabled)
			{
				Light.shadows = LightShadows.None;
				return;
			}
			float num = Vector3.Distance(base.transform.position, PlayerSingleton<PlayerCamera>.Instance.transform.position);
			if (num < SoftShadowsThreshold * QualitySettings.lodBias)
			{
				Light.shadows = LightShadows.Soft;
			}
			else if (num < HardShadowsThreshold * QualitySettings.lodBias)
			{
				Light.shadows = LightShadows.Hard;
			}
			else
			{
				Light.shadows = LightShadows.None;
			}
		}
	}
}
