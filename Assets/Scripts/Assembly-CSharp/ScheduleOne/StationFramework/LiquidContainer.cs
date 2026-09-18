using System;
using LiquidVolumeFX;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.StationFramework
{
	public class LiquidContainer : MonoBehaviour
	{
		[Header("Settings")]
		[Range(0f, 1f)]
		public float Viscosity = 0.4f;

		public bool AdjustMurkiness = true;

		[Header("References")]
		public LiquidVolume LiquidVolume;

		public LiquidVolumeCollider Collider;

		public Transform ColliderTransform_Min;

		public Transform ColliderTransform_Max;

		[Header("Visuals Settings")]
		public float MaxLevel = 1f;

		private MeshRenderer liquidMesh;

		public float CurrentLiquidLevel { get; private set; }

		public Color LiquidColor { get; private set; } = Color.white;


		private void Awake()
		{
			liquidMesh = LiquidVolume.GetComponent<MeshRenderer>();
			SetLiquidColor(LiquidVolume.liquidColor1);
		}

		private void Start()
		{
			LiquidVolume.directionalLight = Singleton<EnvironmentFX>.Instance.SunLight;
			TimeManager instance = NetworkSingleton<TimeManager>.Instance;
			instance.onMinutePass = (Action)Delegate.Combine(instance.onMinutePass, new Action(MinPass));
		}

		private void OnDestroy()
		{
			if (NetworkSingleton<TimeManager>.InstanceExists)
			{
				TimeManager instance = NetworkSingleton<TimeManager>.Instance;
				instance.onMinutePass = (Action)Delegate.Remove(instance.onMinutePass, new Action(MinPass));
			}
		}

		private void MinPass()
		{
			UpdateLighting();
		}

		private void UpdateLighting()
		{
			if (AdjustMurkiness)
			{
				float t = Mathf.Abs((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 1440f - 0.5f) / 0.5f;
				float num = Mathf.Lerp(1f, 0.75f, t);
				SetLiquidColor(LiquidColor * num, setColorVariable: false, updateLigting: false);
			}
		}

		public void SetLiquidLevel(float level, bool debug = false)
		{
			if (debug)
			{
				Console.Log("setting liquid level to: " + level);
			}
			CurrentLiquidLevel = Mathf.Clamp01(level);
			LiquidVolume.level = Mathf.Lerp(0f, MaxLevel, CurrentLiquidLevel);
			if (liquidMesh != null)
			{
				liquidMesh.enabled = CurrentLiquidLevel > 0.01f;
			}
			if (Collider != null && ColliderTransform_Min != null && ColliderTransform_Max != null)
			{
				Collider.transform.localPosition = Vector3.Lerp(ColliderTransform_Min.localPosition, ColliderTransform_Max.localPosition, CurrentLiquidLevel);
				Collider.transform.localScale = Vector3.Lerp(ColliderTransform_Min.localScale, ColliderTransform_Max.localScale, CurrentLiquidLevel);
			}
		}

		public void SetLiquidColor(Color color, bool setColorVariable = true, bool updateLigting = true)
		{
			if (setColorVariable)
			{
				LiquidColor = color;
			}
			LiquidVolume.liquidColor1 = color;
			LiquidVolume.liquidColor2 = color;
			if (updateLigting)
			{
				UpdateLighting();
			}
		}
	}
}
