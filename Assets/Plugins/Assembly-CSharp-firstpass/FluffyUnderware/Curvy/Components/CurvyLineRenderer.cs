using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[AddComponentMenu("Curvy/Converters/Curvy Line Renderer")]
	[RequireComponent(typeof(LineRenderer))]
	[HelpURL("https://curvyeditor.com/doclink/curvylinerenderer")]
	public class CurvyLineRenderer : SplineProcessor
	{
		public const string ComponentPath = "Curvy/Converters/Curvy Line Renderer";

		private LineRenderer cachedLineRenderer;

		private LineRenderer LineRenderer
		{
			get
			{
				if (cachedLineRenderer == null)
				{
					cachedLineRenderer = GetComponent<LineRenderer>();
				}
				return cachedLineRenderer;
			}
		}

		[UsedImplicitly]
		private void Update()
		{
			EnforceWorldSpaceUsage();
		}

		private void EnforceWorldSpaceUsage()
		{
			if (!LineRenderer.useWorldSpace)
			{
				LineRenderer.useWorldSpace = true;
				DTLog.Log("[Curvy] CurvyLineRenderer: Line Renderer's Use World Space was overriden to true. It is required by the CurvyLineRenderer.", this);
			}
		}

		public override void Refresh()
		{
			if ((bool)base.Spline)
			{
				EnforceWorldSpaceUsage();
				if (base.Spline.IsInitialized && !base.Spline.Dirty)
				{
					SubArray<Vector3> positionsCache = base.Spline.GetPositionsCache(Space.World);
					LineRenderer.positionCount = positionsCache.Count;
					LineRenderer.SetPositions(positionsCache.Array);
					ArrayPools.Vector3.Free(positionsCache);
				}
				else
				{
					LineRenderer.positionCount = 0;
				}
			}
		}
	}
}
