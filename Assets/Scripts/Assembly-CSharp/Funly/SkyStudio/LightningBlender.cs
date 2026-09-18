namespace Funly.SkyStudio
{
	public class LightningBlender : FeatureBlender
	{
		protected override string featureKey => "LightningFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor("LightningTintColorKey");
			helper.BlendNumber("ThunderSoundVolumeKey");
			helper.BlendNumber("ThunderSoundDelayKey");
			helper.BlendNumber("LightningProbabilityKey");
			helper.BlendNumber("LightningStrikeCoolDown");
			helper.BlendNumber("LightningIntensityKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("ThunderSoundVolumeKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("ThunderSoundVolumeKey");
		}
	}
}
