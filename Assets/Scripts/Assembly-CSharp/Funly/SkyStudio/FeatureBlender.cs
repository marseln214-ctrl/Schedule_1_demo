using UnityEngine;

namespace Funly.SkyStudio
{
	public abstract class FeatureBlender : MonoBehaviour, IFeatureBlender
	{
		protected abstract string featureKey { get; }

		protected abstract void BlendBoth(ProfileBlendingState state, BlendingHelper helper);

		protected abstract void BlendIn(ProfileBlendingState state, BlendingHelper helper);

		protected abstract void BlendOut(ProfileBlendingState state, BlendingHelper helper);

		protected virtual ProfileFeatureBlendingMode BlendingMode(ProfileBlendingState state, BlendingHelper helper)
		{
			return helper.GetFeatureAnimationMode(featureKey);
		}

		public virtual void Blend(ProfileBlendingState state, BlendingHelper helper)
		{
			switch (BlendingMode(state, helper))
			{
			case ProfileFeatureBlendingMode.Normal:
				BlendBoth(state, helper);
				break;
			case ProfileFeatureBlendingMode.FadeFeatureOut:
				BlendOut(state, helper);
				break;
			case ProfileFeatureBlendingMode.FadeFeatureIn:
				BlendIn(state, helper);
				break;
			}
		}
	}
}
