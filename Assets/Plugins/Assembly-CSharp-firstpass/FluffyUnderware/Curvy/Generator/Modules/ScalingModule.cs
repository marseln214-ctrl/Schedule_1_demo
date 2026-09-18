using System;
using System.Runtime.CompilerServices;
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	public abstract class ScalingModule : CGModule
	{
		[Tab("Scale", Sort = 101)]
		[Label("Mode", "")]
		[SerializeField]
		[Tooltip("What type of scaling should be applied")]
		private ScaleMode m_ScaleMode;

		[FieldCondition("m_ScaleMode", ScaleMode.Advanced, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Label("Reference", "")]
		[SerializeField]
		[Tooltip("Determines on what range the scale is applied:\r\nSelf: the scale is applied over the Path's active range\r\nSource: the scale is applied over the Path's total length")]
		private CGReferenceMode m_ScaleReference = CGReferenceMode.Self;

		[FieldCondition("m_ScaleMode", ScaleMode.Advanced, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Label("Offset", "")]
		[SerializeField]
		[Tooltip("Scale is applied starting at this offset")]
		private float m_ScaleOffset;

		[SerializeField]
		[Label("Uniform Scaling", "")]
		[Tooltip("If enabled, the same scale is applied to both X and Y axis of the cross section")]
		private bool m_ScaleUniform = true;

		[SerializeField]
		[Tooltip("The (base) value of the scaling along the cross section's X axis, and Y axis if Uniform Scaling is disabled")]
		private float m_ScaleX = 1f;

		[SerializeField]
		[FieldCondition("m_ScaleMode", ScaleMode.Advanced, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[AnimationCurveEx("    Multiplier", "")]
		[Tooltip("Defines scale multiplier, depending on the Relative Distance (between 0 and 1) of a point on the path")]
		private AnimationCurve m_ScaleCurveX = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		[SerializeField]
		[FieldCondition("m_ScaleUniform", false, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[Tooltip("The (base) value of the scaling along the cross section's Y axis")]
		private float m_ScaleY = 1f;

		[SerializeField]
		[FieldCondition("m_ScaleUniform", false, false, ConditionalAttribute.OperatorEnum.AND, "m_ScaleMode", ScaleMode.Advanced, false)]
		[AnimationCurveEx("    Multiplier", "")]
		[Tooltip("Defines scale multiplier, depending on the Relative Distance (between 0 and 1) of a point on the path")]
		private AnimationCurve m_ScaleCurveY = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public ScaleMode ScaleMode
		{
			get
			{
				return m_ScaleMode;
			}
			set
			{
				if (m_ScaleMode != value)
				{
					m_ScaleMode = value;
					base.Dirty = true;
				}
			}
		}

		public CGReferenceMode ScaleReference
		{
			get
			{
				return m_ScaleReference;
			}
			set
			{
				if (m_ScaleReference != value)
				{
					m_ScaleReference = value;
					base.Dirty = true;
				}
			}
		}

		public bool ScaleUniform
		{
			get
			{
				return m_ScaleUniform;
			}
			set
			{
				if (m_ScaleUniform != value)
				{
					m_ScaleUniform = value;
					base.Dirty = true;
				}
			}
		}

		public float ScaleOffset
		{
			get
			{
				return m_ScaleOffset;
			}
			set
			{
				if (m_ScaleOffset != value)
				{
					m_ScaleOffset = value;
					base.Dirty = true;
				}
			}
		}

		public float ScaleX
		{
			get
			{
				return m_ScaleX;
			}
			set
			{
				if (m_ScaleX != value)
				{
					m_ScaleX = value;
					base.Dirty = true;
				}
			}
		}

		public AnimationCurve ScaleMultiplierX
		{
			get
			{
				return m_ScaleCurveX;
			}
			set
			{
				if (m_ScaleCurveX != value)
				{
					m_ScaleCurveX = value;
					base.Dirty = true;
				}
			}
		}

		public float ScaleY
		{
			get
			{
				return m_ScaleY;
			}
			set
			{
				if (m_ScaleY != value)
				{
					m_ScaleY = value;
					base.Dirty = true;
				}
			}
		}

		public AnimationCurve ScaleMultiplierY
		{
			get
			{
				return m_ScaleCurveY;
			}
			set
			{
				if (m_ScaleCurveY != value)
				{
					m_ScaleCurveY = value;
					base.Dirty = true;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();
			ScaleMode = ScaleMode.Simple;
			ScaleUniform = true;
			ScaleX = 1f;
			ScaleY = 1f;
			ScaleMultiplierX = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			ScaleMultiplierY = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			ScaleReference = CGReferenceMode.Self;
			ScaleOffset = 0f;
		}

		public Vector2 GetScale(float relativeDistance)
		{
			return GetScale(relativeDistance, ScaleMode, ScaleOffset, ScaleUniform, ScaleX, ScaleMultiplierX, ScaleY, ScaleMultiplierY);
		}

		protected Vector2 GetScale(int sampleIndex, SubArray<float> relativeDistances, SubArray<float> sourceRelativeDistances)
		{
			return ScaleMode switch
			{
				ScaleMode.Advanced => GetAdvancedScale(GetRelativeDistance(sampleIndex, ScaleReference, relativeDistances, sourceRelativeDistances), ScaleOffset, ScaleUniform, ScaleX, ScaleMultiplierX, ScaleY, ScaleMultiplierY), 
				ScaleMode.Simple => GetSimpleScale(ScaleUniform, ScaleX, ScaleY), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		protected static Vector2 GetScale(float relativeDistance, ScaleMode mode, float offset, bool isUniform, float scaleX, AnimationCurve scaleMultiplierX, float scaleY, AnimationCurve scaleMultiplierY)
		{
			return mode switch
			{
				ScaleMode.Advanced => GetAdvancedScale(relativeDistance, offset, isUniform, scaleX, scaleMultiplierX, scaleY, scaleMultiplierY), 
				ScaleMode.Simple => GetSimpleScale(isUniform, scaleX, scaleY), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		protected static float GetRelativeDistance(int sampleIndex, CGReferenceMode cgReferenceMode, SubArray<float> relativeDistances, SubArray<float> sourceRelativeDistances)
		{
			return (cgReferenceMode switch
			{
				CGReferenceMode.Source => sourceRelativeDistances, 
				CGReferenceMode.Self => relativeDistances, 
				_ => throw new ArgumentOutOfRangeException(), 
			}).Array[sampleIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static Vector2 GetAdvancedScale(float relativeDistance, float scaleOffset, bool isUniform, float scaleX, AnimationCurve scaleMultiplierX, float scaleY, AnimationCurve scaleMultiplierY)
		{
			float time = DTMath.Repeat(relativeDistance - scaleOffset, 1f);
			Vector2 result = default(Vector2);
			float num = (result.x = scaleX * scaleMultiplierX.Evaluate(time));
			result.y = (isUniform ? num : (scaleY * scaleMultiplierY.Evaluate(time)));
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static Vector2 GetSimpleScale(bool isUniform, float scaleX, float scaleY)
		{
			Vector2 result = default(Vector2);
			result.x = scaleX;
			result.y = (isUniform ? scaleX : scaleY);
			return result;
		}
	}
}
