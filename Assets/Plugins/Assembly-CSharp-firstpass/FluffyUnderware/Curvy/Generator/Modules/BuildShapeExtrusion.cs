using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Shape Extrusion", ModuleName = "Shape Extrusion", Description = "Simple Shape Extrusion")]
	[HelpURL("https://curvyeditor.com/doclink/cgbuildshapeextrusion")]
	public class BuildShapeExtrusion : ScalingModule, IPathProvider
	{
		public enum CrossShiftModeEnum
		{
			None = 0,
			ByOrientation = 1,
			Custom = 2
		}

		[UsedImplicitly]
		[Obsolete("Use FluffyUnderware.Curvy.Generator.Modules.ScaleMode instead")]
		public enum ScaleModeEnum
		{
			Simple = 0,
			Advanced = 1
		}

		public struct Statistics : IEquatable<Statistics>
		{
			public int PathSampleCount
			{
				get; [UsedImplicitly]
				[Obsolete]
				set;
			}

			public int CrossSampleCount
			{
				get; [UsedImplicitly]
				[Obsolete]
				set;
			}

			public int MaterialGroupsCount
			{
				get; [UsedImplicitly]
				[Obsolete]
				set;
			}

			public void Set(int pathSamples, int crossSamples, int crossGroups)
			{
				PathSampleCount = pathSamples;
				CrossSampleCount = crossSamples;
				MaterialGroupsCount = crossGroups;
			}

			public bool Equals(Statistics other)
			{
				if (PathSampleCount == other.PathSampleCount && CrossSampleCount == other.CrossSampleCount)
				{
					return MaterialGroupsCount == other.MaterialGroupsCount;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is Statistics other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((PathSampleCount * 397) ^ CrossSampleCount) * 397) ^ MaterialGroupsCount;
			}

			public static bool operator ==(Statistics left, Statistics right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Statistics left, Statistics right)
			{
				return !left.Equals(right);
			}
		}

		private const int MinResolution = 1;

		private const int MaxResolution = 100;

		private const float MinAngleThreshold = 0.1f;

		private const float MaxAngleThreshold = 120f;

		private const int MinShiftValue = 0;

		private const int MaxShiftValue = 1;

		private const int MinHollowInset = 0;

		private const int MaxHollowInset = 1;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, RequestDataOnly = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Array = true, ArrayType = SlotInfo.SlotArrayType.Hidden, RequestDataOnly = true)]
		public CGModuleInputSlot InCross = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVolume))]
		public CGModuleOutputSlot OutVolume = new CGModuleOutputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGVolume))]
		public CGModuleOutputSlot OutVolumeHollow = new CGModuleOutputSlot();

		[Tab("Path")]
		[FloatRegion(UseSlider = true, RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
		[SerializeField]
		private FloatRegion m_Range = FloatRegion.ZeroOne;

		[SerializeField]
		[RangeEx(1f, 100f, "Resolution", "Defines how densely the path spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's Max Points Per Unit")]
		private int m_Resolution = 50;

		[SerializeField]
		private bool m_Optimize = true;

		[FieldCondition("m_Optimize", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[RangeEx(0.1f, 120f, "", "", Tooltip = "Max angle")]
		private float m_AngleThreshold = 10f;

		[Tab("Cross")]
		[FieldAction("CBEditCrossButton", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Above)]
		[FloatRegion(UseSlider = true, RegionOptionsPropertyName = "CrossRangeOptions", Precision = 4)]
		[SerializeField]
		private FloatRegion m_CrossRange = FloatRegion.ZeroOne;

		[SerializeField]
		[RangeEx(1f, 100f, "Resolution", "", Tooltip = "Defines how densely the cross spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's Max Points Per Unit")]
		private int m_CrossResolution = 50;

		[SerializeField]
		[Label("Optimize", "")]
		private bool m_CrossOptimize = true;

		[FieldCondition("m_CrossOptimize", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[RangeEx(0.1f, 120f, "Angle Threshold", "", Tooltip = "Max angle")]
		private float m_CrossAngleThreshold = 10f;

		[SerializeField]
		[Label("Include CPs", "")]
		[Tooltip("If enabled, vertices are guaranteed to be created for all the Cross shape's Control Points.")]
		private bool m_CrossIncludeControlpoints;

		[SerializeField]
		[Label("Hard Edges", "")]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		private bool m_CrossHardEdges;

		[SerializeField]
		[Label("Materials", "")]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		private bool m_CrossMaterials;

		[SerializeField]
		[Label("Extended UV", "")]
		[HideInInspector]
		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		private bool m_CrossExtendedUV;

		[SerializeField]
		[Label("Shift", "", Tooltip = "Defines a shift to be applied on the output volume's cross.\r\nThis shift is used when interpolating values (position, normal, ...) along the volume's surface.")]
		private CrossShiftModeEnum m_CrossShiftMode = CrossShiftModeEnum.ByOrientation;

		[SerializeField]
		[RangeEx(0f, 1f, "Value", "Shift By", Slider = true)]
		[FieldCondition("m_CrossShiftMode", CrossShiftModeEnum.Custom, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		private float m_CrossShiftValue;

		[Label("Reverse Normal", "Reverse Vertex Normals?")]
		[SerializeField]
		private bool m_CrossReverseNormals;

		[Tab("Hollow", Sort = 102)]
		[RangeEx(0f, 1f, "", "", Slider = true, Label = "Inset")]
		[SerializeField]
		private float m_HollowInset;

		[Label("Reverse Normal", "Reverse Vertex Normals?")]
		[SerializeField]
		private bool m_HollowReverseNormals;

		public float From
		{
			get
			{
				return m_Range.From;
			}
			set
			{
				float num = Mathf.Repeat(value, 1f);
				if (m_Range.From != num)
				{
					m_Range.From = num;
					base.Dirty = true;
				}
			}
		}

		public float To
		{
			get
			{
				return m_Range.To;
			}
			set
			{
				float num = Mathf.Max(From, value);
				if (ClampPath)
				{
					num = DTMath.Repeat(value, 1f);
				}
				if (m_Range.To != num)
				{
					m_Range.To = num;
					base.Dirty = true;
				}
			}
		}

		public float Length
		{
			get
			{
				if (!ClampPath)
				{
					return m_Range.To;
				}
				return m_Range.To - m_Range.From;
			}
			set
			{
				float num = (ClampPath ? (value - m_Range.To) : value);
				if (m_Range.To != num)
				{
					m_Range.To = num;
					base.Dirty = true;
				}
			}
		}

		public int Resolution
		{
			get
			{
				return m_Resolution;
			}
			set
			{
				int num = Mathf.Clamp(value, 1, 100);
				if (m_Resolution != num)
				{
					m_Resolution = num;
					base.Dirty = true;
				}
			}
		}

		public bool Optimize
		{
			get
			{
				return m_Optimize;
			}
			set
			{
				if (m_Optimize != value)
				{
					m_Optimize = value;
					base.Dirty = true;
				}
			}
		}

		public float AngleThreshold
		{
			get
			{
				return m_AngleThreshold;
			}
			set
			{
				float num = Mathf.Clamp(value, 0.1f, 120f);
				if (m_AngleThreshold != num)
				{
					m_AngleThreshold = num;
					base.Dirty = true;
				}
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
				float num = Mathf.Repeat(value, 1f);
				if (m_CrossRange.From != num)
				{
					m_CrossRange.From = num;
					base.Dirty = true;
				}
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
				float num = Mathf.Max(CrossFrom, value);
				if (ClampCross)
				{
					num = DTMath.Repeat(value, 1f);
				}
				if (m_CrossRange.To != num)
				{
					m_CrossRange.To = num;
					base.Dirty = true;
				}
			}
		}

		public float CrossLength
		{
			get
			{
				if (!ClampCross)
				{
					return m_CrossRange.To;
				}
				return m_CrossRange.To - m_CrossRange.From;
			}
			set
			{
				float num = (ClampCross ? (value - m_CrossRange.To) : value);
				if (m_CrossRange.To != num)
				{
					m_CrossRange.To = num;
					base.Dirty = true;
				}
			}
		}

		public int CrossResolution
		{
			get
			{
				return m_CrossResolution;
			}
			set
			{
				int num = Mathf.Clamp(value, 1, 100);
				if (m_CrossResolution != num)
				{
					m_CrossResolution = num;
					base.Dirty = true;
				}
			}
		}

		public bool CrossOptimize
		{
			get
			{
				return m_CrossOptimize;
			}
			set
			{
				if (m_CrossOptimize != value)
				{
					m_CrossOptimize = value;
					base.Dirty = true;
				}
			}
		}

		public float CrossAngleThreshold
		{
			get
			{
				return m_CrossAngleThreshold;
			}
			set
			{
				float num = Mathf.Clamp(value, 0.1f, 120f);
				if (m_CrossAngleThreshold != num)
				{
					m_CrossAngleThreshold = num;
					base.Dirty = true;
				}
			}
		}

		public bool CrossIncludeControlPoints
		{
			get
			{
				return m_CrossIncludeControlpoints;
			}
			set
			{
				if (m_CrossIncludeControlpoints != value)
				{
					m_CrossIncludeControlpoints = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		public bool CrossHardEdges
		{
			get
			{
				return m_CrossHardEdges;
			}
			set
			{
				if (m_CrossHardEdges != value)
				{
					m_CrossHardEdges = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		public bool CrossMaterials
		{
			get
			{
				return m_CrossMaterials;
			}
			set
			{
				if (m_CrossMaterials != value)
				{
					m_CrossMaterials = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("This option is now always assumed to be true")]
		public bool CrossExtendedUV
		{
			get
			{
				return m_CrossExtendedUV;
			}
			set
			{
				if (m_CrossExtendedUV != value)
				{
					m_CrossExtendedUV = value;
					base.Dirty = true;
				}
			}
		}

		public CrossShiftModeEnum CrossShiftMode
		{
			get
			{
				return m_CrossShiftMode;
			}
			set
			{
				if (m_CrossShiftMode != value)
				{
					m_CrossShiftMode = value;
					base.Dirty = true;
				}
			}
		}

		public float CrossShiftValue
		{
			get
			{
				return m_CrossShiftValue;
			}
			set
			{
				float num = value.Repeat(0f, 1f);
				if (m_CrossShiftValue != num)
				{
					m_CrossShiftValue = num;
					base.Dirty = true;
				}
			}
		}

		public bool CrossReverseNormals
		{
			get
			{
				return m_CrossReverseNormals;
			}
			set
			{
				if (m_CrossReverseNormals != value)
				{
					m_CrossReverseNormals = value;
					base.Dirty = true;
				}
			}
		}

		[UsedImplicitly]
		[Obsolete("Use parent class ScalingModule's ScaleMode instead")]
		public new ScaleModeEnum ScaleMode
		{
			get
			{
				if (base.ScaleMode != 0)
				{
					return ScaleModeEnum.Advanced;
				}
				return ScaleModeEnum.Simple;
			}
			set
			{
				if (value == ScaleModeEnum.Simple)
				{
					base.ScaleMode = FluffyUnderware.Curvy.Generator.Modules.ScaleMode.Simple;
				}
				else
				{
					base.ScaleMode = FluffyUnderware.Curvy.Generator.Modules.ScaleMode.Advanced;
				}
			}
		}

		public float HollowInset
		{
			get
			{
				return m_HollowInset;
			}
			set
			{
				float num = Mathf.Clamp(value, 0f, 1f);
				if (m_HollowInset != num)
				{
					m_HollowInset = num;
					base.Dirty = true;
				}
			}
		}

		public bool HollowReverseNormals
		{
			get
			{
				return m_HollowReverseNormals;
			}
			set
			{
				if (m_HollowReverseNormals != value)
				{
					m_HollowReverseNormals = value;
					base.Dirty = true;
				}
			}
		}

		public IExternalInput Cross
		{
			get
			{
				if (!IsConfigured)
				{
					return null;
				}
				return InCross.SourceSlot().ExternalInput;
			}
		}

		[UsedImplicitly]
		[Obsolete]
		public Vector3 CrossPosition
		{
			get
			{
				if (OutVolume.Data.Length == 0)
				{
					return default(Vector3);
				}
				CGVolume cGVolume = (CGVolume)OutVolume.Data[0];
				if (cGVolume.Positions.Array.Length == 0)
				{
					return default(Vector3);
				}
				return cGVolume.Positions.Array[0];
			}
			protected set
			{
				throw new InvalidOperationException("Property is not settable");
			}
		}

		[UsedImplicitly]
		[Obsolete]
		public Quaternion CrossRotation
		{
			get
			{
				if (OutVolume.Data.Length == 0)
				{
					return default(Quaternion);
				}
				CGVolume cGVolume = (CGVolume)OutVolume.Data[0];
				if (cGVolume.Positions.Array.Length == 0)
				{
					return default(Quaternion);
				}
				return Quaternion.LookRotation(cGVolume.Directions.Array[0], cGVolume.Normals.Array[0]);
			}
			protected set
			{
				throw new InvalidOperationException("Property is not settable");
			}
		}

		public bool PathIsClosed => InPath.SourceSlot().PathProvider.PathIsClosed;

		public Statistics ExtrusionStatistics
		{
			get; [UsedImplicitly]
			[Obsolete]
			set;
		}

		private bool ClampPath
		{
			get
			{
				if (InPath.IsLinked)
				{
					return !InPath.SourceSlot().PathProvider.PathIsClosed;
				}
				return true;
			}
		}

		private bool ClampCross
		{
			get
			{
				if (InCross.IsLinked)
				{
					return !InCross.SourceSlot().PathProvider.PathIsClosed;
				}
				return true;
			}
		}

		private RegionOptions<float> RangeOptions
		{
			get
			{
				if (ClampPath)
				{
					return RegionOptions<float>.MinMax(0f, 1f);
				}
				RegionOptions<float> result = default(RegionOptions<float>);
				result.LabelFrom = "Start";
				result.ClampFrom = DTValueClamping.Min;
				result.FromMin = 0f;
				result.LabelTo = "Length";
				result.ClampTo = DTValueClamping.Range;
				result.ToMin = 0f;
				result.ToMax = 1f;
				return result;
			}
		}

		private RegionOptions<float> CrossRangeOptions
		{
			get
			{
				if (ClampCross)
				{
					return RegionOptions<float>.MinMax(0f, 1f);
				}
				RegionOptions<float> result = default(RegionOptions<float>);
				result.LabelFrom = "Start";
				result.ClampFrom = DTValueClamping.Min;
				result.FromMin = 0f;
				result.LabelTo = "Length";
				result.ClampTo = DTValueClamping.Range;
				result.ToMin = 0f;
				result.ToMax = 1f;
				return result;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use ExtrusionStatistics instead")]
		public int PathSamples
		{
			get
			{
				return ExtrusionStatistics.PathSampleCount;
			}
			private set
			{
				Statistics extrusionStatistics = ExtrusionStatistics;
				extrusionStatistics.PathSampleCount = value;
				ExtrusionStatistics = extrusionStatistics;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use ExtrusionStatistics instead")]
		public int CrossSamples
		{
			get
			{
				return ExtrusionStatistics.CrossSampleCount;
			}
			private set
			{
				Statistics extrusionStatistics = ExtrusionStatistics;
				extrusionStatistics.CrossSampleCount = value;
				ExtrusionStatistics = extrusionStatistics;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use ExtrusionStatistics instead")]
		public int CrossGroups
		{
			get
			{
				return ExtrusionStatistics.MaterialGroupsCount;
			}
			private set
			{
				Statistics extrusionStatistics = ExtrusionStatistics;
				extrusionStatistics.MaterialGroupsCount = value;
				ExtrusionStatistics = extrusionStatistics;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 270f;
			Properties.LabelWidth = 100f;
		}

		public override void Reset()
		{
			base.Reset();
			From = 0f;
			To = 1f;
			Resolution = 50;
			AngleThreshold = 10f;
			Optimize = true;
			CrossFrom = 0f;
			CrossTo = 1f;
			CrossResolution = 50;
			CrossAngleThreshold = 10f;
			CrossOptimize = true;
			CrossIncludeControlPoints = false;
			CrossHardEdges = false;
			CrossMaterials = false;
			CrossShiftMode = CrossShiftModeEnum.ByOrientation;
			HollowInset = 0f;
			CrossExtendedUV = false;
			CrossReverseNormals = false;
			HollowReverseNormals = false;
		}

		public override void Refresh()
		{
			base.Refresh();
			if (Length == 0f)
			{
				OutVolume.ClearData();
				OutVolumeHollow.ClearData();
				return;
			}
			List<CGDataRequestParameter> list = new List<CGDataRequestParameter>();
			list.Add(new CGDataRequestRasterization(From, Length, Resolution, AngleThreshold, Optimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, list.ToArray());
			list.Clear();
			CGDataRequestRasterization item = ((InCross.LinkedSlots.Count != 1 || !(InCross.LinkedSlots[0].Info is ShapeOutputSlotInfo) || !(InCross.LinkedSlots[0].Info as ShapeOutputSlotInfo).OutputsVariableShape || !data) ? new CGDataRequestRasterization(CrossFrom, CrossLength, CrossResolution, CrossAngleThreshold, CrossOptimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even) : new CGDataRequestShapeRasterization(data.RelativeDistances, CrossFrom, CrossLength, CrossResolution, CrossAngleThreshold, CrossOptimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
			list.Add(item);
			list.Add(new CGDataRequestMetaCGOptions(CrossHardEdges, CrossMaterials, CrossIncludeControlPoints, CrossExtendedUV));
			bool isDataDisposable2;
			List<CGShape> allData = InCross.GetAllData<CGShape>(out isDataDisposable2, list.ToArray());
			bool flag = !data || data.Count == 0;
			List<int> source = allData.Select((CGShape c) => c?.Count ?? 0).Distinct().ToList();
			bool flag2;
			if (source.Count() != 1 || source.First() == 0)
			{
				flag2 = true;
				UIMessages.Add("Shape Extrusion: All input Crosses are expected to have the same non zero number of sample points.");
			}
			else
			{
				flag2 = false;
			}
			if (flag || flag2)
			{
				OutVolume.ClearData();
				OutVolumeHollow.ClearData();
				return;
			}
			CGShape cGShape = allData[0];
			CGVolume cGVolume = CGVolume.Get((OutVolume.Data.Length == 0) ? null : (OutVolume.Data[0] as CGVolume), data, cGShape);
			CGVolume cGVolume2 = ((!OutVolumeHollow.IsLinked) ? null : CGVolume.Get((OutVolumeHollow.Data.Length == 0) ? null : (OutVolumeHollow.Data[0] as CGVolume), data, cGShape));
			bool flag3 = cGVolume2;
			ExtrusionStatistics.Set(data.Count, cGShape.Count, cGShape.MaterialGroups.Count);
			int num = 0;
			Vector2[] array = cGVolume.Scales.Array;
			float num2 = ((!CrossReverseNormals) ? 1 : (-1));
			float num3 = ((!HollowReverseNormals) ? 1 : (-1));
			bool flag4 = allData.Count == 1;
			int count = data.Count;
			for (int i = 0; i < count; i++)
			{
				CGShape cGShape2;
				if (flag4)
				{
					cGShape2 = cGShape;
				}
				else
				{
					int index = Mathf.RoundToInt((float)(allData.Count - 1) * data.RelativeDistances.Array[i]);
					cGShape2 = allData[index];
				}
				SubArray<Vector3> positions = cGShape2.Positions;
				SubArray<Vector3> normals = cGShape2.Normals;
				Quaternion q = Quaternion.LookRotation(data.Directions.Array[i], data.Normals.Array[i]);
				float num4 = q.x * 2f;
				float num5 = q.y * 2f;
				float num6 = q.z * 2f;
				float num7 = q.x * num4;
				float num8 = q.y * num5;
				float num9 = q.z * num6;
				float num10 = q.x * num5;
				float num11 = q.x * num6;
				float num12 = q.y * num6;
				float num13 = q.w * num4;
				float num14 = q.w * num5;
				float num15 = q.w * num6;
				Vector2 scale = GetScale(i, data.RelativeDistances, data.SourceRelativeDistances);
				Matrix4x4 matrix4x = Matrix4x4.TRS(data.Positions.Array[i], q, scale);
				Matrix4x4 matrix4x2 = (flag3 ? Matrix4x4.TRS(data.Positions.Array[i], q, scale * (1f - HollowInset)) : default(Matrix4x4));
				array[i].x = scale.x;
				array[i].y = scale.y;
				int count2 = cGShape2.Count;
				for (int j = 0; j < count2; j++)
				{
					cGVolume.Vertices.Array[num] = matrix4x.MultiplyPoint3x4(positions.Array[j]);
					Vector3 vector = normals.Array[j];
					float num16 = (1f - (num8 + num9)) * vector.x + (num10 - num15) * vector.y + (num11 + num14) * vector.z;
					float num17 = (num10 + num15) * vector.x + (1f - (num7 + num9)) * vector.y + (num12 - num13) * vector.z;
					float num18 = (num11 - num14) * vector.x + (num12 + num13) * vector.y + (1f - (num7 + num8)) * vector.z;
					cGVolume.VertexNormals.Array[num].x = num16 * num2;
					cGVolume.VertexNormals.Array[num].y = num17 * num2;
					cGVolume.VertexNormals.Array[num].z = num18 * num2;
					if (flag3)
					{
						cGVolume2.Vertices.Array[num] = matrix4x2.MultiplyPoint3x4(positions.Array[j]);
						cGVolume2.VertexNormals.Array[num].x = num16 * num3;
						cGVolume2.VertexNormals.Array[num].y = num17 * num3;
						cGVolume2.VertexNormals.Array[num].z = num18 * num3;
					}
					num++;
				}
			}
			switch (CrossShiftMode)
			{
			case CrossShiftModeEnum.ByOrientation:
			{
				cGVolume.CrossFShift = 0f;
				for (int k = 0; k < cGShape.Count - 1; k++)
				{
					if (DTMath.RayLineSegmentIntersection(cGVolume.Positions.Array[0], cGVolume.Normals.Array[0], cGVolume.Vertices.Array[k], cGVolume.Vertices.Array[k + 1], out var _, out var frag))
					{
						cGVolume.CrossFShift = DTMath.SnapPrecision(cGVolume.CrossRelativeDistances.Array[k] + (cGVolume.CrossRelativeDistances.Array[k + 1] - cGVolume.CrossRelativeDistances.Array[k]) * frag, 2);
						break;
					}
				}
				break;
			}
			case CrossShiftModeEnum.Custom:
				cGVolume.CrossFShift = CrossShiftValue;
				break;
			case CrossShiftModeEnum.None:
				cGVolume.CrossFShift = 0f;
				break;
			default:
				throw new ArgumentOutOfRangeException("CrossShiftMode");
			}
			if (flag3)
			{
				cGVolume2.CrossFShift = cGVolume.CrossFShift;
			}
			OutVolume.SetDataToElement(cGVolume);
			if (flag3)
			{
				OutVolumeHollow.SetDataToElement(cGVolume2);
			}
			else
			{
				OutVolumeHollow.ClearData();
			}
			if (isDataDisposable)
			{
				data.Dispose();
			}
			if (isDataDisposable2)
			{
				allData.ForEach(delegate(CGShape c)
				{
					c.Dispose();
				});
			}
		}

		[UsedImplicitly]
		[Obsolete("Use parent class ScalingModule's GetScale instead")]
		public new Vector3 GetScale(float relativeDistance)
		{
			Vector2 scale = base.GetScale(relativeDistance);
			return new Vector3(scale.x, scale.y, 1f);
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			ExtrusionStatistics = default(Statistics);
		}
	}
}
