using System;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Input/Spline Shape", ModuleName = "Input Spline Shape", Description = "Spline Shape")]
	[HelpURL("https://curvyeditor.com/doclink/cginputsplineshape")]
	public class InputSplineShape : SplineInputModuleBase, IExternalInput, IOnRequestProcessing, IPathProvider
	{
		[HideInInspector]
		[OutputSlotInfo(typeof(CGShape))]
		public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

		[Tab("General", Sort = 0)]
		[SerializeField]
		[CGResourceManager("Shape")]
		[FieldCondition("m_Shape", null, false, ActionAttribute.ActionEnum.ShowWarning, "Missing Shape input", ActionAttribute.ActionPositionEnum.Below)]
		private CurvySpline m_Shape;

		public CurvySpline Shape
		{
			get
			{
				return m_Shape;
			}
			set
			{
				if (m_Shape != value)
				{
					m_Shape = value;
					if (base.IsActiveAndEnabled)
					{
						OnSplineAssigned();
					}
					ValidateStartAndEndCps();
					base.Dirty = true;
				}
			}
		}

		public bool SupportsIPE => FreeForm;

		public bool FreeForm
		{
			get
			{
				if (Shape != null)
				{
					return Shape.GetComponent<CurvyShape>() == null;
				}
				return false;
			}
			set
			{
				if (Shape != null)
				{
					CurvyShape component = Shape.GetComponent<CurvyShape>();
					if (value && component != null)
					{
						component.Delete();
					}
					else if (!value && component == null)
					{
						Shape.gameObject.AddComponent<CSCircle>();
					}
				}
			}
		}

		protected override CurvySpline InputSpline
		{
			get
			{
				return Shape;
			}
			set
			{
				Shape = value;
			}
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestRasterization requestParameter = CGModule.GetRequestParameter<CGDataRequestRasterization>(ref requests);
			CGDataRequestMetaCGOptions requestParameter2 = CGModule.GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);
			if (!requestParameter || requestParameter.RasterizedRelativeLength == 0f)
			{
				return Array.Empty<CGData>();
			}
			CGData splineData = GetSplineData(Shape, fullPath: false, requestParameter, requestParameter2);
			if (splineData != null)
			{
				return new CGData[1] { splineData };
			}
			return Array.Empty<CGData>();
		}

		public T SetManagedShape<T>() where T : CurvyShape2D
		{
			if (!Shape)
			{
				Shape = (CurvySpline)AddManagedResource("Shape");
			}
			CurvyShape component = Shape.GetComponent<CurvyShape>();
			if (component != null)
			{
				component.Delete();
			}
			return Shape.gameObject.AddComponent<T>();
		}

		public void RemoveManagedShape()
		{
			if ((bool)Shape)
			{
				DeleteManagedResource("Shape", Shape);
			}
		}

		protected override void OnSplineAssigned()
		{
			base.OnSplineAssigned();
			if ((bool)Shape)
			{
				Shape.RestrictTo2D = true;
			}
		}
	}
}
