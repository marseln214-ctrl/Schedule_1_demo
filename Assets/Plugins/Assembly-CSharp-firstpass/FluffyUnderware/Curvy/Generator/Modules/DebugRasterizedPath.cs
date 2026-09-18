using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Debug/Rasterized Path", ModuleName = "Debug Rasterized Path", Description = "Shows the tangents and orientation of a rasterized path")]
	[HelpURL("https://curvyeditor.com/doclink/cgdebugrasterizedpath")]
	public class DebugRasterizedPath : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, DisplayName = "Rasterized Path")]
		public CGModuleInputSlot InPath = new CGModuleInputSlot();

		[Tooltip("Display the normal at each one of the path's points")]
		public bool ShowNormals = true;

		[Tooltip("Display the orientation at each one of the path's points")]
		public bool ShowOrientation = true;

		public override void Reset()
		{
			base.Reset();
			ShowNormals = (ShowOrientation = true);
		}
	}
}
