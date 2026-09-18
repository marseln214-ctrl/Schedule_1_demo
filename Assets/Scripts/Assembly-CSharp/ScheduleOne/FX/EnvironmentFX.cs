using AtmosphericHeightFog;
using Funly.SkyStudio;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;
using VolumetricFogAndMist2;

namespace ScheduleOne.FX
{
	[ExecuteInEditMode]
	public class EnvironmentFX : Singleton<EnvironmentFX>
	{
		[Header("References")]
		[SerializeField]
		protected WindZone windZone;

		[SerializeField]
		protected TimeOfDayController timeOfDayController;

		public HeightFogGlobal HeightFog;

		//public VolumetricFog VolumetricFog;

		public Light SunLight;

		public Light MoonLight;

		[Header("Fog")]
		[SerializeField]
		protected Gradient fogColorGradient;

		[SerializeField]
		protected AnimationCurve fogEndDistanceCurve;

		[SerializeField]
		protected float fogEndDistanceMultiplier = 0.01f;

		[Header("Height Fog")]
		[SerializeField]
		protected Gradient HeightFogColor;

		[SerializeField]
		protected AnimationCurve HeightFogIntensityCurve;

		[SerializeField]
		protected float HeightFogIntensityMultiplier = 0.5f;

		[SerializeField]
		protected AnimationCurve HeightFogDirectionalIntensityCurve;

		[Header("Volumetric Fog")]
		[SerializeField]
		protected Gradient VolumetricFogColor;

		[SerializeField]
		protected AnimationCurve VolumetricFogIntensityCurve;

		[SerializeField]
		protected float VolumetricFogIntensityMultiplier = 0.5f;

		[Header("God rays")]
		[SerializeField]
		protected AnimationCurve godRayIntensityCurve;

		[Header("Contrast")]
		[SerializeField]
		protected AnimationCurve contrastCurve;

		[SerializeField]
		protected float contractMultiplier = 1f;

		[Header("Saturation")]
		[SerializeField]
		protected AnimationCurve saturationCurve;

		[SerializeField]
		protected float saturationMultiplier = 1f;

		[Header("Grass")]
		[SerializeField]
		protected Material grassMat;

		[SerializeField]
		protected Gradient grassColorGradient;

		[Header("Trees")]
		public Material distanceTreeMat;

		public AnimationCurve distanceTreeColorCurve;

		[Header("Stealth settings")]
		public AnimationCurve environmentalBrightnessCurve;

		[Header("Bloom")]
		public AnimationCurve bloomThreshholdCurve;

		private bool started;

		public float normalizedEnvironmentalBrightness => environmentalBrightnessCurve.Evaluate(((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal + NetworkSingleton<TimeManager>.Instance.TimeOnCurrentMinute / (5f / 6f)) / 1440f);

		protected override void Start()
		{
			base.Start();
			UpdateVisuals();
			if (Application.isPlaying && !started)
			{
				started = true;
				InvokeRepeating("UpdateVisuals", 0f, 0.1f);
			}
		}

		private void Update()
		{
			if (Application.isEditor)
			{
				byte b = (byte)distanceTreeColorCurve.Evaluate(timeOfDayController.skyTime);
				distanceTreeMat.SetColor("_TintColor", new Color32(b, b, b, byte.MaxValue));
				grassMat.color = grassColorGradient.Evaluate(timeOfDayController.skyTime);
			}
		}

		private void UpdateVisuals()
		{
			if (Application.isPlaying)
			{
				float num = (float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal + NetworkSingleton<TimeManager>.Instance.TimeOnCurrentMinute / (5f / 6f);
				timeOfDayController.skyTime = num / 1440f;
				RenderSettings.fogColor = fogColorGradient.Evaluate(timeOfDayController.skyTime);
				RenderSettings.fogEndDistance = fogEndDistanceCurve.Evaluate(timeOfDayController.skyTime) * fogEndDistanceMultiplier;
				HeightFog.fogColorStart = HeightFogColor.Evaluate(timeOfDayController.skyTime);
				HeightFog.fogColorEnd = HeightFogColor.Evaluate(timeOfDayController.skyTime);
				HeightFog.fogIntensity = HeightFogIntensityCurve.Evaluate(timeOfDayController.skyTime);
				HeightFog.directionalIntensity = HeightFogDirectionalIntensityCurve.Evaluate(timeOfDayController.skyTime);
				Color albedo = VolumetricFogColor.Evaluate(timeOfDayController.skyTime);
				albedo.a = VolumetricFogIntensityCurve.Evaluate(timeOfDayController.skyTime) * VolumetricFogIntensityMultiplier;
				//VolumetricFog.profile.albedo = albedo;
				byte b = (byte)distanceTreeColorCurve.Evaluate(num / 1440f);
				distanceTreeMat.SetColor("_TintColor", new Color32(b, b, b, byte.MaxValue));
				grassMat.color = grassColorGradient.Evaluate(timeOfDayController.skyTime);
				Singleton<PostProcessingManager>.Instance.SetGodRayIntensity(godRayIntensityCurve.Evaluate(timeOfDayController.skyTime));
				Singleton<PostProcessingManager>.Instance.SetContrast(contrastCurve.Evaluate(timeOfDayController.skyTime) * contractMultiplier);
				Singleton<PostProcessingManager>.Instance.SetSaturation(saturationCurve.Evaluate(timeOfDayController.skyTime) * saturationMultiplier);
				Singleton<PostProcessingManager>.Instance.SetBloomThreshold(bloomThreshholdCurve.Evaluate(timeOfDayController.skyTime));
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}
