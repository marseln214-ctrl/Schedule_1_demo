using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Spline Path", ModuleName = "Input Spline Path", Description = "Spline Path")]
	[HelpURL("https://curvyeditor.com/doclink/cginputsplinepath")]
	public class InputSplinePath : SplineInputModuleBase, IExternalInput, IOnRequestProcessing, IPathProvider
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot Path = new CGModuleOutputSlot();

		[Tab("General", Sort = 0)]
		[SerializeField]
		[CGResourceManager("Spline")]
		[FieldCondition("m_Spline", null, false, ActionAttribute.ActionEnum.ShowWarning, "Missing Spline input", ActionAttribute.ActionPositionEnum.Below)]
		private CurvySpline m_Spline;

		public CurvySpline Spline
		{
			get
			{
				return m_Spline;
			}
			set
			{
				if (m_Spline != value)
				{
					m_Spline = value;
					if (base.IsActiveAndEnabled)
					{
						OnSplineAssigned();
					}
					ValidateStartAndEndCps();
					base.Dirty = true;
				}
			}
		}

		public bool SupportsIPE => false;

		protected override CurvySpline InputSpline
		{
			get
			{
				return Spline;
			}
			set
			{
				Spline = value;
			}
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestRasterization requestParameter = CGModule.GetRequestParameter<CGDataRequestRasterization>(ref requests);
			CGDataRequestMetaCGOptions requestParameter2 = CGModule.GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);
			if ((bool)requestParameter2)
			{
				UIMessages.Add("Meta CG Options are not supported for Path rasterization. They are supported only for Shape rasterization.");
			}
			if (!requestParameter || requestParameter.RasterizedRelativeLength == 0f)
			{
				return Array.Empty<CGData>();
			}
			CGData splineData = GetSplineData(Spline, fullPath: true, requestParameter, requestParameter2);
			if (splineData != null)
			{
				return new CGData[1] { splineData };
			}
			return Array.Empty<CGData>();
		}

		public override void OnTemplateCreated()
		{
			base.OnTemplateCreated();
			if ((bool)Spline && !IsManagedResource(Spline))
			{
				Spline = null;
			}
		}
	}
}
