using UnityEngine;

namespace Funly.SkyStudio
{
	public class BlendingHelper
	{
		private ProfileBlendingState m_State;

		public BlendingHelper(ProfileBlendingState state)
		{
			m_State = state;
		}

		public void UpdateState(ProfileBlendingState state)
		{
			m_State = state;
		}

		public Color ProfileColorForKey(SkyProfile profile, string key)
		{
			float time = ((profile == m_State.toProfile) ? 0f : m_State.timeOfDay);
			return profile.GetGroup<ColorKeyframeGroup>(key).ColorForTime(time);
		}

		public float ProfileNumberForKey(SkyProfile profile, string key)
		{
			float time = ((profile == m_State.toProfile) ? 0f : m_State.timeOfDay);
			return profile.GetGroup<NumberKeyframeGroup>(key).NumericValueAtTime(time);
		}

		public SpherePoint ProfileSpherePointForKey(SkyProfile profile, string key)
		{
			float time = ((profile == m_State.toProfile) ? 0f : m_State.timeOfDay);
			return profile.GetGroup<SpherePointKeyframeGroup>(key).SpherePointForTime(time);
		}

		public void BlendColor(string key)
		{
			BlendColor(key, ProfileColorForKey(m_State.fromProfile, key), ProfileColorForKey(m_State.toProfile, key), m_State.progress);
		}

		public void BlendColorOut(string key)
		{
			BlendColor(key, ProfileColorForKey(m_State.fromProfile, key), ProfileColorForKey(m_State.fromProfile, key).Clear(), m_State.outProgress);
		}

		public void BlendColorIn(string key)
		{
			BlendColor(key, ProfileColorForKey(m_State.toProfile, key).Clear(), ProfileColorForKey(m_State.toProfile, key), m_State.inProgress);
		}

		public void BlendColor(string key, Color from, Color to, float progress)
		{
			m_State.blendedProfile.GetGroup<ColorKeyframeGroup>(key).keyframes[0].color = Color.LerpUnclamped(from, to, progress);
		}

		public void BlendNumber(string key)
		{
			BlendNumber(key, ProfileNumberForKey(m_State.fromProfile, key), ProfileNumberForKey(m_State.toProfile, key), m_State.progress);
		}

		public void BlendNumberOut(string key, float toValue = 0f)
		{
			BlendNumber(key, ProfileNumberForKey(m_State.fromProfile, key), toValue, m_State.outProgress);
		}

		public void BlendNumberIn(string key, float fromValue = 0f)
		{
			BlendNumber(key, fromValue, ProfileNumberForKey(m_State.toProfile, key), m_State.inProgress);
		}

		public void BlendNumber(string key, float from, float to, float progress)
		{
			m_State.blendedProfile.GetGroup<NumberKeyframeGroup>(key).keyframes[0].value = Mathf.Lerp(from, to, progress);
		}

		public void BlendSpherePoint(string key)
		{
			BlendSpherePoint(key, ProfileSpherePointForKey(m_State.fromProfile, "MoonPositionKey"), ProfileSpherePointForKey(m_State.toProfile, "MoonPositionKey"), m_State.progress);
		}

		public void BlendSpherePoint(string key, SpherePoint from, SpherePoint to, float progress)
		{
			Vector3 vector = Vector3.Slerp(from.GetWorldDirection(), to.GetWorldDirection(), progress);
			m_State.blendedProfile.GetGroup<SpherePointKeyframeGroup>(key).keyframes[0].spherePoint = new SpherePoint(vector.normalized);
		}

		public ProfileFeatureBlendingMode GetFeatureAnimationMode(string featureKey)
		{
			bool flag = m_State.fromProfile.IsFeatureEnabled(featureKey);
			bool flag2 = m_State.toProfile.IsFeatureEnabled(featureKey);
			if (flag && flag2)
			{
				return ProfileFeatureBlendingMode.Normal;
			}
			if (flag && !flag2)
			{
				return ProfileFeatureBlendingMode.FadeFeatureOut;
			}
			if (!flag && flag2)
			{
				return ProfileFeatureBlendingMode.FadeFeatureIn;
			}
			return ProfileFeatureBlendingMode.None;
		}
	}
}
