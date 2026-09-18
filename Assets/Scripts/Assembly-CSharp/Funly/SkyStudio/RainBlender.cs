namespace Funly.SkyStudio
{
	public class RainBlender : FeatureBlender
	{
		protected override string featureKey => "RainFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumber("RainSoundVolume");
			helper.BlendNumber("RainNearIntensityKey");
			helper.BlendNumber("RainNearSpeedKey");
			helper.BlendNumber("RainNearTextureTiling");
			helper.BlendNumber("RainFarIntensityKey");
			helper.BlendNumber("RainFarSpeedKey");
			helper.BlendNumber("RainFarTextureTiling");
			helper.BlendColor("RainTintColorKey");
			helper.BlendNumber("RainWindTurbulenceKey");
			helper.BlendNumber("RainWindTurbulenceSpeedKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("RainSoundVolume");
			helper.BlendNumberIn("RainNearIntensityKey");
			helper.BlendNumberIn("RainFarIntensityKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("RainSoundVolume");
			helper.BlendNumberOut("RainNearIntensityKey");
			helper.BlendNumberOut("RainFarIntensityKey");
		}
	}
}
