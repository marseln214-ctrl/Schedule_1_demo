using System;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Controllers
{
	[AddComponentMenu("Curvy/Controllers/CG Volume Controller")]
	[HelpURL("https://curvyeditor.com/doclink/volumecontroller")]
	public class VolumeController : CurvyController
	{
		private const float CrossPositionRangeMin = -0.5f;

		private const float CrossPositionRangeMax = 0.5f;

		[Section("General", true, false, 100)]
		[CGDataReferenceSelector(typeof(CGVolume), Label = "Volume/Slot")]
		[SerializeField]
		private CGDataReference m_Volume = new CGDataReference();

		[Section("Cross Position", true, false, 100, Sort = 1, HelpURL = "https://curvyeditor.com/doclink/volumecontroller_crossposition")]
		[SerializeField]
		[FloatRegion(UseSlider = true, Precision = 4, RegionOptionsPropertyName = "CrossRangeOptions", Options = AttributeOptionsFlags.Full)]
		private FloatRegion m_CrossRange = new FloatRegion(-0.5f, 0.5f);

		[RangeEx("MinCrossRelativePosition", "MaxCrossRelativePosition", "", "")]
		[SerializeField]
		private float crossRelativePosition;

		[SerializeField]
		private CurvyClamping m_CrossClamping;

		[SerializeField]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("Use crossRelativePosition instead. This field is kept for retro compatibility reasons")]
		private float m_CrossInitialPosition;

		public CGDataReference Volume
		{
			get
			{
				return m_Volume;
			}
			set
			{
				m_Volume = value;
			}
		}

		[CanBeNull]
		public CGVolume VolumeData
		{
			get
			{
				if (!Volume.HasValue)
				{
					return null;
				}
				return Volume.GetData<CGVolume>();
			}
		}

		public float CrossFrom
		{
			get
			{
				return m_CrossRange.From;
			}
			set
			{
				m_CrossRange.From = Mathf.Clamp(value, -0.5f, 0.5f);
			}
		}

		public float CrossTo
		{
			get
			{
				return m_CrossRange.To;
			}
			set
			{
				m_CrossRange.To = Mathf.Clamp(value, CrossFrom, 0.5f);
			}
		}

		public float CrossLength => m_CrossRange.Length;

		public CurvyClamping CrossClamping
		{
			get
			{
				return m_CrossClamping;
			}
			set
			{
				m_CrossClamping = value;
			}
		}

		public float CrossRelativePosition
		{
			get
			{
				return GetClampedCrossPosition(crossRelativePosition);
			}
			set
			{
				crossRelativePosition = GetClampedCrossPosition(value);
			}
		}

		public override float Length
		{
			get
			{
				if (VolumeData == null)
				{
					return 0f;
				}
				return VolumeData.Length;
			}
		}

		public override bool IsReady
		{
			get
			{
				if (Volume != null && !Volume.IsEmpty)
				{
					return Volume.HasValue;
				}
				return false;
			}
		}

		private RegionOptions<float> CrossRangeOptions => RegionOptions<float>.MinMax(-0.5f, 0.5f);

		private float MinCrossRelativePosition => m_CrossRange.From;

		private float MaxCrossRelativePosition => m_CrossRange.To;

		public float CrossRelativeToAbsolute(float relativeDistance)
		{
			if (VolumeData == null)
			{
				return 0f;
			}
			return VolumeData.CrossFToDistance(base.RelativePosition, relativeDistance, CrossClamping);
		}

		public float CrossAbsoluteToRelative(float worldUnitDistance)
		{
			if (VolumeData == null)
			{
				return 0f;
			}
			return VolumeData.CrossDistanceToF(base.RelativePosition, worldUnitDistance, CrossClamping);
		}

		protected override float RelativeToAbsolute(float relativeDistance)
		{
			if (VolumeData == null)
			{
				return 0f;
			}
			return VolumeData.FToDistance(relativeDistance);
		}

		protected override float AbsoluteToRelative(float worldUnitDistance)
		{
			if (VolumeData == null)
			{
				return 0f;
			}
			return VolumeData.DistanceToF(worldUnitDistance);
		}

		protected override Vector3 GetInterpolatedSourcePosition(float tf)
		{
			return Volume.Module.Generator.transform.TransformPoint(VolumeData.InterpolateVolumePosition(tf, CrossRelativePosition));
		}

		protected override void GetInterpolatedSourcePosition(float tf, out Vector3 interpolatedPosition, out Vector3 tangent, out Vector3 up)
		{
			VolumeData.InterpolateVolume(tf, CrossRelativePosition, out interpolatedPosition, out tangent, out up);
			Transform transform = Volume.Module.Generator.transform;
			interpolatedPosition = transform.TransformPoint(interpolatedPosition);
			tangent = transform.TransformDirection(tangent);
			up = transform.TransformDirection(up);
		}

		protected override Vector3 GetTangent(float tf)
		{
			return Volume.Module.Generator.transform.TransformDirection(VolumeData.InterpolateVolumeDirection(tf, CrossRelativePosition));
		}

		protected override Vector3 GetOrientation(float tf)
		{
			return Volume.Module.Generator.transform.TransformDirection(VolumeData.InterpolateVolumeUp(tf, CrossRelativePosition));
		}

		protected override void Advance(float speed, float deltaTime)
		{
			float tf = base.RelativePosition;
			MovementDirection direction = base.MovementDirection;
			SimulateAdvance(ref tf, ref direction, speed, deltaTime);
			base.MovementDirection = direction;
			base.RelativePosition = tf;
		}

		protected override void SimulateAdvance(ref float tf, ref MovementDirection direction, float speed, float deltaTime)
		{
			int direction2 = direction.ToInt();
			switch (base.MoveMode)
			{
			case MoveModeEnum.Relative:
				VolumeData.Move(ref tf, ref direction2, speed * deltaTime, base.Clamping);
				break;
			case MoveModeEnum.AbsolutePrecise:
				VolumeData.MoveBy(ref tf, ref direction2, speed * deltaTime, base.Clamping);
				break;
			default:
				throw new NotSupportedException();
			}
			direction = MovementDirectionMethods.FromInt(direction2);
		}

		private float GetClampedCrossPosition(float position)
		{
			return CurvyUtility.ClampValue(position, CrossClamping, CrossFrom, CrossTo);
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			if (!float.IsNaN(m_CrossInitialPosition))
			{
				crossRelativePosition = DTMath.MapValue(CrossFrom, CrossTo, m_CrossInitialPosition, -0.5f, 0.5f);
				m_CrossInitialPosition = float.NaN;
			}
		}
	}
}
