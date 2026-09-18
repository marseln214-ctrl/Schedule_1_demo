using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluffyUnderware.Curvy.Generator
{
	[CGDataInfo(0.98f, 0.5f, 0f, 1f)]
	public class CGVMesh : CGBounds
	{
		public CGVSubMesh[] SubMeshes;

		private SubArray<int>? sortedVertexIndices;

		private readonly object vertexIndicesLock = new object();

		private SubArray<Vector3> vertices;

		private SubArray<Vector2> uvs;

		private SubArray<Vector2> uv2s;

		private SubArray<Vector3> normals;

		private SubArray<Vector4> tangents;

		private bool hasPartialNormals;

		private bool hasPartialTangents;

		public SubArray<Vector3> Vertices
		{
			get
			{
				return vertices;
			}
			set
			{
				ArrayPools.Vector3.Free(vertices);
				vertices = value;
				OnVerticesChanged();
			}
		}

		public SubArray<Vector2> UVs
		{
			get
			{
				return uvs;
			}
			set
			{
				ArrayPools.Vector2.Free(uvs);
				uvs = value;
			}
		}

		public SubArray<Vector2> UV2s
		{
			get
			{
				return uv2s;
			}
			set
			{
				ArrayPools.Vector2.Free(uv2s);
				uv2s = value;
			}
		}

		public SubArray<Vector3> NormalsList
		{
			get
			{
				return normals;
			}
			set
			{
				ArrayPools.Vector3.Free(normals);
				normals = value;
			}
		}

		public SubArray<Vector4> TangentsList
		{
			get
			{
				return tangents;
			}
			set
			{
				ArrayPools.Vector4.Free(tangents);
				tangents = value;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use Vertices instead")]
		public Vector3[] Vertex
		{
			get
			{
				return Vertices.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				Vertices = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use UVs instead")]
		public Vector2[] UV
		{
			get
			{
				return UVs.CopyToArray(ArrayPools.Vector2);
			}
			set
			{
				UVs = new SubArray<Vector2>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use UV2s instead")]
		public Vector2[] UV2
		{
			get
			{
				return UV2s.CopyToArray(ArrayPools.Vector2);
			}
			set
			{
				UV2s = new SubArray<Vector2>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use NormalList instead")]
		public Vector3[] Normals
		{
			get
			{
				return NormalsList.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				NormalsList = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use TangentsList instead")]
		public Vector4[] Tangents
		{
			get
			{
				return TangentsList.CopyToArray(ArrayPools.Vector4);
			}
			set
			{
				TangentsList = new SubArray<Vector4>(value);
			}
		}

		public override int Count => vertices.Count;

		public bool HasUV => uvs.Count > 0;

		public bool HasUV2 => uv2s.Count > 0;

		public bool HasNormals => normals.Count > 0;

		public bool HasPartialNormals
		{
			get
			{
				return hasPartialNormals;
			}
			private set
			{
				hasPartialNormals = value;
			}
		}

		public bool HasTangents => tangents.Count > 0;

		public bool HasPartialTangents
		{
			get
			{
				return hasPartialTangents;
			}
			private set
			{
				hasPartialTangents = value;
			}
		}

		public int TriangleCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < SubMeshes.Length; i++)
				{
					num += SubMeshes[i].TrianglesList.Count;
				}
				return num / 3;
			}
		}

		public CGVMesh()
			: this(0)
		{
		}

		public CGVMesh(int vertexCount, bool addUV = false, bool addUV2 = false, bool addNormals = false, bool addTangents = false)
		{
			vertices = ArrayPools.Vector3.Allocate(vertexCount);
			uvs = (addUV ? ArrayPools.Vector2.Allocate(vertexCount) : ArrayPools.Vector2.Allocate(0));
			uv2s = (addUV2 ? ArrayPools.Vector2.Allocate(vertexCount) : ArrayPools.Vector2.Allocate(0));
			normals = (addNormals ? ArrayPools.Vector3.Allocate(vertexCount) : ArrayPools.Vector3.Allocate(0));
			tangents = (addTangents ? ArrayPools.Vector4.Allocate(vertexCount) : ArrayPools.Vector4.Allocate(0));
			hasPartialNormals = false;
			hasPartialTangents = false;
			SubMeshes = new CGVSubMesh[0];
		}

		public CGVMesh(CGVolume volume)
			: this(volume.Vertices.Count)
		{
			Array.Copy(volume.Vertices.Array, 0, vertices.Array, 0, volume.Vertices.Count);
		}

		public CGVMesh(CGVolume volume, IntRegion subset)
			: this((subset.LengthPositive + 1) * volume.CrossSize, addUV: false, addUV2: false, addNormals: true)
		{
			int sourceIndex = subset.Low * volume.CrossSize;
			Array.Copy(volume.Vertices.Array, sourceIndex, vertices.Array, 0, vertices.Count);
			Array.Copy(volume.VertexNormals.Array, sourceIndex, normals.Array, 0, normals.Count);
		}

		public CGVMesh(CGVMesh source)
			: base(source)
		{
			vertices = ArrayPools.Vector3.Clone(source.vertices);
			uvs = ArrayPools.Vector2.Clone(source.uvs);
			uv2s = ArrayPools.Vector2.Clone(source.uv2s);
			normals = ArrayPools.Vector3.Clone(source.normals);
			tangents = ArrayPools.Vector4.Clone(source.tangents);
			hasPartialNormals = source.HasPartialNormals;
			hasPartialTangents = source.HasPartialTangents;
			SubMeshes = new CGVSubMesh[source.SubMeshes.Length];
			for (int i = 0; i < source.SubMeshes.Length; i++)
			{
				SubMeshes[i] = new CGVSubMesh(source.SubMeshes[i]);
			}
		}

		public CGVMesh([NotNull] CGMeshProperties meshProperties)
			: this(meshProperties.Mesh, meshProperties.Material, meshProperties.Matrix)
		{
		}

		public CGVMesh([NotNull] Mesh source, Material[] materials, Matrix4x4 trsMatrix)
		{
			Name = source.name;
			vertices = new SubArray<Vector3>(source.vertices);
			normals = new SubArray<Vector3>(source.normals);
			tangents = new SubArray<Vector4>(source.tangents);
			hasPartialNormals = false;
			hasPartialTangents = false;
			uvs = new SubArray<Vector2>(source.uv);
			uv2s = new SubArray<Vector2>(source.uv2);
			SubMeshes = new CGVSubMesh[source.subMeshCount];
			for (int i = 0; i < source.subMeshCount; i++)
			{
				SubMeshes[i] = new CGVSubMesh(source.GetTriangles(i), (materials.Length > i) ? materials[i] : null);
			}
			base.Bounds = source.bounds;
			if (!trsMatrix.isIdentity)
			{
				TRS(trsMatrix);
			}
		}

		protected override bool Dispose(bool disposing)
		{
			bool flag = base.Dispose(disposing);
			if (flag)
			{
				if (sortedVertexIndices.HasValue)
				{
					ArrayPools.Int32.Free(sortedVertexIndices.Value);
				}
				ArrayPools.Vector3.Free(vertices);
				ArrayPools.Vector2.Free(uvs);
				ArrayPools.Vector2.Free(uv2s);
				ArrayPools.Vector3.Free(normals);
				ArrayPools.Vector4.Free(tangents);
				if (disposing)
				{
					for (int i = 0; i < SubMeshes.Length; i++)
					{
						SubMeshes[i].Dispose();
					}
				}
			}
			return flag;
		}

		public override T Clone<T>()
		{
			return new CGVMesh(this) as T;
		}

		[UsedImplicitly]
		[Obsolete("Member not used by Curvy, will get removed next major version. Use another overload of this method")]
		public static CGVMesh Get(CGVMesh data, CGVolume source, bool addUV, bool reverseNormals)
		{
			return Get(data, source, new IntRegion(0, source.Count - 1), addUV, reverseNormals);
		}

		[UsedImplicitly]
		[Obsolete("Member not used by Curvy, will get removed next major version. Use another overload of this method")]
		public static CGVMesh Get(CGVMesh data, CGVolume source, IntRegion subset, bool addUV, bool reverseNormals)
		{
			return Get(data, source, subset, addUV, addUV2: false, reverseNormals);
		}

		[NotNull]
		public static CGVMesh Get([CanBeNull] CGVMesh data, CGVolume source, IntRegion subset, bool addUV, bool addUV2, bool reverseNormals)
		{
			int sourceIndex = subset.Low * source.CrossSize;
			int num = (subset.LengthPositive + 1) * source.CrossSize;
			if (data == null)
			{
				data = new CGVMesh(num, addUV, addUV2, addNormals: true);
			}
			else
			{
				if (data.vertices.Count != num)
				{
					ArrayPools.Vector3.Resize(ref data.vertices, num, clearNewSpace: false);
				}
				if (data.normals.Count != num)
				{
					ArrayPools.Vector3.Resize(ref data.normals, num, clearNewSpace: false);
				}
				int num2 = (addUV ? num : 0);
				if (data.uvs.Count != num2)
				{
					ArrayPools.Vector2.ResizeAndClear(ref data.uvs, num2);
				}
				int num3 = (addUV2 ? num : 0);
				if (data.uv2s.Count != num3)
				{
					ArrayPools.Vector2.ResizeAndClear(ref data.uv2s, num3);
				}
				if (data.tangents.Count != 0)
				{
					ArrayPools.Vector4.Resize(ref data.tangents, 0);
				}
				data.HasPartialTangents = false;
			}
			Array.Copy(source.Vertices.Array, sourceIndex, data.vertices.Array, 0, num);
			Array.Copy(source.VertexNormals.Array, sourceIndex, data.normals.Array, 0, num);
			data.hasPartialNormals = false;
			if (reverseNormals)
			{
				Vector3[] array = data.normals.Array;
				for (int i = 0; i < data.normals.Count; i++)
				{
					array[i].x = 0f - array[i].x;
					array[i].y = 0f - array[i].y;
					array[i].z = 0f - array[i].z;
				}
			}
			data.OnVerticesChanged();
			return data;
		}

		public void SetSubMeshCount(int count)
		{
			Array.Resize(ref SubMeshes, count);
		}

		public void AddSubMesh(CGVSubMesh submesh = null)
		{
			SubMeshes = SubMeshes.Add(submesh);
		}

		public void MergeVMesh(CGVMesh source)
		{
			MergeVMesh(source, Matrix4x4.identity);
		}

		public void MergeVMesh(CGVMesh source, Matrix4x4 matrix)
		{
			int count = Count;
			if (source.Count == 0)
			{
				return;
			}
			int num = count + source.Count;
			ArrayPools.Vector3.Resize(ref vertices, num);
			if (matrix == Matrix4x4.identity)
			{
				Array.Copy(source.vertices.Array, 0, vertices.Array, count, source.Count);
			}
			else
			{
				for (int i = count; i < num; i++)
				{
					vertices.Array[i] = matrix.MultiplyPoint3x4(source.vertices.Array[i - count]);
				}
			}
			MergeUVsNormalsAndTangents(source, count);
			for (int j = 0; j < source.SubMeshes.Length; j++)
			{
				GetMaterialSubMesh(source.SubMeshes[j].Material).Add(source.SubMeshes[j], count);
			}
			OnVerticesChanged();
		}

		public void MergeVMeshes(List<CGVMesh> vMeshes, int startIndex, int endIndex)
		{
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			Dictionary<Material, List<SubArray<int>>> dictionary = new Dictionary<Material, List<SubArray<int>>>();
			Dictionary<Material, int> dictionary2 = new Dictionary<Material, int>();
			List<SubArray<int>> list = null;
			int num2 = 0;
			for (int i = startIndex; i <= endIndex; i++)
			{
				CGVMesh cGVMesh = vMeshes[i];
				num += cGVMesh.Count;
				flag |= cGVMesh.HasNormals;
				flag2 |= !cGVMesh.HasNormals || cGVMesh.HasPartialNormals;
				flag3 |= cGVMesh.HasTangents;
				flag4 |= !cGVMesh.HasTangents || cGVMesh.hasPartialTangents;
				flag5 |= cGVMesh.HasUV;
				flag6 |= cGVMesh.HasUV2;
				for (int j = 0; j < cGVMesh.SubMeshes.Length; j++)
				{
					CGVSubMesh cGVSubMesh = cGVMesh.SubMeshes[j];
					if (cGVSubMesh.Material != null)
					{
						Material material2 = cGVSubMesh.Material;
						if (!dictionary.ContainsKey(material2))
						{
							dictionary[material2] = new List<SubArray<int>>(1);
							dictionary2[material2] = 0;
						}
						dictionary[material2].Add(cGVSubMesh.TrianglesList);
					}
					else
					{
						if (list == null)
						{
							list = new List<SubArray<int>>(1);
							num2 = 0;
						}
						list.Add(cGVSubMesh.TrianglesList);
					}
				}
			}
			ArrayPools.Vector3.Resize(ref vertices, num);
			if (flag)
			{
				ArrayPools.Vector3.Resize(ref normals, num);
			}
			hasPartialNormals = flag2;
			if (flag3)
			{
				ArrayPools.Vector4.Resize(ref tangents, num);
			}
			hasPartialTangents = flag4;
			if (flag5)
			{
				ArrayPools.Vector2.Resize(ref uvs, num);
			}
			if (flag6)
			{
				ArrayPools.Vector2.Resize(ref uv2s, num);
			}
			foreach (KeyValuePair<Material, List<SubArray<int>>> item in dictionary)
			{
				ProcessTriangleArrays(item.Value, item.Key);
			}
			if (list != null)
			{
				ProcessTriangleArrays(list, null);
			}
			int num3 = 0;
			for (int k = startIndex; k <= endIndex; k++)
			{
				CGVMesh cGVMesh2 = vMeshes[k];
				Array.Copy(cGVMesh2.vertices.Array, 0, vertices.Array, num3, cGVMesh2.vertices.Count);
				if (flag)
				{
					if (cGVMesh2.HasNormals)
					{
						Array.Copy(cGVMesh2.normals.Array, 0, normals.Array, num3, cGVMesh2.normals.Count);
					}
					else
					{
						Array.Clear(normals.Array, num3, cGVMesh2.vertices.Count);
					}
				}
				if (flag3)
				{
					if (cGVMesh2.HasTangents)
					{
						Array.Copy(cGVMesh2.tangents.Array, 0, tangents.Array, num3, cGVMesh2.tangents.Count);
					}
					else
					{
						Array.Clear(tangents.Array, num3, cGVMesh2.vertices.Count);
					}
				}
				if (flag5)
				{
					if (cGVMesh2.HasUV)
					{
						Array.Copy(cGVMesh2.uvs.Array, 0, uvs.Array, num3, cGVMesh2.uvs.Count);
					}
					else
					{
						Array.Clear(uvs.Array, num3, cGVMesh2.vertices.Count);
					}
				}
				if (flag6)
				{
					if (cGVMesh2.HasUV2)
					{
						Array.Copy(cGVMesh2.uv2s.Array, 0, uv2s.Array, num3, cGVMesh2.uv2s.Count);
					}
					else
					{
						Array.Clear(uv2s.Array, num3, cGVMesh2.vertices.Count);
					}
				}
				for (int l = 0; l < cGVMesh2.SubMeshes.Length; l++)
				{
					CGVSubMesh obj = cGVMesh2.SubMeshes[l];
					Material material3 = obj.Material;
					SubArray<int> trianglesList = obj.TrianglesList;
					int count = trianglesList.Count;
					SubArray<int> trianglesList2 = GetMaterialSubMesh(material3).TrianglesList;
					int num4 = ((material3 == null) ? num2 : dictionary2[material3]);
					if (count == 0)
					{
						continue;
					}
					if (num3 == 0)
					{
						Array.Copy(trianglesList.Array, 0, trianglesList2.Array, num4, count);
					}
					else
					{
						for (int m = 0; m < count; m++)
						{
							trianglesList2.Array[num4 + m] = trianglesList.Array[m] + num3;
						}
					}
					int num5 = num4 + count;
					if (material3 == null)
					{
						num2 = num5;
					}
					else
					{
						dictionary2[material3] = num5;
					}
				}
				num3 += cGVMesh2.vertices.Count;
			}
			OnVerticesChanged();
			void ProcessTriangleArrays(List<SubArray<int>> subArrays, Material material1)
			{
				int num6 = 0;
				for (int n = 0; n < subArrays.Count; n++)
				{
					num6 += subArrays[n].Count;
				}
				AddSubMesh(new CGVSubMesh(num6, material1));
			}
		}

		private void MergeUVsNormalsAndTangents(CGVMesh source, int preMergeVertexCount)
		{
			int count = source.Count;
			if (count == 0)
			{
				return;
			}
			int minimalSize = preMergeVertexCount + count;
			if (HasUV || source.HasUV)
			{
				SubArray<Vector2> uVs = ArrayPools.Vector2.Allocate(minimalSize, clearArray: false);
				if (HasUV)
				{
					Array.Copy(uvs.Array, 0, uVs.Array, 0, preMergeVertexCount);
				}
				else
				{
					Array.Clear(uVs.Array, 0, preMergeVertexCount);
				}
				if (source.HasUV)
				{
					Array.Copy(source.uvs.Array, 0, uVs.Array, preMergeVertexCount, count);
				}
				else
				{
					Array.Clear(uVs.Array, preMergeVertexCount, count);
				}
				UVs = uVs;
			}
			if (HasUV2 || source.HasUV2)
			{
				SubArray<Vector2> uV2s = ArrayPools.Vector2.Allocate(minimalSize, clearArray: false);
				if (HasUV2)
				{
					Array.Copy(uv2s.Array, 0, uV2s.Array, 0, preMergeVertexCount);
				}
				else
				{
					Array.Clear(uV2s.Array, 0, preMergeVertexCount);
				}
				if (source.HasUV2)
				{
					Array.Copy(source.uv2s.Array, 0, uV2s.Array, preMergeVertexCount, count);
				}
				else
				{
					Array.Clear(uV2s.Array, preMergeVertexCount, count);
				}
				UV2s = uV2s;
			}
			if (HasNormals || source.HasNormals)
			{
				HasPartialNormals = HasNormals ^ source.HasNormals;
				SubArray<Vector3> normalsList = ArrayPools.Vector3.Allocate(minimalSize, clearArray: false);
				if (HasNormals)
				{
					Array.Copy(normals.Array, 0, normalsList.Array, 0, preMergeVertexCount);
				}
				else
				{
					Array.Clear(normalsList.Array, 0, preMergeVertexCount);
				}
				if (source.HasNormals)
				{
					Array.Copy(source.normals.Array, 0, normalsList.Array, preMergeVertexCount, count);
				}
				else
				{
					Array.Clear(normalsList.Array, preMergeVertexCount, count);
				}
				NormalsList = normalsList;
			}
			if (HasTangents || source.HasTangents)
			{
				HasPartialTangents = HasTangents ^ source.HasTangents;
				SubArray<Vector4> tangentsList = ArrayPools.Vector4.Allocate(minimalSize, clearArray: false);
				if (HasTangents)
				{
					Array.Copy(tangents.Array, 0, tangentsList.Array, 0, preMergeVertexCount);
				}
				else
				{
					Array.Clear(tangentsList.Array, 0, preMergeVertexCount);
				}
				if (source.HasTangents)
				{
					Array.Copy(source.tangents.Array, 0, tangentsList.Array, preMergeVertexCount, count);
				}
				else
				{
					Array.Clear(tangentsList.Array, preMergeVertexCount, count);
				}
				TangentsList = tangentsList;
			}
		}

		public CGVSubMesh GetMaterialSubMesh(Material mat, bool createIfMissing = true)
		{
			for (int i = 0; i < SubMeshes.Length; i++)
			{
				if (SubMeshes[i].Material == mat)
				{
					return SubMeshes[i];
				}
			}
			if (createIfMissing)
			{
				CGVSubMesh cGVSubMesh = new CGVSubMesh(mat);
				AddSubMesh(cGVSubMesh);
				return cGVSubMesh;
			}
			return null;
		}

		[UsedImplicitly]
		[Obsolete("Use ToMesh instead")]
		public Mesh AsMesh()
		{
			Mesh mesh = new Mesh();
			ToMesh(ref mesh);
			return mesh;
		}

		public void ToMesh(ref Mesh mesh, bool includeNormals = true, bool includeTangents = true)
		{
			mesh.indexFormat = ((Count >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			mesh.SetVertices(vertices.Array, 0, vertices.Count);
			mesh.SetUVs(0, uvs.Array, 0, HasUV ? uvs.Count : 0);
			mesh.SetUVs(1, uv2s.Array, 0, HasUV2 ? uv2s.Count : 0);
			mesh.SetNormals(normals.Array, 0, (includeNormals && HasNormals) ? normals.Count : 0);
			mesh.SetTangents(tangents.Array, 0, (includeTangents && HasTangents) ? tangents.Count : 0);
			mesh.subMeshCount = SubMeshes.Length;
			for (int i = 0; i < SubMeshes.Length; i++)
			{
				SubArray<int> trianglesList = SubMeshes[i].TrianglesList;
				mesh.SetTriangles(trianglesList.Array, 0, trianglesList.Count, i);
			}
		}

		public Material[] GetMaterials()
		{
			List<Material> list = new List<Material>();
			for (int i = 0; i < SubMeshes.Length; i++)
			{
				list.Add(SubMeshes[i].Material);
			}
			return list.ToArray();
		}

		public override void RecalculateBounds()
		{
			if (Count == 0)
			{
				mBounds = new Bounds(Vector3.zero, Vector3.zero);
				return;
			}
			int count = vertices.Count;
			Vector3 min = vertices.Array[0];
			Vector3 max = vertices.Array[0];
			for (int i = 1; i < count; i++)
			{
				Vector3 vector = vertices.Array[i];
				if (vector.x < min.x)
				{
					min.x = vector.x;
				}
				else if (vector.x > max.x)
				{
					max.x = vector.x;
				}
				if (vector.y < min.y)
				{
					min.y = vector.y;
				}
				else if (vector.y > max.y)
				{
					max.y = vector.y;
				}
				if (vector.z < min.z)
				{
					min.z = vector.z;
				}
				else if (vector.z > max.z)
				{
					max.z = vector.z;
				}
			}
			Bounds value = default(Bounds);
			value.SetMinMax(min, max);
			mBounds = value;
		}

		[UsedImplicitly]
		[Obsolete("Method will get remove in next major update. Copy its content if you need it")]
		public void RecalculateUV2()
		{
			ArrayPools.Vector2.Resize(ref uv2s, UVs.Count);
			CGUtility.CalculateUV2(uvs.Array, uv2s.Array, uvs.Count);
		}

		public void TRS(Matrix4x4 matrix)
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				vertices.Array[i] = matrix.MultiplyPoint3x4(vertices.Array[i]);
			}
			count = normals.Count;
			for (int j = 0; j < count; j++)
			{
				normals.Array[j] = matrix.MultiplyVector(normals.Array[j]);
			}
			count = tangents.Count;
			Vector3 vector2 = default(Vector3);
			for (int k = 0; k < count; k++)
			{
				Vector4 vector = tangents.Array[k];
				vector2.x = vector.x;
				vector2.y = vector.y;
				vector2.z = vector.z;
				tangents.Array[k] = matrix.MultiplyVector(vector2);
			}
			OnVerticesChanged();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void OnVerticesChanged()
		{
			mBounds = null;
			ClearCachedSortedVertexIndices();
		}

		public SubArray<int> GetCachedSortedVertexIndices()
		{
			if (!sortedVertexIndices.HasValue)
			{
				lock (vertexIndicesLock)
				{
					if (!sortedVertexIndices.HasValue)
					{
						int count = vertices.Count;
						SubArray<int> value = ArrayPools.Int32.Allocate(count);
						SubArray<float> subArray = ArrayPools.Single.Allocate(count);
						for (int i = 0; i < count; i++)
						{
							value.Array[i] = i;
							subArray.Array[i] = vertices.Array[i].z;
						}
						Array.Sort(subArray.Array, value.Array, 0, count);
						ArrayPools.Single.Free(subArray);
						sortedVertexIndices = value;
					}
				}
			}
			return sortedVertexIndices.Value;
		}

		private void ClearCachedSortedVertexIndices()
		{
			if (!sortedVertexIndices.HasValue)
			{
				return;
			}
			lock (vertexIndicesLock)
			{
				if (sortedVertexIndices.HasValue)
				{
					ArrayPools.Int32.Free(sortedVertexIndices.Value);
					sortedVertexIndices = null;
				}
			}
		}
	}
}
