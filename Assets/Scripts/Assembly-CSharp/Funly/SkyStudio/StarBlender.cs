using UnityEngine;

namespace Funly.SkyStudio
{
	public class StarBlender : FeatureBlender
	{
		[Range(1f, 3f)]
		public int starLayer;

		protected override string featureKey => "StarLayer" + starLayer + "Feature";

		protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendColor(PropertyKeyForLayer("Star1ColorKey"));
			helper.BlendNumber(PropertyKeyForLayer("Star1SizeKey"));
			helper.BlendNumber(PropertyKeyForLayer("Star1RotationSpeed"));
			helper.BlendNumber(PropertyKeyForLayer("Star1TwinkleAmountKey"));
			helper.BlendNumber(PropertyKeyForLayer("Star1TwinkleSpeedKey"));
			helper.BlendNumber(PropertyKeyForLayer("Star1EdgeFeathering"));
			helper.BlendNumber(PropertyKeyForLayer("Star1ColorIntensityKey"));
		}

		protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberIn(PropertyKeyForLayer("Star1SizeKey"));
		}

		protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
		{
			helper.BlendNumberOut(PropertyKeyForLayer("Star1SizeKey"));
		}

		private string PropertyKeyForLayer(string key)
		{
			return key.Replace("Star1", "Star" + starLayer);
		}
	}
}
