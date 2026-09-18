namespace Funly.SkyStudio
{
	public class RainSplashBlender : FeatureBlender
	{
		protected override string featureKey => "RainSplashFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumber("RainSplashMaxConcurrentKey");
			helper.BlendNumber("RainSplashAreaStartKey");
			helper.BlendNumber("RainSplashAreaLengthKey");
			helper.BlendNumber("RainSplashScaleKey");
			helper.BlendNumber("RainSplashScaleVarienceKey");
			helper.BlendNumber("RainSplashIntensityKey");
			helper.BlendNumber("RainSplashSurfaceOffsetKey");
			helper.BlendColor("RainSplashTintColorKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("RainSplashIntensityKey");
			helper.BlendNumberIn("RainSplashMaxConcurrentKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("RainSplashIntensityKey");
			helper.BlendNumberOut("RainSplashMaxConcurrentKey");
		}
	}
}
