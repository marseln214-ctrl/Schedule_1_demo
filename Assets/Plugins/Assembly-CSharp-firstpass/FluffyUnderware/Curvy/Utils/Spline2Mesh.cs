using System;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
using ToolBuddy.Pooling;
using ToolBuddy.Pooling.Collections;
using ToolBuddy.Pooling.Pools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public class Spline2Mesh
	{
		public List<SplinePolyLine> Lines = new List<SplinePolyLine>();

		public WindingRule Winding;

		public Vector2 UVTiling = Vector2.one;

		public Vector2 UVOffset = Vector2.zero;

		public bool SuppressUVMapping;

		public bool UV2;

		public string MeshName = string.Empty;

		public bool VertexLineOnly;

		private Tess mTess;

		private Mesh mMesh;

		public string Error { get; private set; }

		public bool Apply(out Mesh result)
		{
			ArrayPool<Vector3> pool = ArrayPoolsProvider.GetPool<Vector3>();
			mTess = null;
			mMesh = null;
			Error = string.Empty;
			bool flag = triangulate();
			if (flag)
			{
				mMesh = new Mesh();
				mMesh.name = MeshName;
				if (VertexLineOnly && Lines.Count > 0 && Lines[0] != null)
				{
					SubArray<Vector3> vertexList = Lines[0].GetVertexList();
					mMesh.SetVertices(vertexList.Array, 0, vertexList.Count);
					pool.Free(vertexList);
				}
				else
				{
					ContourVertex[] vertices = mTess.Vertices;
					SubArray<Vector3> subArray = pool.Allocate(vertices.Length);
					UnityLibTessUtility.FromContourVertex(vertices, subArray);
					mMesh.SetVertices(subArray.Array, 0, subArray.Count);
					mMesh.SetTriangles(mTess.ElementsArray.Value.Array, 0, mTess.ElementsArray.Value.Count, 0);
					pool.Free(subArray);
				}
				mMesh.RecalculateBounds();
				mMesh.RecalculateNormals();
				if (!SuppressUVMapping && !VertexLineOnly)
				{
					Vector3 size = mMesh.bounds.size;
					Vector3 min = mMesh.bounds.min;
					float num = Mathf.Min(size.x, Mathf.Min(size.y, size.z));
					bool flag2 = num == size.x;
					bool flag3 = num == size.y;
					bool flag4 = num == size.z;
					Vector3[] vertices2 = mMesh.vertices;
					int num2 = vertices2.Length;
					SubArray<Vector2> subArray2 = ArrayPools.Vector2.Allocate(num2);
					Vector2[] array = subArray2.Array;
					SubArray<Vector2> subArray3 = ArrayPools.Vector2.Allocate(UV2 ? num2 : 0);
					Vector2[] array2 = subArray3.Array;
					for (int i = 0; i < num2; i++)
					{
						Vector3 vector = vertices2[i];
						float num3;
						float num4;
						if (flag2)
						{
							num3 = (vector.y - min.y) / size.y;
							num4 = (vector.z - min.z) / size.z;
						}
						else if (flag3)
						{
							num3 = (vector.z - min.z) / size.z;
							num4 = (vector.x - min.x) / size.x;
						}
						else
						{
							if (!flag4)
							{
								throw new InvalidOperationException("Couldn't find the minimal bound dimension");
							}
							num3 = (vector.x - min.x) / size.x;
							num4 = (vector.y - min.y) / size.y;
						}
						if (UV2)
						{
							array2[i].x = num3;
							array2[i].y = num4;
						}
						num3 += UVOffset.x;
						num4 += UVOffset.y;
						num3 *= UVTiling.x;
						num4 *= UVTiling.y;
						array[i].x = num3;
						array[i].y = num4;
					}
					mMesh.SetUVs(0, subArray2.Array, 0, subArray2.Count);
					mMesh.SetUVs(1, subArray3.Array, 0, subArray3.Count);
					ArrayPools.Vector2.Free(subArray2);
					ArrayPools.Vector2.Free(subArray3);
					ArrayPools.Vector3.Free(vertices2);
				}
			}
			result = mMesh;
			return flag;
		}

		private bool triangulate()
		{
			if (Lines.Count == 0)
			{
				Error = "Missing splines to triangulate";
				return false;
			}
			if (VertexLineOnly)
			{
				return true;
			}
			mTess = new Tess();
			for (int i = 0; i < Lines.Count; i++)
			{
				if (Lines[i].Spline == null)
				{
					Error = "Missing Spline";
					return false;
				}
				if (!polyLineIsValid(Lines[i]))
				{
					Error = Lines[i].Spline.name + ": Angle must be >0";
					return false;
				}
				SubArray<Vector3> vertexList = Lines[i].GetVertexList();
				if (vertexList.Count < 3)
				{
					Error = Lines[i].Spline.name + ": At least 3 Vertices needed!";
					return false;
				}
				mTess.AddContour(UnityLibTessUtility.ToContourVertex(vertexList), Lines[i].Orientation);
				ArrayPoolsProvider.GetPool<Vector3>().Free(vertexList);
			}
			try
			{
				mTess.Tessellate(Winding, ElementType.Polygons, 3);
				return true;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}
			return false;
		}

		private static bool polyLineIsValid(SplinePolyLine pl)
		{
			if (pl == null || pl.VertexMode != 0)
			{
				return !Mathf.Approximately(0f, pl.Angle);
			}
			return true;
		}
	}
}
