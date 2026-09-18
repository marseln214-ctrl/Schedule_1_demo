using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy;
using UnityEngine;

namespace ScheduleOne.Math
{
	public class PathSmoothingUtility : MonoBehaviour
	{
		public class SmoothedPath
		{
			public List<Vector3> vectorPath = new List<Vector3>();
		}

		public const float MinControlPointDistance = 0.5f;

		private static CurvySpline spline;

		private void Awake()
		{
			spline = CurvySpline.Create();
			spline.transform.SetParent(base.transform);
			spline.Interpolation = CurvyInterpolation.BSpline;
			spline.BSplineDegree = 5;
			spline.Orientation = CurvyOrientation.None;
			spline.CacheDensity = 30;
		}

		public static SmoothedPath CalculateSmoothedPath(List<Vector3> controlPoints, float maxCPDistance = 5f)
		{
			if (controlPoints.Count < 2)
			{
				Debug.LogWarning("Smoothing requires at least 2 control points.");
				return new SmoothedPath
				{
					vectorPath = controlPoints
				};
			}
			for (int i = 1; i < controlPoints.Count; i++)
			{
				if (Vector3.Distance(controlPoints[i], controlPoints[i - 1]) < 0.5f)
				{
					controlPoints.RemoveAt(i);
					i--;
				}
			}
			if (controlPoints.Count < 2)
			{
				Debug.LogWarning("Smoothing requires at least 2 control points.");
				return new SmoothedPath
				{
					vectorPath = controlPoints
				};
			}
			SmoothedPath smoothedPath = new SmoothedPath();
			controlPoints = InsertIntermediatePoints(controlPoints, maxCPDistance);
			spline.Clear(isUndoable: false);
			spline.Add(controlPoints.ToArray(), Space.World);
			spline.Refresh();
			List<Vector3> collection = spline.GetApproximation().ToList();
			smoothedPath.vectorPath.AddRange(collection);
			return smoothedPath;
		}

		public static void DrawPath(SmoothedPath path, Color col, float duration)
		{
			for (int i = 1; i < path.vectorPath.Count; i++)
			{
				Debug.DrawLine(path.vectorPath[i - 1], path.vectorPath[i], col, duration);
			}
		}

		private static List<Vector3> InsertIntermediatePoints(List<Vector3> points, float maxDistance)
		{
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector3 a = points[i];
				Vector3 b = points[i + 1];
				float num = Vector3.Distance(a, b);
				if (num > maxDistance)
				{
					int num2 = Mathf.FloorToInt(num / maxDistance);
					for (int j = 0; j < num2; j++)
					{
						Vector3 item = Vector3.Lerp(a, b, (float)(j + 1) * (1f / (float)(num2 + 1)));
						points.Insert(i + (j + 1), item);
					}
				}
			}
			return points;
		}
	}
}
