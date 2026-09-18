namespace Funly.SkyStudio
{
	public class SkyBlender : FeatureBlender
	{
		protected override string featureKey => "";

		protected override ProfileFeatureBlendingMode BlendingMode(ProfileBlendingState state, BlendingHelper helper)
		{
			return ProfileFeatureBlendingMode.Normal;
		}

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor("SkyLowerColorKey");
			helper.BlendColor("SkyMiddleColorKey");
			helper.BlendColor("SkyUpperColorKey");
			helper.BlendNumber("SkyMiddleColorPosition");
			helper.BlendNumber("HorizonTransitionStartKey");
			helper.BlendNumber("HorizonTransitionLengthKey");
			helper.BlendNumber("StarTransitionStartKey");
			helper.BlendNumber("StarTransitionLengthKey");
			helper.BlendNumber("HorizonStarScaleKey");
			helper.BlendColor("AmbientLightSkyColorKey");
			helper.BlendColor("AmbientLightEquatorColorKey");
			helper.BlendColor("AmbientLightGroundColorKey");
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor("AmbientLightSkyColorKey");
			helper.BlendColor("AmbientLightEquatorColorKey");
			helper.BlendColor("AmbientLightGroundColorKey");
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor("AmbientLightSkyColorKey");
			helper.BlendColor("AmbientLightEquatorColorKey");
			helper.BlendColor("AmbientLightGroundColorKey");
		}
	}
}
