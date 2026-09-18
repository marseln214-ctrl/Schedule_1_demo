using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator.Modules
{
	[ModuleInfo("Modifier/Mix Paths", ModuleName = "Mix Paths", Description = "Interpolates between two paths")]
	[HelpURL("https://curvyeditor.com/doclink/cgmixpaths")]
	public class ModifierMixPaths : CGModule, IOnRequestProcessing, IPathProvider
	{
		private const int MixMinValue = -1;

		private const int MixMaxValue = 1;

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path A")]
		public CGModuleInputSlot InPathA = new CGModuleInputSlot();

		[HideInInspector]
		[InputSlotInfo(new Type[] { typeof(CGPath) }, Name = "Path B")]
		public CGModuleInputSlot InPathB = new CGModuleInputSlot();

		[HideInInspector]
		[OutputSlotInfo(typeof(CGPath))]
		public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

		[SerializeField]
		[RangeEx(-1f, 1f, "", "", Tooltip = "Mix between the paths. Values between -1 for Path A and 1 for Path B")]
		private float m_Mix;

		public float Mix
		{
			get
			{
				return m_Mix;
			}
			set
			{
				float num = Mathf.Clamp(value, -1f, 1f);
				if (m_Mix != num)
				{
					m_Mix = num;
					base.Dirty = true;
				}
			}
		}

		public bool PathIsClosed
		{
			get
			{
				if (IsConfigured && InPathA.SourceSlot().PathProvider.PathIsClosed)
				{
					return InPathB.SourceSlot().PathProvider.PathIsClosed;
				}
				return false;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Properties.MinWidth = 250f;
			Properties.LabelWidth = 50f;
		}

		public override void Reset()
		{
			base.Reset();
			Mix = 0f;
		}

		public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
		{
			if (!CGModule.GetRequestParameter<CGDataRequestRasterization>(ref requests))
			{
				return Array.Empty<CGData>();
			}
			bool isDataDisposable;
			CGPath data = InPathA.GetData<CGPath>(out isDataDisposable, requests);
			bool isDataDisposable2;
			CGPath data2 = InPathB.GetData<CGPath>(out isDataDisposable2, requests);
			CGPath cGPath = MixPath(data, data2, Mix, UIMessages);
			if (isDataDisposable)
			{
				data.Dispose();
			}
			if (isDataDisposable2)
			{
				data2.Dispose();
			}
			if (cGPath != null)
			{
				return new CGData[1] { cGPath };
			}
			return Array.Empty<CGData>();
		}

		[CanBeNull]
		public static CGPath MixPath([CanBeNull] CGPath pathA, [CanBeNull] CGPath pathB, float mix, [NotNull] List<string> warningsContainer)
		{
			if (pathA == null)
			{
				return pathB;
			}
			if (pathB == null)
			{
				return pathA;
			}
			int num = Mathf.Max(pathA.Count, pathB.Count);
			CGPath cGPath = new CGPath();
			ModifierMixShapes.InterpolateShape(cGPath, pathA, pathB, mix, warningsContainer);
			float t = (mix + 1f) * 0.5f;
			SubArray<Vector3> directions = ArrayPools.Vector3.Allocate(num);
			if (pathA.Count == num)
			{
				for (int i = 0; i < num; i++)
				{
					float frag;
					int fIndex = pathB.GetFIndex(pathA.RelativeDistances.Array[i], out frag);
					Vector3 b = Vector3.SlerpUnclamped(pathB.Directions.Array[fIndex], pathB.Directions.Array[fIndex + 1], frag);
					directions.Array[i] = Vector3.SlerpUnclamped(pathA.Directions.Array[i], b, t);
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					float frag2;
					int fIndex2 = pathA.GetFIndex(pathB.RelativeDistances.Array[j], out frag2);
					Vector3 a = Vector3.SlerpUnclamped(pathA.Directions.Array[fIndex2], pathA.Directions.Array[fIndex2 + 1], frag2);
					directions.Array[j] = Vector3.SlerpUnclamped(a, pathB.Directions.Array[j], t);
				}
			}
			cGPath.Directions = directions;
			return cGPath;
		}
	}
}
