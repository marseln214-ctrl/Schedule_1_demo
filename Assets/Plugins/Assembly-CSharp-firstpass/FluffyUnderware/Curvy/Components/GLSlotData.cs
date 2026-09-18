using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using ToolBuddy.Pooling.Collections;
using ToolBuddy.Pooling.Pools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Components
{
	[Serializable]
	public class GLSlotData
	{
		[SerializeField]
		public CurvySpline Spline;

		public Color LineColor = CurvyGlobalManager.DefaultGizmoColor;

		public List<Vector3[]> VertexData = new List<Vector3[]>();

		public void GetVertexData()
		{
			VertexData.Clear();
			ArrayPool<Vector3> vector = ArrayPools.Vector3;
			if (Spline.IsInitialized)
			{
				if (Spline.Dirty)
				{
					Spline.Refresh();
				}
				SubArray<Vector3> positionsCache = Spline.GetPositionsCache(Space.World);
				VertexData.Add(positionsCache.CopyToArray(vector));
				vector.Free(positionsCache);
			}
		}

		public void Render(Material mat)
		{
			for (int i = 0; i < VertexData.Count; i++)
			{
				if (VertexData[i].Length != 0)
				{
					mat.SetPass(0);
					GL.Begin(1);
					GL.Color(LineColor);
					for (int j = 1; j < VertexData[i].Length; j++)
					{
						GL.Vertex(VertexData[i][j - 1]);
						GL.Vertex(VertexData[i][j]);
					}
					GL.End();
				}
			}
		}
	}
}
