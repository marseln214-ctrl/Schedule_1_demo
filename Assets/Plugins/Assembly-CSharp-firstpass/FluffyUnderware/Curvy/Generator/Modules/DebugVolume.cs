using System;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Debug/Volume", ModuleName = "Debug Volume")]
	[HelpURL("https://curvyeditor.com/doclink/cgdebugvolume")]
	public class DebugVolume : CGModule
	{
		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGVolume) }, Name = "Volume")]
		public CGModuleInputSlot InData = new CGModuleInputSlot();

		[Tab("General")]
		public bool ShowPathSamples = true;

		public bool ShowCrossSamples = true;

		[FieldCondition("ShowCrossSamples", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[IntRegion(RegionIsOptional = true)]
		public IntRegion LimitCross = new IntRegion(0, 0);

		public bool ShowNormals;

		public bool ShowIndex;

		public bool ShowMap;

		public Color PathColor = Color.white;

		public Color VolumeColor = Color.gray;

		public Color NormalColor = Color.yellow;

		[Tab("Interpolate")]
		public bool Interpolate;

		[RangeEx(-1f, 1f, "Path", "")]
		public float InterpolatePathF;

		[RangeEx(-1f, 1f, "Cross", "")]
		public float InterpolateCrossF;

		public override void Reset()
		{
			base.Reset();
			ShowPathSamples = true;
			ShowCrossSamples = true;
			LimitCross = new IntRegion(0, 0);
			ShowNormals = false;
			ShowIndex = false;
			ShowMap = false;
			PathColor = Color.white;
			VolumeColor = Color.gray;
			NormalColor = Color.yellow;
			Interpolate = false;
			InterpolatePathF = 0f;
			InterpolateCrossF = 0f;
		}
	}
}
