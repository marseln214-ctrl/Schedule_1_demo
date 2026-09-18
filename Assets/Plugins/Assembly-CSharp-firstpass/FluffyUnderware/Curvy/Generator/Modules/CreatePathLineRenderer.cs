using System;
using FluffyUnderware.Curvy.Utils;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Create/Path Line Renderer", ModuleName = "Create Path Line Renderer", Description = "Feeds a Line Renderer with a Path")]
	[RequireComponent(typeof(LineRenderer))]
	[HelpURL("https://curvyeditor.com/doclink/cgcreatepathlinerenderer")]
	public class CreatePathLineRenderer : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, DisplayName = "Rasterized Path")]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		private LineRenderer mLineRenderer;

		public LineRenderer LineRenderer
		{
			get
			{
				if (mLineRenderer == null)
				{
					mLineRenderer = GetComponent<LineRenderer>();
				}
				return mLineRenderer;
			}
		}

		public override void Reset()
		{
			base.Reset();
			LineRenderer.useWorldSpace = false;
			LineRenderer.textureMode = LineTextureMode.Tile;
			LineRenderer.sharedMaterial = CurvyUtility.GetDefaultMaterial();
		}

		public override void Refresh()
		{
			base.Refresh();
			bool isDataDisposable;
			CGPath data = InPath.GetData<CGPath>(out isDataDisposable, Array.Empty<CGDataRequestParameter>());
			if (data != null)
			{
				LineRenderer.useWorldSpace = false;
				LineRenderer.positionCount = data.Positions.Count;
				LineRenderer.SetPositions(data.Positions.Array);
			}
			else
			{
				LineRenderer.positionCount = 0;
			}
			if (isDataDisposable)
			{
				data.Dispose();
			}
		}
	}
}
