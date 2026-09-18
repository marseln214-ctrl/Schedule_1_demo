using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Variable Mix Shapes", ModuleName = "Variable Mix Shapes", Description = "Interpolates between two shapes in a way that varies along the shape extrusion")]
	[HelpURL("https://curvyeditor.com/doclink/cgvariablemixshapes")]
	public class ModifierVariableMixShapes : CGModule, IOnRequestProcessing, IPathProvider
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape A")]
		public CGModuleInputSlot InShapeA = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape B")]
		public CGModuleInputSlot InShapeB = new CGModuleInputSlot();

		[HideInInspector]
		[ShapeOutputSlotInfo(OutputsVariableShape = true, Array = true, ArrayType = SlotInfo.SlotArrayType.Hidden)]
		public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

		[Label("Mix Curve", "Mix between the shapes. Values (Y axis) between -1 for Shape A and 1 for Shape B. Times (X axis) between 0 for extrusion start and 1 for extrusion end")]
		[SerializeField]
		private AnimationCurve m_MixCurve = AnimationCurve.Linear(0f, -1f, 1f, 1f);

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured && InShapeA.SourceSlot().PathProvider.PathIsClosed)
				{
					return InShapeB.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		public AnimationCurve MixCurve
		{
			get
			{
				return m_MixCurve;
			}
			set
			{
				if (m_MixCurve != value)
				{
					m_MixCurve = value;
					base.Dirty = true;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();
			m_MixCurve = AnimationCurve.Linear(0f, -1f, 1f, 1f);
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			CGDataRequestShapeRasterization requestParameter = CGModule.GetRequestParameter<CGDataRequestShapeRasterization>(ref requests);
			if (!requestParameter)
			{
				return Array.Empty<CGData>();
			}
			int count = requestParameter.RelativeDistances.Count;
			CGData[] array = new CGData[count];
			if (count > 0)
			{
				bool isDataDisposable;
				CGShape data = InShapeA.GetData<CGShape>(out isDataDisposable, requests);
				bool isDataDisposable2;
				CGShape data2 = InShapeB.GetData<CGShape>(out isDataDisposable2, requests);
				for (int i = 0; i < count; i++)
				{
					float mix = MixCurve.Evaluate(requestParameter.RelativeDistances.Array[i]);
					array[i] = ModifierMixShapes.MixShapes(data, data2, mix, UIMessages, i != 0);
				}
				if (isDataDisposable)
				{
					data.Dispose();
				}
				if (isDataDisposable2)
				{
					data2.Dispose();
				}
			}
			return array;
		}
	}
}
