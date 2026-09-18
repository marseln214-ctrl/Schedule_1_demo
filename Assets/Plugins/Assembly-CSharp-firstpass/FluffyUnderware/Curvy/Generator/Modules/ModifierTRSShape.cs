using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/TRS Shape", ModuleName = "TRS Shape", Description = "Transform,Rotate,Scale a Shape")]
	[HelpURL("https://curvyeditor.com/doclink/cgtrsshape")]
	public class ModifierTRSShape : TRSModuleBase, IOnRequestProcessing, IPathProvider
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGShape) }, Name = "Shape A", ModifiesData = true)]
		public CGModuleInputSlot InShape = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGShape))]
		public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured)
				{
					return InShape.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (requestedSlot != OutShape)
			{
				return Array.Empty<CGData>();
			}
			bool isDataDisposable;
			CGShape data = InShape.GetData<CGShape>(out isDataDisposable, requests);
			if (data == null)
			{
				return Array.Empty<CGData>();
			}
			ApplyTrsOnShape(data);
			return new CGData[1] { data };
		}
	}
}
