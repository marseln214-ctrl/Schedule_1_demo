using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	[UsedImplicitly]
	[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
	public static class MeshFilterExt
	{
		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public static Mesh PrepareNewShared(this MeshFilter m, string name = "Mesh")
		{
			if (m == null)
			{
				return null;
			}
			if (m.sharedMesh == null)
			{
				Mesh mesh = new Mesh();
				mesh.MarkDynamic();
				mesh.name = name;
				m.sharedMesh = mesh;
			}
			else
			{
				m.sharedMesh.Clear();
				m.sharedMesh.name = name;
				m.sharedMesh.subMeshCount = 0;
			}
			return m.sharedMesh;
		}

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public static void CalculateTangents(this MeshFilter m)
		{
			int[] triangles = m.sharedMesh.triangles;
			Vector3[] vertices = m.sharedMesh.vertices;
			Vector2[] uv = m.sharedMesh.uv;
			Vector3[] normals = m.sharedMesh.normals;
			if (uv.Length != 0)
			{
				int num = triangles.Length;
				int num2 = vertices.Length;
				Vector3[] array = new Vector3[num2];
				Vector3[] array2 = new Vector3[num2];
				Vector4[] array3 = new Vector4[num2];
				for (int i = 0; i < num; i += 3)
				{
					int num3 = triangles[i];
					int num4 = triangles[i + 1];
					int num5 = triangles[i + 2];
					Vector3 vector = vertices[num3];
					Vector3 vector2 = vertices[num4];
					Vector3 vector3 = vertices[num5];
					Vector2 vector4 = uv[num3];
					Vector2 vector5 = uv[num4];
					Vector2 vector6 = uv[num5];
					float num6 = vector2.x - vector.x;
					float num7 = vector3.x - vector.x;
					float num8 = vector2.y - vector.y;
					float num9 = vector3.y - vector.y;
					float num10 = vector2.z - vector.z;
					float num11 = vector3.z - vector.z;
					float num12 = vector5.x - vector4.x;
					float num13 = vector6.x - vector4.x;
					float num14 = vector5.y - vector4.y;
					float num15 = vector6.y - vector4.y;
					float num16 = num12 * num15 - num13 * num14;
					float num17 = ((num16 == 0f) ? 0f : (1f / num16));
					float num18 = (num15 * num6 - num14 * num7) * num17;
					float num19 = (num15 * num8 - num14 * num9) * num17;
					float num20 = (num15 * num10 - num14 * num11) * num17;
					float num21 = (num12 * num7 - num13 * num6) * num17;
					float num22 = (num12 * num9 - num13 * num8) * num17;
					float num23 = (num12 * num11 - num13 * num10) * num17;
					array[num3].x += num18;
					array[num3].y += num19;
					array[num3].z += num20;
					array[num4].x += num18;
					array[num4].y += num19;
					array[num4].z += num20;
					array[num5].x += num18;
					array[num5].y += num19;
					array[num5].z += num20;
					array2[num3].x += num21;
					array2[num3].y += num22;
					array2[num3].z += num23;
					array2[num4].x += num21;
					array2[num4].y += num22;
					array2[num4].z += num23;
					array2[num5].x += num21;
					array2[num5].y += num22;
					array2[num5].z += num23;
				}
				for (int j = 0; j < num2; j++)
				{
					Vector3 normal = normals[j];
					Vector3 tangent = array[j];
					Vector3.OrthoNormalize(ref normal, ref tangent);
					array3[j].x = tangent.x;
					array3[j].y = tangent.y;
					array3[j].z = tangent.z;
					float num24 = (normal.y * tangent.z - normal.z * tangent.y) * array2[j].x + (normal.z * tangent.x - normal.x * tangent.z) * array2[j].y + (normal.x * tangent.y - normal.y * tangent.x) * array2[j].z;
					array3[j].w = ((num24 < 0f) ? (-1f) : 1f);
				}
				m.sharedMesh.tangents = array3;
			}
		}
	}
}
