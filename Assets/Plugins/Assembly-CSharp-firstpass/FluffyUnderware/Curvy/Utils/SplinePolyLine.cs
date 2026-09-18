using System;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

namespace FluffyUnderware.Curvy.Utils
{
	[Serializable]
	public class SplinePolyLine
	{
		public enum VertexCalculation
		{
			ByApproximation = 0,
			ByAngle = 1
		}

		public ContourOrientation Orientation;

		public CurvySpline Spline;

		public VertexCalculation VertexMode;

		public float Angle;

		public float Distance;

		public Space Space;

		public bool IsClosed
		{
			get
			{
				if ((bool)Spline)
				{
					return Spline.Closed;
				}
				return false;
			}
		}

		public SplinePolyLine(CurvySpline spline)
			: this(spline, VertexCalculation.ByApproximation, 0f, 0f)
		{
		}

		public SplinePolyLine(CurvySpline spline, float angle, float distance)
			: this(spline, VertexCalculation.ByAngle, angle, distance)
		{
		}

		private SplinePolyLine(CurvySpline spline, VertexCalculation vertexMode, float angle, float distance, Space space = Space.World)
		{
			Spline = spline;
			VertexMode = vertexMode;
			Angle = angle;
			Distance = distance;
			Space = space;
		}

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public Vector3[] GetVertices()
		{
			SubArray<Vector3> vertexList = GetVertexList();
			Vector3[] result = vertexList.CopyToArray(ArrayPools.Vector3);
			ArrayPools.Vector3.Free(vertexList);
			return result;
		}

		public SubArray<Vector3> GetVertexList()
		{
			SubArray<Vector3> result = ((VertexMode != VertexCalculation.ByAngle) ? Spline.GetPositionsCache(Space.Self) : GetPolygon(Spline, 0f, 1f, Angle, Distance, -1f, includeEndPoint: false).ToSubArray());
			if (Space == Space.World)
			{
				Vector3[] array = result.Array;
				int count = result.Count;
				for (int i = 0; i < count; i++)
				{
					array[i] = Spline.transform.TransformPoint(array[i]);
				}
			}
			return result;
		}

		private static SubArrayList<Vector3> GetPolygon(CurvySpline spline, float fromTF, float toTF, float maxAngle, float minDistance, float maxDistance, bool includeEndPoint = true, float stepSize = 0.01f)
		{
			stepSize = Mathf.Clamp(stepSize, 0.002f, 1f);
			maxDistance = ((maxDistance == -1f) ? spline.Length : Mathf.Clamp(maxDistance, 0f, spline.Length));
			minDistance = Mathf.Clamp(minDistance, 0f, maxDistance);
			if (!spline.Closed)
			{
				toTF = Mathf.Clamp01(toTF);
				fromTF = Mathf.Clamp(fromTF, 0f, toTF);
			}
			SubArrayList<Vector3> vPos = new SubArrayList<Vector3>(50, ArrayPools.Vector3);
			int linearSteps = 0;
			float angleFromLast = 0f;
			float distAccu = 0f;
			Vector3 position2 = spline.Interpolate(fromTF);
			Vector3 tangent = spline.GetTangent(fromTF);
			Vector3 vector = position2;
			Vector3 vector2 = tangent;
			Action<Vector3> action = delegate(Vector3 position)
			{
				vPos.Add(position);
				angleFromLast = 0f;
				distAccu = 0f;
				linearSteps = 0;
			};
			action(position2);
			float num = fromTF + stepSize;
			while (num < toTF)
			{
				spline.InterpolateAndGetTangent(num % 1f, out position2, out tangent);
				if (tangent == Vector3.zero)
				{
					Debug.Log("zero Tangent! Oh no!");
				}
				distAccu += (position2 - vector).magnitude;
				if (tangent == vector2)
				{
					linearSteps++;
				}
				if (distAccu >= minDistance)
				{
					if (distAccu >= maxDistance)
					{
						action(position2);
					}
					else
					{
						angleFromLast += Vector3.Angle(vector2, tangent);
						if (angleFromLast >= maxAngle || (linearSteps > 0 && angleFromLast > 0f))
						{
							action(position2);
						}
					}
				}
				num += stepSize;
				vector = position2;
				vector2 = tangent;
			}
			if (includeEndPoint)
			{
				position2 = spline.Interpolate(toTF % 1f);
				vPos.Add(position2);
			}
			return vPos;
		}
	}
}
