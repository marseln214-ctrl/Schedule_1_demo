namespace Funly.SkyStudio
{
	public class MoonBlender : FeatureBlender
	{
		protected override string featureKey => "MoonFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor("MoonColorKey");
			helper.BlendNumber("MoonSizeKey");
			helper.BlendNumber("MoonEdgeFeatheringKey");
			helper.BlendNumber("MoonColorIntensityKey");
			helper.BlendNumber("MoonAlphaKey");
			helper.BlendColor("MoonLightColorKey");
			helper.BlendNumber("MoonLightIntensityKey");
			helper.BlendSpherePoint("MoonPositionKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("MoonAlphaKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("MoonAlphaKey");
		}
	}
}
