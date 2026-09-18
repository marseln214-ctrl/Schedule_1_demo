using System;
using FluffyUnderware.Curvy.Pools;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[AddComponentMenu("Curvy/Converters/Curvy Spline To Edge Collider 2D")]
	[RequireComponent(typeof(EdgeCollider2D))]
	[HelpURL("https://curvyeditor.com/doclink/edgecollider2d")]
	public class CurvySplineToEdgeCollider2D : SplineProcessor
	{
		public const string ComponentPath = "Curvy/Converters/Curvy Spline To Edge Collider 2D";

		private EdgeCollider2D cachedEdgeCollider2D;

		private EdgeCollider2D EdgeCollider
		{
			get
			{
				if (cachedEdgeCollider2D == null)
				{
					cachedEdgeCollider2D = GetComponent<EdgeCollider2D>();
				}
				return cachedEdgeCollider2D;
			}
		}

		public override void Refresh()
		{
			if (!base.Spline)
			{
				return;
			}
			if (base.Spline.IsInitialized && !base.Spline.Dirty)
			{
				SubArray<Vector3> positionsCache = base.Spline.GetPositionsCache(Space.Self);
				SubArray<Vector2> subArray = ArrayPools.Vector2.AllocateExactSize(positionsCache.Count);
				Vector3[] array = positionsCache.Array;
				Vector2[] array2 = subArray.Array;
				for (int i = 0; i < positionsCache.Count; i++)
				{
					array2[i].x = array[i].x;
					array2[i].y = array[i].y;
				}
				EdgeCollider.points = array2;
				ArrayPools.Vector2.Free(subArray);
				ArrayPools.Vector3.Free(positionsCache);
			}
			else
			{
				EdgeCollider.points = Array.Empty<Vector2>();
			}
		}
	}
}
