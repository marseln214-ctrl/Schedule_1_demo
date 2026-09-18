namespace Funly.SkyStudio
{
	public class CloudBlender : FeatureBlender
	{
		protected override string featureKey => "CloudFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumber("CloudDensityKey");
			helper.BlendNumber("CloudTextureTiling");
			helper.BlendNumber("CloudSpeedKey");
			helper.BlendNumber("CloudDirectionKey");
			helper.BlendNumber("CloudFadeAmountKey");
			helper.BlendNumber("CloudFadePositionKey");
			helper.BlendNumber("CloudAlphaKey");
			helper.BlendColor("CloudColor1Key");
			helper.BlendColor("CloudColor2Key");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("CloudAlphaKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("CloudAlphaKey");
		}
	}
}
