using System;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Build/Rasterize Path", ModuleName = "Rasterize Path", Description = "Rasterizes a virtual path")]
	[HelpURL("https://curvyeditor.com/doclink/cgbuildrasterizedpath")]
	public class BuildRasterizedPath : CGModule, IPathProvider
	{
		private const int MinResolution = 1;

		private const int MaxResolution = 100;

		private const float MinAngleThreshold = 0.1f;

		private const float MaxAngleThreshold = 120f;

		private const int DefaultResolution = 50;

		private const int DefaultAngleThreshold = 10;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path", RequestDataOnly = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath), Name = "Path", DisplayName = "Rasterized Path")]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		[FloatRegion(UseSlider = true, RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
		[SerializeField]
		private FloatRegion m_Range = FloatRegion.ZeroOne;

		[SerializeField]
		[RangeEx(1f, 100f, "Resolution", "Defines how densely the path spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's Max Points Per Unit")]
		private int m_Resolution = 50;

		[SerializeField]
		private bool m_Optimize;

		[FieldCondition("m_Optimize", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[RangeEx(0.1f, 120f, "", "")]
		private float m_AngleTreshold = 10f;

		[SerializeField]
		[Section("Backward Compatibility", false, false, 100)]
		[Tooltip("Curvy versions prior to 8.0.0 had a bug in the computation of the rasterization range for closed splines. Enable this value to keep that bugged behaviour if your project depends on it")]
		private bool useBuggedRange;

		public float From
		{
			get
			{
				return m_Range.From;
			}
			set
			{
				float num = DTMath.Repeat(value, 1f);
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
					num = Mathf.Repeat(value, 1f);
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
				return m_AngleTreshold;
			}
			set
			{
				float num = Mathf.Clamp(value, 0.1f, 120f);
				if (m_AngleTreshold != num)
				{
					m_AngleTreshold = num;
					base.Dirty = true;
				}
			}
		}

		[CanBeNull]
		public CGPath Path
		{
			get
			{
				if (OutPath.Data.Length != 0)
				{
					return OutPath.Data[0] as CGPath;
				}
				return null;
			}
		}

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured)
				{
					return InPath.SourceSlot().PathProvider.PathIsClosed;
				}
				return true;
			}
		}

		public bool UseBuggedRange
		{
			get
			{
				return useBuggedRange;
			}
			set
			{
				if (useBuggedRange != value)
				{
					useBuggedRange = value;
					base.Dirty = true;
				}
			}
		}

		private bool ClampPath
		{
			get
			{
				if (!UseBuggedRange)
				{
					return !PathIsClosed;
				}
				return PathIsClosed;
			}
		}

		private RegionOptions<float> RangeOptions
		{
			get
			{
				if (!PathIsClosed)
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

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 112f;
		}

		public override void Reset()
		{
			base.Reset();
			m_Range = FloatRegion.ZeroOne;
			Resolution = 50;
			AngleThreshold = 10f;
			OutPath.ClearData();
			Optimize = false;
		}

		public override void Refresh()
		{
			base.Refresh();
			if (Length == 0f)
			{
				OutPath.ClearData();
				return;
			}
			List<CGDataRequestParameter> list = new List<CGDataRequestParameter>();
			list.Add(new CGDataRequestRasterization(From, Length, Resolution, AngleThreshold, Optimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, list.ToArray());
			if (data == null)
			{
				OutPath.ClearData();
			}
			else
			{
				OutPath.SetDataToElement(data);
			}
		}
	}
}
