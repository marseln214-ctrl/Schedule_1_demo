using System;
using ToolBuddy.Pooling;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.ThirdParty.LibTessDotNet
{
	public static class UnityLibTessUtility
	{
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public static ContourVertex[] ToContourVertex(Vector3[] v, bool zeroZ = false)
		{
			ContourVertex[] array = new ContourVertex[v.Length];
			for (int i = 0; i < v.Length; i++)
			{
				array[i].Position.X = v[i].x;
				array[i].Position.Y = v[i].y;
				array[i].Position.Z = (zeroZ ? 0f : v[i].z);
			}
			return array;
		}

		public static ContourVertex[] ToContourVertex(SubArray<Vector3> v, bool zeroZ = false)
		{
			int count = v.Count;
			Vector3[] array = v.Array;
			ContourVertex[] array2 = new ContourVertex[count];
			for (int i = 0; i < count; i++)
			{
				array2[i].Position.X = array[i].x;
				array2[i].Position.Y = array[i].y;
				array2[i].Position.Z = (zeroZ ? 0f : array[i].z);
			}
			return array2;
		}

		public static void FromContourVertex(ContourVertex[] v, SubArray<Vector3> output)
		{
			int count = output.Count;
			Vector3[] array = output.Array;
			for (int i = 0; i < count; i++)
			{
				array[i].x = v[i].Position.X;
				array[i].y = v[i].Position.Y;
				array[i].z = v[i].Position.Z;
			}
		}

		public static SubArray<Vector3> ContourVerticesToPositions(ContourVertex[] v)
		{
			SubArray<Vector3> result = ArrayPoolsProvider.GetPool<Vector3>().Allocate(v.Length);
			Vector3[] array = result.Array;
			for (int i = 0; i < result.Count; i++)
			{
				array[i].x = v[i].Position.X;
				array[i].y = v[i].Position.Y;
				array[i].z = v[i].Position.Z;
			}
			return result;
		}

		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public static void SetFromContourVertex(ref Vector3[] v3Array, ref ContourVertex[] cvArray)
		{
			Array.Resize(ref v3Array, cvArray.Length);
			for (int i = 0; i < v3Array.Length; i++)
			{
				v3Array[i].x = cvArray[i].Position.X;
				v3Array[i].y = cvArray[i].Position.Y;
				v3Array[i].z = cvArray[i].Position.Z;
			}
		}

		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public static void SetToContourVertex(ref ContourVertex[] cvArray, ref Vector3[] v3Array)
		{
			Array.Resize(ref cvArray, v3Array.Length);
			for (int i = 0; i < cvArray.Length; i++)
			{
				cvArray[i].Position.X = v3Array[i].x;
				cvArray[i].Position.Y = v3Array[i].y;
				cvArray[i].Position.Z = v3Array[i].z;
			}
		}
	}
}
