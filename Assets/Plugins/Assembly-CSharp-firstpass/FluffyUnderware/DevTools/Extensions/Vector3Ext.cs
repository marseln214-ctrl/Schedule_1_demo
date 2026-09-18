using UnityEngine;

namespace FluffyUnderware.DevTools.Extensions
{
	public static class Vector3Ext
	{
		public static float AngleSigned(this Vector3 a, Vector3 b, Vector3 normal)
		{
			return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(a, b)), Vector3.Dot(a, b)) * 57.29578f;
		}

		public static Vector3 RotateAround(this Vector3 point, Vector3 origin, Quaternion rotation)
		{
			Vector3 vector = point - origin;
			vector = rotation * vector;
			return origin + vector;
		}

		public static Vector2 ToVector2(this Vector3 v)
		{
			Vector2 result = default(Vector2);
			result.x = v.x;
			result.y = v.y;
			return result;
		}

		public static bool Approximately(this Vector3 v1, Vector3 v2)
		{
			Vector3 vector = v1;
			vector.x -= v2.x;
			vector.y -= v2.y;
			vector.z -= v2.z;
			if (Vector3.SqrMagnitude(vector) < 1E-06f)
			{
				return true;
			}
			if (Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z))
			{
				return true;
			}
			return false;
		}

		public static bool NotApproximately(this Vector3 v1, Vector3 v2)
		{
			return !v1.Approximately(v2);
		}
	}
}
