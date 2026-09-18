namespace Funly.SkyStudio
{
	public class SunBlender : FeatureBlender
	{
		protected override string featureKey => "SunFeature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor("SunColorKey");
			helper.BlendNumber("SunSizeKey");
			helper.BlendNumber("SunEdgeFeatheringKey");
			helper.BlendNumber("SunColorIntensityKey");
			helper.BlendNumber("SunAlphaKey");
			helper.BlendColor("SunLightColorKey");
			helper.BlendNumber("SunLightIntensityKey");
			helper.BlendSpherePoint("SunPositionKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn("SunAlphaKey");
			helper.BlendNumberIn("SunLightIntensityKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut("SunAlphaKey");
			helper.BlendNumberOut("SunLightIntensityKey");
		}
	}
}
