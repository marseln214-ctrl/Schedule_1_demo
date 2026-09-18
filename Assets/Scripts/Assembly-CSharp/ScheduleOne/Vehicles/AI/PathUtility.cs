using ScheduleOne.Math;
using UnityEngine;

namespace ScheduleOne.Vehicles.AI
{
	public static class PathUtility
	{
		public static Vector3 GetAverageAheadPoint(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, int sampleCount, float stepSize)
		{
			GetClosestPointOnPath(path, referencePoint, out var startPointIndex, out var _, out var pointLerp);
			Vector3 zero = Vector3.zero;
			for (int i = 1; i <= sampleCount; i++)
			{
				zero += GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, (float)i * stepSize);
			}
			return zero / sampleCount;
		}

		public static Vector3 GetAheadPoint(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, float distance)
		{
			GetClosestPointOnPath(path, referencePoint, out var startPointIndex, out var _, out var pointLerp);
			return GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distance);
		}

		public static Vector3 GetAheadPoint(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, float distance, int startPointIndex, float pointLerp)
		{
			return GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distance);
		}

		public static Vector3 GetPointAheadOfPathPoint(PathSmoothingUtility.SmoothedPath path, int startPointIndex, float pointLerp, float distanceAhead)
		{
			if (path == null || path.vectorPath.Count < 2)
			{
				return Vector3.zero;
			}
			if (path.vectorPath.Count == startPointIndex + 1)
			{
				return path.vectorPath[startPointIndex];
			}
			float num = distanceAhead;
			Vector3 zero = Vector3.zero;
			int num2 = startPointIndex;
			while (num > 0f)
			{
				Vector3 vector = path.vectorPath[num2] + (path.vectorPath[num2 + 1] - path.vectorPath[num2]) * pointLerp;
				pointLerp = 0f;
				Vector3 vector2 = path.vectorPath[num2 + 1];
				if (Vector3.Distance(vector, vector2) > num)
				{
					return vector + (vector2 - vector).normalized * num;
				}
				num -= Vector3.Distance(vector, vector2);
				num2++;
				if (path.vectorPath.Count <= num2 + 1)
				{
					return vector2;
				}
			}
			return zero;
		}

		public static float CalculateAngleChangeOverPath(PathSmoothingUtility.SmoothedPath path, int startPointIndex, float pointLerp, float distanceAhead)
		{
			if (path.vectorPath.Count == startPointIndex + 1)
			{
				return 0f;
			}
			float num = distanceAhead;
			int num2 = startPointIndex;
			float num3 = 0f;
			while (num > 0f)
			{
				Vector3 vector = path.vectorPath[num2] + (path.vectorPath[num2 + 1] - path.vectorPath[num2]) * pointLerp;
				pointLerp = 0f;
				if (path.vectorPath.Count <= num2 + 2)
				{
					break;
				}
				Vector3 vector2 = path.vectorPath[num2 + 1];
				Vector3 vector3 = path.vectorPath[num2 + 2];
				if (Vector3.Distance(vector, vector2) > num)
				{
					break;
				}
				num -= Vector3.Distance(vector, vector2);
				num2++;
				num3 += Vector3.Angle((vector3 - vector2).normalized, (vector2 - vector).normalized);
				if (path.vectorPath.Count <= num2 + 2)
				{
					break;
				}
			}
			return num3;
		}

		public static float CalculateCTE(Vector3 flatCarPos, Transform vehicleTransform, Vector3 wp_from, Vector3 wp_to, PathSmoothingUtility.SmoothedPath path)
		{
			new Vector3(wp_from.x, flatCarPos.y, wp_from.z);
			new Vector3(wp_to.x, flatCarPos.y, wp_to.z);
			int startPointIndex;
			int endPointIndex;
			float pointLerp;
			Vector3 closestPointOnPath = GetClosestPointOnPath(path, flatCarPos, out startPointIndex, out endPointIndex, out pointLerp);
			Debug.DrawLine(flatCarPos, closestPointOnPath, Color.red);
			Vector3 vector = closestPointOnPath - flatCarPos;
			return 0f - vehicleTransform.InverseTransformVector(Vector3.Project(vector, vehicleTransform.right)).x;
		}

		public static Vector3 GetClosestPointOnPath(PathSmoothingUtility.SmoothedPath path, Vector3 point, out int startPointIndex, out int endPointIndex, out float pointLerp)
		{
			startPointIndex = 0;
			endPointIndex = 0;
			pointLerp = 0f;
			if (path == null || path.vectorPath == null || path.vectorPath.Count < 2)
			{
				return Vector3.zero;
			}
			float num = float.MaxValue;
			Vector3 result = Vector3.zero;
			for (int i = 0; i < path.vectorPath.Count - 1; i++)
			{
				Vector3 vector = path.vectorPath[i];
				Vector3 vector2 = path.vectorPath[i + 1];
				Vector3 closestPointOnLine = GetClosestPointOnLine(point, vector, vector2);
				float sqrMagnitude = (closestPointOnLine - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = closestPointOnLine;
					num = sqrMagnitude;
					startPointIndex = i;
					Vector3 vector3 = vector2 - vector;
					pointLerp = Vector3.Dot(closestPointOnLine - vector, vector3.normalized) / vector3.magnitude;
				}
			}
			endPointIndex = startPointIndex + 1;
			return result;
		}

		public static Vector3 GetAheadPointDirection(PathSmoothingUtility.SmoothedPath path, Vector3 referencePoint, float distanceAhead)
		{
			GetClosestPointOnPath(path, referencePoint, out var startPointIndex, out var _, out var pointLerp);
			Vector3 pointAheadOfPathPoint = GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distanceAhead);
			return (GetPointAheadOfPathPoint(path, startPointIndex, pointLerp, distanceAhead + 0.01f) - pointAheadOfPathPoint).normalized;
		}

		private static Vector3 GetClosestPointOnLine(Vector3 point, Vector3 line_start, Vector3 line_end, bool clamp = true)
		{
			Vector3 vector = line_end - line_start;
			float sqrMagnitude = vector.sqrMagnitude;
			if (sqrMagnitude < Mathf.Epsilon)
			{
				return line_start;
			}
			float num = Vector3.Dot(point - line_start, vector) / sqrMagnitude;
			if (clamp)
			{
				num = Mathf.Clamp01(num);
			}
			return line_start + num * vector;
		}
	}
}
