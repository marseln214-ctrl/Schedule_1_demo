using System;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	public static class OptimizedOperators
	{
		public static Vector3 Addition(this Vector3 a, Vector3 b)
		{
			a.x += b.x;
			a.y += b.y;
			a.z += b.z;
			return a;
		}

		public static Vector3 UnaryNegation(this Vector3 a)
		{
			Vector3 result = default(Vector3);
			result.x = 0f - a.x;
			result.y = 0f - a.y;
			result.z = 0f - a.z;
			return result;
		}

		public static Vector3 Subtraction(this Vector3 a, Vector3 b)
		{
			a.x -= b.x;
			a.y -= b.y;
			a.z -= b.z;
			return a;
		}

		public static Vector3 Multiply(this Vector3 a, float d)
		{
			a.x *= d;
			a.y *= d;
			a.z *= d;
			return a;
		}

		public static Vector3 Multiply(this float d, Vector3 a)
		{
			a.x *= d;
			a.y *= d;
			a.z *= d;
			return a;
		}

		public static Vector3 Division(this Vector3 a, float d)
		{
			float num = 1f / d;
			a.x *= num;
			a.y *= num;
			a.z *= num;
			return a;
		}

		public static Vector3 Normalize(this Vector3 value)
		{
			float num = (float)Math.Sqrt((double)value.x * (double)value.x + (double)value.y * (double)value.y + (double)value.z * (double)value.z);
			Vector3 result = default(Vector3);
			if ((double)num > 9.99999974737875E-06)
			{
				float num2 = 1f / num;
				result.x = value.x * num2;
				result.y = value.y * num2;
				result.z = value.z * num2;
			}
			else
			{
				result.x = 0f;
				result.y = 0f;
				result.z = 0f;
			}
			return result;
		}

		public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, float t)
		{
			a.x += (b.x - a.x) * t;
			a.y += (b.y - a.y) * t;
			a.z += (b.z - a.z) * t;
			return a;
		}

		public static Color Multiply(this Color a, float b)
		{
			a.r *= b;
			a.g *= b;
			a.b *= b;
			a.a *= b;
			return a;
		}

		public static Color Multiply(this float b, Color a)
		{
			a.r *= b;
			a.g *= b;
			a.b *= b;
			a.a *= b;
			return a;
		}

		public static Quaternion Multiply(this Quaternion lhs, Quaternion rhs)
		{
			Quaternion result = default(Quaternion);
			result.x = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
			result.y = lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z;
			result.z = lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x;
			result.w = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;
			return result;
		}
	}
}
