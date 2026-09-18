using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/TRS Path", ModuleName = "TRS Path", Description = "Transform,Rotate,Scale a Path")]
	[HelpURL("https://curvyeditor.com/doclink/cgtrspath")]
	public class ModifierTRSPath : TRSModuleBase, IOnRequestProcessing, IPathProvider
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path A", ModifiesData = true)]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured)
				{
					return InPath.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (requestedSlot != OutPath)
			{
				return Array.Empty<CGData>();
			}
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, requests);
			if (data == null)
			{
				return Array.Empty<CGData>();
			}
			Matrix4x4 matrix4x = ApplyTrsOnShape(data);
			for (int i = 0; i < data.Count; i++)
			{
				data.Directions.Array[i] = matrix4x.MultiplyVector(data.Directions.Array[i]);
			}
			return new CGData[1] { data };
		}
	}
}
