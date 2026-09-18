using System;
using FluffyUnderware.Curvy.Pools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
	public class CGVSubMesh : CGData
	{
		public Material Material;

		private SubArray<int> triangles;

		public SubArray<int> TrianglesList
		{
			get
			{
				return triangles;
			}
			set
			{
				ArrayPools.Int32.Free(triangles);
				triangles = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use TrianglesList instead")]
		public int[] Triangles
		{
			get
			{
				return TrianglesList.CopyToArray(ArrayPools.Int32);
			}
			set
			{
				TrianglesList = new SubArray<int>(value);
			}
		}

		public override int Count => triangles.Count;

		public CGVSubMesh(Material material = null)
		{
			Material = material;
			triangles = ArrayPools.Int32.Allocate(0);
		}

		public CGVSubMesh(int[] triangles, Material material = null)
		{
			Material = material;
			this.triangles = new SubArray<int>(triangles);
		}

		public CGVSubMesh(SubArray<int> triangles, Material material = null)
		{
			Material = material;
			this.triangles = triangles;
		}

		public CGVSubMesh(int triangleCount, Material material = null)
		{
			Material = material;
			triangles = ArrayPools.Int32.Allocate(triangleCount);
		}

		public CGVSubMesh(CGVSubMesh source)
		{
			Material = source.Material;
			triangles = ArrayPools.Int32.Clone(source.triangles);
		}

		protected override bool Dispose(bool disposing)
		{
			bool num = base.Dispose(disposing);
			if (num)
			{
				ArrayPools.Int32.Free(triangles);
			}
			return num;
		}

		public override T Clone<T>()
		{
			return new CGVSubMesh(this) as T;
		}

		public static CGVSubMesh Get(CGVSubMesh data, int triangleCount, Material material = null)
		{
			if (data == null)
			{
				return new CGVSubMesh(triangleCount, material);
			}
			ArrayPools.Int32.Resize(ref data.triangles, triangleCount);
			data.Material = material;
			return data;
		}

		public void ShiftIndices(int offset, int startIndex = 0)
		{
			for (int i = startIndex; i < triangles.Count; i++)
			{
				triangles.Array[i] += offset;
			}
		}

		public void Add(CGVSubMesh other, int shiftIndexOffset = 0)
		{
			int count = triangles.Count;
			int count2 = other.triangles.Count;
			if (count2 != 0)
			{
				ArrayPools.Int32.Resize(ref triangles, count + count2);
				Array.Copy(other.triangles.Array, 0, triangles.Array, count, count2);
				if (shiftIndexOffset != 0)
				{
					ShiftIndices(shiftIndexOffset, count);
				}
			}
		}
	}
}
