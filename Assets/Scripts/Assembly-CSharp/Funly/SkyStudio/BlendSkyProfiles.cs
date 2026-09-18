using System;
using UnityEngine;

namespace Funly.SkyStudio
{
	public class BlendSkyProfiles : MonoBehaviour
	{
		[Tooltip("Called when blending finishes.")]
		public Action<BlendSkyProfiles> onBlendComplete;

		[HideInInspector]
		private float m_StartTime = -1f;

		[HideInInspector]
		private float m_EndTime = -1f;

		[Tooltip("Blender used for basic sky background properties.")]
		public FeatureBlender skyBlender;

		[Tooltip("Blender used for the sun properties.")]
		public FeatureBlender sunBlender;

		[Tooltip("Blender used moon properties.")]
		public FeatureBlender moonBlender;

		[Tooltip("Blender used cloud properties.")]
		public FeatureBlender cloudBlender;

		[Tooltip("Blender used star layer 1 properties.")]
		public FeatureBlender starLayer1Blender;

		[Tooltip("Blender used star layer 2 properties.")]
		public FeatureBlender starLayer2Blender;

		[Tooltip("Blender used star layer 3 properties.")]
		public FeatureBlender starLayer3Blender;

		[Tooltip("Blender used by the rain downfall feature.")]
		public FeatureBlender rainBlender;

		[Tooltip("Blender used by the rain splash feature.")]
		public FeatureBlender rainSplashBlender;

		[Tooltip("Blender used for lightning feature properties.")]
		public FeatureBlender lightningBlender;

		[Tooltip("Blender used for fog properties.")]
		public FeatureBlender fogBlender;

		private bool m_IsBlendingFirstHalf = true;

		private ProfileBlendingState m_State;

		private TimeOfDayController m_TimeOfDayController;

		private BlendingHelper blendingHelper;

		public SkyProfile fromProfile { get; private set; }

		public SkyProfile toProfile { get; private set; }

		public SkyProfile blendedProfile { get; private set; }

		public SkyProfile StartBlending(TimeOfDayController controller, SkyProfile fromProfile, SkyProfile toProfile, float duration)
		{
			if (controller == null)
			{
				Debug.LogWarning("Can't transition with null TimeOfDayController");
				return null;
			}
			if (fromProfile == null)
			{
				Debug.LogWarning("Can't transition to null 'from' sky profile.");
				return null;
			}
			if (toProfile == null)
			{
				Debug.LogWarning("Can't transition to null 'to' sky profile");
				return null;
			}
			if (!fromProfile.IsFeatureEnabled("GradientSkyFeature") || !toProfile.IsFeatureEnabled("GradientSkyFeature"))
			{
				Debug.LogWarning("Sky Studio doesn't currently support automatic transition blending with cubemap backgrounds.");
			}
			m_TimeOfDayController = controller;
			this.fromProfile = fromProfile;
			this.toProfile = toProfile;
			m_StartTime = Time.time;
			m_EndTime = m_StartTime + duration;
			blendedProfile = UnityEngine.Object.Instantiate(fromProfile);
			blendedProfile.skyboxMaterial = fromProfile.skyboxMaterial;
			m_TimeOfDayController.skyProfile = blendedProfile;
			m_State = new ProfileBlendingState(blendedProfile, fromProfile, toProfile, 0f, 0f, 0f, m_TimeOfDayController.timeOfDay);
			blendingHelper = new BlendingHelper(m_State);
			UpdateBlendedProfile();
			return blendedProfile;
		}

		public void CancelBlending()
		{
			TearDownBlending();
		}

		public void TearDownBlending()
		{
			if (!(m_TimeOfDayController == null))
			{
				m_TimeOfDayController = null;
				blendedProfile = null;
				base.enabled = false;
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void Update()
		{
			if (!(blendedProfile == null))
			{
				UpdateBlendedProfile();
			}
		}

		private void UpdateBlendedProfile()
		{
			if (m_TimeOfDayController == null)
			{
				return;
			}
			float num = m_EndTime - m_StartTime;
			float num2 = Time.time - m_StartTime;
			m_State.progress = num2 / num;
			m_State.inProgress = PercentForMode(ProfileFeatureBlendingMode.FadeFeatureIn, m_State.progress);
			m_State.outProgress = PercentForMode(ProfileFeatureBlendingMode.FadeFeatureOut, m_State.progress);
			blendingHelper.UpdateState(m_State);
			if (m_State.progress > 0.5f && m_IsBlendingFirstHalf)
			{
				m_IsBlendingFirstHalf = false;
				blendedProfile = UnityEngine.Object.Instantiate(toProfile);
				m_State.blendedProfile = blendedProfile;
				m_TimeOfDayController.skyProfile = blendedProfile;
			}
			blendingHelper.UpdateState(m_State);
			FeatureBlender[] array = new FeatureBlender[11]
			{
				skyBlender, sunBlender, moonBlender, cloudBlender, starLayer1Blender, starLayer2Blender, starLayer3Blender, rainBlender, rainSplashBlender, lightningBlender,
				fogBlender
			};
			foreach (FeatureBlender featureBlender in array)
			{
				if (!(featureBlender == null))
				{
					featureBlender.Blend(m_State, blendingHelper);
				}
			}
			m_TimeOfDayController.skyProfile = blendedProfile;
			if (m_State.progress >= 1f)
			{
				onBlendComplete(this);
				TearDownBlending();
			}
		}

		private float PercentForMode(ProfileFeatureBlendingMode mode, float percent)
		{
			return mode switch
			{
				ProfileFeatureBlendingMode.FadeFeatureIn => Mathf.Clamp01((percent - 0.5f) * 2f), 
				ProfileFeatureBlendingMode.FadeFeatureOut => Mathf.Clamp01(percent * 2f), 
				_ => percent, 
			};
		}
	}
}
