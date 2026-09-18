using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolBuddy.ThirdParty.VectorGraphics
{
	internal class PathDistanceForwardIterator
	{
		private class BezierLoop : IList<BezierPathSegment>, ICollection<BezierPathSegment>, IEnumerable<BezierPathSegment>, IEnumerable
		{
			private IList<BezierPathSegment> OpenPath;

			public BezierPathSegment this[int index]
			{
				get
				{
					if (index == OpenPath.Count)
					{
						return OpenPath[0];
					}
					return OpenPath[index];
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			public int Count => OpenPath.Count + 1;

			public bool IsReadOnly => true;

			public BezierLoop(IList<BezierPathSegment> openPath)
			{
				OpenPath = openPath;
			}

			public void Add(BezierPathSegment item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
			}

			public bool Contains(BezierPathSegment item)
			{
				throw new NotImplementedException();
			}

			public void CopyTo(BezierPathSegment[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public IEnumerator<BezierPathSegment> GetEnumerator()
			{
				throw new NotImplementedException();
			}

			public int IndexOf(BezierPathSegment item)
			{
				throw new NotImplementedException();
			}

			public void Insert(int index, BezierPathSegment item)
			{
				throw new NotSupportedException();
			}

			public bool Remove(BezierPathSegment item)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		public enum Result
		{
			Stepped = 0,
			NewSegment = 1,
			Ended = 2
		}

		private readonly bool closed;

		private readonly bool needTangentsDuringEval;

		private readonly float maxCordDeviationSq;

		private readonly float maxTanAngleDevCosine;

		private readonly float stepSizeT;

		private int currentSegment;

		private float currentT;

		private float segmentLengthSoFar;

		private float lengthSoFar;

		private Vector2 lastPointEval;

		private Vector2 currentTTangent;

		private BezierSegment currentBezSeg;

		public IList<BezierPathSegment> Segments { get; }

		public bool Closed => closed;

		public int CurrentSegment => currentSegment;

		public float CurrentT => currentT;

		public float LengthSoFar => lengthSoFar;

		public float SegmentLengthSoFar => segmentLengthSoFar;

		public bool Ended
		{
			get
			{
				if (currentT == 1f)
				{
					return currentSegment + 1 == Segments.Count - 1;
				}
				return false;
			}
		}

		public PathDistanceForwardIterator(IList<BezierPathSegment> pathSegments, bool closed, float maxCordDeviationSq, float maxTanAngleDevCosine, float stepSizeT)
		{
			if (pathSegments.Count < 2)
			{
				throw new Exception("Cannot iterate a path with no segments in it");
			}
			IList<BezierPathSegment> list;
			if (!closed || VectorUtils.PathEndsPerfectlyMatch(pathSegments))
			{
				list = pathSegments;
			}
			else
			{
				IList<BezierPathSegment> list2 = new BezierLoop(pathSegments);
				list = list2;
			}
			Segments = list;
			this.closed = closed;
			needTangentsDuringEval = maxTanAngleDevCosine < 1f;
			this.maxCordDeviationSq = maxCordDeviationSq;
			this.maxTanAngleDevCosine = maxTanAngleDevCosine;
			this.stepSizeT = stepSizeT;
			currentBezSeg = new BezierSegment
			{
				P0 = pathSegments[0].P0,
				P1 = pathSegments[0].P1,
				P2 = pathSegments[0].P2,
				P3 = pathSegments[1].P0
			};
			lastPointEval = pathSegments[0].P0;
			currentTTangent = (needTangentsDuringEval ? VectorUtils.EvalTangent(currentBezSeg, 0f) : Vector2.zero);
		}

		private float PointToLineDistanceSq(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
		{
			float sqrMagnitude = (lineEnd - lineStart).sqrMagnitude;
			if (sqrMagnitude < VectorUtils.Epsilon)
			{
				return (point - lineStart).sqrMagnitude;
			}
			float num = (lineEnd.y - lineStart.y) * point.x - (lineEnd.x - lineStart.x) * point.y + lineEnd.x * lineStart.y - lineEnd.y * lineStart.x;
			return num * num / sqrMagnitude;
		}

		public Result AdvanceBy(float units, out float unitsRemaining)
		{
			unitsRemaining = units;
			if (Ended)
			{
				return Result.Ended;
			}
			float num = currentT;
			Vector2 vector = lastPointEval;
			Vector2 tangent;
			while (true)
			{
				float num2 = Mathf.Min(num + stepSizeT, 1f);
				tangent = Vector2.zero;
				Vector2 vector2 = (needTangentsDuringEval ? VectorUtils.EvalFull(currentBezSeg, num2, out tangent) : VectorUtils.Eval(currentBezSeg, num2));
				bool flag = false;
				if (needTangentsDuringEval)
				{
					flag = Vector2.Dot(tangent, currentTTangent) < maxTanAngleDevCosine;
				}
				if (!flag && maxCordDeviationSq != float.MaxValue)
				{
					Vector2 vector3 = vector;
					if ((vector2 - vector3).sqrMagnitude > VectorUtils.Epsilon)
					{
						Vector2 lineEnd = VectorUtils.Eval(currentBezSeg, Mathf.Min((num2 - currentT) * 2f + currentT, 1f));
						flag = PointToLineDistanceSq(vector2, vector3, lineEnd) >= maxCordDeviationSq;
					}
				}
				float num3 = (vector2 - lastPointEval).magnitude;
				if (num3 > unitsRemaining)
				{
					num2 = num + stepSizeT * (unitsRemaining / num3);
					num3 = unitsRemaining;
					vector2 = VectorUtils.Eval(currentBezSeg, num2);
				}
				segmentLengthSoFar += num3;
				lengthSoFar += num3;
				unitsRemaining -= num3;
				lastPointEval = vector2;
				num = num2;
				if (!(num2 < 1f))
				{
					break;
				}
				if (!(unitsRemaining > 0f) || flag)
				{
					currentT = num2;
					currentTTangent = tangent;
					return Result.Stepped;
				}
			}
			if (currentSegment + 1 == Segments.Count - 1)
			{
				currentT = 1f;
				return Result.Ended;
			}
			currentSegment++;
			currentBezSeg = new BezierSegment
			{
				P0 = Segments[currentSegment].P0,
				P1 = Segments[currentSegment].P1,
				P2 = Segments[currentSegment].P2,
				P3 = Segments[currentSegment + 1].P0
			};
			segmentLengthSoFar = 0f;
			currentT = 0f;
			currentTTangent = tangent;
			lastPointEval = currentBezSeg.P0;
			return Result.NewSegment;
		}

		public Vector2 EvalCurrent()
		{
			return VectorUtils.Eval(currentBezSeg, currentT);
		}
	}
}
