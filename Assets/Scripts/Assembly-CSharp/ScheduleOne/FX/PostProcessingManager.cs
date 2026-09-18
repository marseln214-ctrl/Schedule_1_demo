using Beautify.Universal;
using CorgiGodRays;
using ScheduleOne.DevUtilities;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.FX
{
	public class PostProcessingManager : Singleton<PostProcessingManager>
	{
		[Header("References")]
		public Volume GlobalVolume;

		[Header("Vignette")]
		public float Vig_DefaultIntensity = 0.25f;

		public float Vig_DefaultSmoothness = 0.3f;

		[Header("Blur")]
		public float MinBlur;

		public float MaxBlur = 1f;

		[Header("Smoothers")]
		public FloatSmoother ChromaticAberrationController;

		public FloatSmoother SaturationController;

		public FloatSmoother BloomController;

		public HDRColorSmoother ColorFilterController;

		private Vignette vig;

		private DepthOfField DoF;

		private GodRaysVolume GodRays;

		private ColorAdjustments ColorAdjustments;

		//private Beautify.Universal.Beautify beautifySettings;

		private Bloom bloom;

		private ChromaticAberration chromaticAberration;

		private ColorAdjustments colorAdjustments;

		protected override void Awake()
		{
			base.Awake();
			GlobalVolume.enabled = true;
			GlobalVolume.sharedProfile.TryGet<Vignette>(out vig);
			ResetVignette();
			GlobalVolume.sharedProfile.TryGet<DepthOfField>(out DoF);
			DoF.active = false;
			GlobalVolume.sharedProfile.TryGet<GodRaysVolume>(out GodRays);
			GlobalVolume.sharedProfile.TryGet<ColorAdjustments>(out ColorAdjustments);
			//GlobalVolume.sharedProfile.TryGet<Beautify.Universal.Beautify>(out beautifySettings);
			GlobalVolume.sharedProfile.TryGet<Bloom>(out bloom);
			GlobalVolume.sharedProfile.TryGet<ChromaticAberration>(out chromaticAberration);
			GlobalVolume.sharedProfile.TryGet<ColorAdjustments>(out colorAdjustments);
			ChromaticAberrationController.Initialize();
			SaturationController.Initialize();
			BloomController.Initialize();
			ColorFilterController.Initialize();
			SetBlur(0f);
		}

		public void Update()
		{
			UpdateEffects();
		}

		private void UpdateEffects()
		{
			chromaticAberration.intensity.value = ChromaticAberrationController.CurrentValue;
			ColorAdjustments.saturation.value = SaturationController.CurrentValue;
			bloom.intensity.value = BloomController.CurrentValue;
			colorAdjustments.colorFilter.value = ColorFilterController.CurrentValue;
		}

		public void OverrideVignette(float intensity, float smoothness)
		{
			vig.intensity.value = intensity;
			vig.smoothness.value = smoothness;
		}

		public void ResetVignette()
		{
			vig.intensity.value = Vig_DefaultIntensity;
			vig.smoothness.value = Vig_DefaultSmoothness;
		}

		public void SetGodRayIntensity(float intensity)
		{
			GodRays.MainLightIntensity.value = intensity;
		}

		public void SetContrast(float value)
		{
			ColorAdjustments.contrast.value = value;
		}

		public void SetSaturation(float value)
		{
			SaturationController.SetDefault(value);
		}

		public void SetBloomThreshold(float threshold)
		{
			bloom.threshold.value = threshold;
		}

		public void SetBlur(float blurLevel)
		{
			//beautifySettings.blurIntensity.value = Mathf.Lerp(MinBlur, MaxBlur, blurLevel);
		}
	}
}
