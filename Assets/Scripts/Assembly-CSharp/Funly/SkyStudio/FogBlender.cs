namespace Funly.SkyStudio
{
	public class FogBlender : FeatureBlender
	{
		protected override string featureKey => "FogFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumber("FogDensityKey");
			helper.BlendNumber("FogLengthKey");
			helper.BlendColor("FogColorKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("FogDensityKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("FogDensityKey");
		}
	}
}
