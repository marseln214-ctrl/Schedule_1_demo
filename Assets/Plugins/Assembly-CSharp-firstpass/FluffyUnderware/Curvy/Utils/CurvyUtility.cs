using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public static class CurvyUtility
	{
		public static float ClampTF(float tf, CurvyClamping clamping)
		{
			return clamping switch
			{
				CurvyClamping.Loop => Mathf.Repeat(tf, 1f), 
				CurvyClamping.PingPong => Mathf.PingPong(tf, 1f), 
				CurvyClamping.Clamp => Mathf.Clamp01(tf), 
				_ => throw new InvalidEnumArgumentException(), 
			};
		}

		public static float ClampTF(float tf, ref int dir, CurvyClamping clamping)
		{
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return Mathf.Repeat(tf, 1f);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(tf) % 2 != 0)
				{
					dir *= -1;
				}
				return Mathf.PingPong(tf, 1f);
			case CurvyClamping.Clamp:
				return Mathf.Clamp01(tf);
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		public static float ClampValue(float tf, CurvyClamping clamping, float minTF, float maxTF)
		{
			switch (clamping)
			{
			case CurvyClamping.Loop:
			{
				float t2 = DTMath.MapValue(0f, 1f, tf, minTF, maxTF);
				return DTMath.MapValue(minTF, maxTF, Mathf.Repeat(t2, 1f), 0f);
			}
			case CurvyClamping.PingPong:
			{
				float t = DTMath.MapValue(0f, 1f, tf, minTF, maxTF);
				return DTMath.MapValue(minTF, maxTF, Mathf.PingPong(t, 1f), 0f);
			}
			case CurvyClamping.Clamp:
				return Mathf.Clamp(tf, minTF, maxTF);
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		public static float ClampDistance(float distance, CurvyClamping clamping, float length)
		{
			if (length == 0f)
			{
				return 0f;
			}
			return clamping switch
			{
				CurvyClamping.Loop => Mathf.Repeat(distance, length), 
				CurvyClamping.PingPong => Mathf.PingPong(distance, length), 
				CurvyClamping.Clamp => Mathf.Clamp(distance, 0f, length), 
				_ => throw new InvalidEnumArgumentException(), 
			};
		}

		public static float ClampDistance(float distance, CurvyClamping clamping, float length, float min, float max)
		{
			if (length == 0f)
			{
				return 0f;
			}
			min = Mathf.Clamp(min, 0f, length);
			max = Mathf.Clamp(max, min, length);
			return clamping switch
			{
				CurvyClamping.Loop => min + Mathf.Repeat(distance, max - min), 
				CurvyClamping.PingPong => min + Mathf.PingPong(distance, max - min), 
				CurvyClamping.Clamp => Mathf.Clamp(distance, min, max), 
				_ => throw new InvalidEnumArgumentException(), 
			};
		}

		public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length)
		{
			if (length == 0f)
			{
				return 0f;
			}
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return Mathf.Repeat(distance, length);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(distance / length) % 2 != 0)
				{
					dir *= -1;
				}
				return Mathf.PingPong(distance, length);
			case CurvyClamping.Clamp:
				return Mathf.Clamp(distance, 0f, length);
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length, float min, float max)
		{
			if (length == 0f)
			{
				return 0f;
			}
			min = Mathf.Clamp(min, 0f, length);
			max = Mathf.Clamp(max, min, length);
			switch (clamping)
			{
			case CurvyClamping.Loop:
				return min + Mathf.Repeat(distance, max - min);
			case CurvyClamping.PingPong:
				if (Mathf.FloorToInt(distance / (max - min)) % 2 != 0)
				{
					dir *= -1;
				}
				return min + Mathf.PingPong(distance, max - min);
			case CurvyClamping.Clamp:
				return Mathf.Clamp(distance, min, max);
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		public static Material GetDefaultMaterial()
		{
			Material material = Resources.Load("CurvyDefaultMaterial") as Material;
			if (material == null)
			{
				Shader shader = Shader.Find("Standard");
				if (shader != null)
				{
					material = new Material(shader);
				}
			}
			if (material == null)
			{
				DTLog.LogWarning("[Curvy] Couldn't find Curvy's default material. Please raise a bug report.");
			}
			return material;
		}

		public static bool Approximately(this float x, float y)
		{
			float num = Mathf.Epsilon * 8f;
			float num2 = Math.Abs(x);
			float num3 = Math.Abs(y);
			if (num3 < num)
			{
				return num2 < 9E-06f;
			}
			if (num2 < num)
			{
				return num3 < 9E-06f;
			}
			return Mathf.Approximately(x, y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int InterpolationSearch(float[] array, float x)
		{
			return InterpolationSearch(array, array.Length, x);
		}

		public static int InterpolationSearch(float[] array, int elementsCount, float x)
		{
			int num = 0;
			int i = elementsCount - 1;
			while (num <= i && array[num] <= x && x <= array[i])
			{
				if (num == i)
				{
					if (array[num] != x)
					{
						break;
					}
					return num;
				}
				int num2 = num + (int)((float)(i - num) / (array[i] - array[num]) * (x - array[num]));
				if (array[num2] == x)
				{
					return num2;
				}
				if (array[num2] < x)
				{
					num = num2 + 1;
				}
				else
				{
					i = num2 - 1;
				}
			}
			if (num > i)
			{
				int num3 = i;
				int num4 = num;
				num = num3;
				i = num4;
			}
			if (x <= array[num])
			{
				while (num >= 0)
				{
					if (array[num] <= x)
					{
						return num;
					}
					num--;
				}
				return 0;
			}
			if (array[i] < x)
			{
				for (; i < elementsCount; i++)
				{
					if (x < array[i])
					{
						return i - 1;
					}
				}
				return elementsCount - 1;
			}
			return -1;
		}

		public static Mesh SplineToMesh(this CurvySpline spline)
		{
			Spline2Mesh spline2Mesh = new Spline2Mesh();
			spline2Mesh.Lines.Add(new SplinePolyLine(spline));
			spline2Mesh.Apply(out var result);
			if (!string.IsNullOrEmpty(spline2Mesh.Error))
			{
				Debug.Log(spline2Mesh.Error);
			}
			return result;
		}

		public static void GetNearestPointIndex(Vector3 point, Vector3[] points, int pointsCount, out int index, out float fragement)
		{
			float num = float.MaxValue;
			int num2 = 0;
			Vector3 vector = default(Vector3);
			for (int i = 0; i < pointsCount; i++)
			{
				vector.x = points[i].x - point.x;
				vector.y = points[i].y - point.y;
				vector.z = points[i].z - point.z;
				float num3 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
				if (num3 <= num)
				{
					num = num3;
					num2 = i;
				}
			}
			int num4 = ((num2 > 0) ? (num2 - 1) : (-1));
			int num5 = ((num2 < pointsCount - 1) ? (num2 + 1) : (-1));
			float frag = 0f;
			float frag2 = 0f;
			float num6 = float.MaxValue;
			float num7 = float.MaxValue;
			if (num4 > -1)
			{
				num6 = DTMath.LinePointDistanceSqr(points[num4], points[num2], point, out frag);
			}
			if (num5 > -1)
			{
				num7 = DTMath.LinePointDistanceSqr(points[num2], points[num5], point, out frag2);
			}
			if (num6 < num7)
			{
				fragement = frag;
				index = num4;
			}
			else
			{
				fragement = frag2;
				index = num2;
			}
		}
	}
}
