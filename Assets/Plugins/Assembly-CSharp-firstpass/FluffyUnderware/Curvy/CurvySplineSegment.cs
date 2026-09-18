using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using ToolBuddy.Pooling.Pools;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy
{
	[ExecuteAlways]
	[HelpURL("https://curvyeditor.com/doclink/curvysplinesegment")]
	public class CurvySplineSegment : DTVersionedMonoBehaviour, IPoolable
	{
		private class Approximations
		{
			public SubArray<Vector3> Positions;

			public SubArray<Vector3> Tangents;

			public SubArray<Vector3> Ups;

			public SubArray<float> Distances;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ResizePositions(int size)
			{
				ArrayPools.Vector3.ResizeCopyless(ref Positions, size);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ResizeTangents(int size)
			{
				ArrayPools.Vector3.ResizeCopyless(ref Tangents, size);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ResizeUps(int size)
			{
				ArrayPools.Vector3.ResizeCopyless(ref Ups, size);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void ResizeDistances(int size)
			{
				ArrayPools.Single.ResizeCopyless(ref Distances, size);
			}

			public Approximations()
			{
				Initialize();
			}

			public void Clear()
			{
				Free();
				Initialize();
			}

			private void Initialize()
			{
				Positions = new SubArray<Vector3>(Array.Empty<Vector3>());
				Tangents = new SubArray<Vector3>(Array.Empty<Vector3>());
				Ups = new SubArray<Vector3>(Array.Empty<Vector3>());
				Distances = new SubArray<float>(Array.Empty<float>());
			}

			private void Free()
			{
				ArrayPools.Vector3.Free(Positions);
				ArrayPools.Vector3.Free(Tangents);
				ArrayPools.Vector3.Free(Ups);
				ArrayPools.Single.Free(Distances);
			}
		}

		private static class ApproximationsSetter
		{
			public static void SetPositionsToPoint([NotNull] Approximations approximations, Vector3 currentPosition)
			{
				approximations.ResizePositions(1);
				approximations.Positions.Array[0] = currentPosition;
			}

			public static void SetPositionsToLinear([NotNull] Approximations approximations, int elementCount, Vector3 startPosition, Vector3 endPosition)
			{
				approximations.ResizePositions(elementCount);
				float num = 1f / (float)(elementCount - 1);
				Vector3[] array = approximations.Positions.Array;
				array[0] = startPosition;
				for (int i = 1; i < elementCount - 1; i++)
				{
					array[i] = startPosition.LerpUnclamped(endPosition, (float)i * num);
				}
				array[elementCount - 1] = endPosition;
			}

			public static void SetPositionsToCatmullRom([NotNull] Approximations approximations, int elementCount, Vector3 startPosition, Vector3 endPosition, Vector3 preSegmentPosition, Vector3 postSegmentPosition)
			{
				approximations.ResizePositions(elementCount);
				float num = 1f / (float)(elementCount - 1);
				double num2 = -0.5 * (double)preSegmentPosition.x + 1.5 * (double)startPosition.x + -1.5 * (double)endPosition.x + 0.5 * (double)postSegmentPosition.x;
				double num3 = (double)preSegmentPosition.x + -2.5 * (double)startPosition.x + 2.0 * (double)endPosition.x + -0.5 * (double)postSegmentPosition.x;
				double num4 = -0.5 * (double)preSegmentPosition.x + 0.5 * (double)endPosition.x;
				double num5 = startPosition.x;
				double num6 = -0.5 * (double)preSegmentPosition.y + 1.5 * (double)startPosition.y + -1.5 * (double)endPosition.y + 0.5 * (double)postSegmentPosition.y;
				double num7 = (double)preSegmentPosition.y + -2.5 * (double)startPosition.y + 2.0 * (double)endPosition.y + -0.5 * (double)postSegmentPosition.y;
				double num8 = -0.5 * (double)preSegmentPosition.y + 0.5 * (double)endPosition.y;
				double num9 = startPosition.y;
				double num10 = -0.5 * (double)preSegmentPosition.z + 1.5 * (double)startPosition.z + -1.5 * (double)endPosition.z + 0.5 * (double)postSegmentPosition.z;
				double num11 = (double)preSegmentPosition.z + -2.5 * (double)startPosition.z + 2.0 * (double)endPosition.z + -0.5 * (double)postSegmentPosition.z;
				double num12 = -0.5 * (double)preSegmentPosition.z + 0.5 * (double)endPosition.z;
				double num13 = startPosition.z;
				Vector3[] array = approximations.Positions.Array;
				array[0] = startPosition;
				for (int i = 1; i < elementCount - 1; i++)
				{
					float num14 = (float)i * num;
					array[i].x = (float)(((num2 * (double)num14 + num3) * (double)num14 + num4) * (double)num14 + num5);
					array[i].y = (float)(((num6 * (double)num14 + num7) * (double)num14 + num8) * (double)num14 + num9);
					array[i].z = (float)(((num10 * (double)num14 + num11) * (double)num14 + num12) * (double)num14 + num13);
				}
				array[elementCount - 1] = endPosition;
			}

			public static void SetPositionsToTCB([NotNull] Approximations approximations, int elementCount, TcbParameters tcbParameters, Vector3 startPosition, Vector3 endPosition, Vector3 preSegmentPosition, Vector3 postSegmentPosition)
			{
				approximations.ResizePositions(elementCount);
				float num = 1f / (float)(elementCount - 1);
				float startTension = tcbParameters.StartTension;
				float endTension = tcbParameters.EndTension;
				float startContinuity = tcbParameters.StartContinuity;
				float endContinuity = tcbParameters.EndContinuity;
				float startBias = tcbParameters.StartBias;
				float endBias = tcbParameters.EndBias;
				double num2 = (1f - startTension) * (1f + startContinuity) * (1f + startBias);
				double num3 = (1f - startTension) * (1f - startContinuity) * (1f - startBias);
				double num4 = (1f - endTension) * (1f - endContinuity) * (1f + endBias);
				double num5 = (1f - endTension) * (1f + endContinuity) * (1f - endBias);
				double num6 = 2.0;
				double num7 = (0.0 - num2) / num6;
				double num8 = (4.0 + num2 - num3 - num4) / num6;
				double num9 = (-4.0 + num3 + num4 - num5) / num6;
				double num10 = num5 / num6;
				double num11 = 2.0 * num2 / num6;
				double num12 = (-6.0 - 2.0 * num2 + 2.0 * num3 + num4) / num6;
				double num13 = (6.0 - 2.0 * num3 - num4 + num5) / num6;
				double num14 = (0.0 - num5) / num6;
				double num15 = (0.0 - num2) / num6;
				double num16 = (num2 - num3) / num6;
				double num17 = num3 / num6;
				double num18 = 2.0 / num6;
				double num19 = num7 * (double)preSegmentPosition.x + num8 * (double)startPosition.x + num9 * (double)endPosition.x + num10 * (double)postSegmentPosition.x;
				double num20 = num11 * (double)preSegmentPosition.x + num12 * (double)startPosition.x + num13 * (double)endPosition.x + num14 * (double)postSegmentPosition.x;
				double num21 = num15 * (double)preSegmentPosition.x + num16 * (double)startPosition.x + num17 * (double)endPosition.x;
				double num22 = num18 * (double)startPosition.x;
				double num23 = num7 * (double)preSegmentPosition.y + num8 * (double)startPosition.y + num9 * (double)endPosition.y + num10 * (double)postSegmentPosition.y;
				double num24 = num11 * (double)preSegmentPosition.y + num12 * (double)startPosition.y + num13 * (double)endPosition.y + num14 * (double)postSegmentPosition.y;
				double num25 = num15 * (double)preSegmentPosition.y + num16 * (double)startPosition.y + num17 * (double)endPosition.y;
				double num26 = num18 * (double)startPosition.y;
				double num27 = num7 * (double)preSegmentPosition.z + num8 * (double)startPosition.z + num9 * (double)endPosition.z + num10 * (double)postSegmentPosition.z;
				double num28 = num11 * (double)preSegmentPosition.z + num12 * (double)startPosition.z + num13 * (double)endPosition.z + num14 * (double)postSegmentPosition.z;
				double num29 = num15 * (double)preSegmentPosition.z + num16 * (double)startPosition.z + num17 * (double)endPosition.z;
				double num30 = num18 * (double)startPosition.z;
				Vector3[] array = approximations.Positions.Array;
				array[0] = startPosition;
				for (int i = 1; i < elementCount - 1; i++)
				{
					float num31 = (float)i * num;
					array[i].x = (float)(((num19 * (double)num31 + num20) * (double)num31 + num21) * (double)num31 + num22);
					array[i].y = (float)(((num23 * (double)num31 + num24) * (double)num31 + num25) * (double)num31 + num26);
					array[i].z = (float)(((num27 * (double)num31 + num28) * (double)num31 + num29) * (double)num31 + num30);
				}
				array[elementCount - 1] = endPosition;
			}

			public static void SetPositionsToBezier([NotNull] Approximations approximations, int elementCount, Vector3 startPosition, Vector3 startTangent, Vector3 endPosition, Vector3 endTangent)
			{
				approximations.ResizePositions(elementCount);
				float num = 1f / (float)(elementCount - 1);
				double num2 = (double)(0f - startPosition.x) + 3.0 * (double)startTangent.x + -3.0 * (double)endTangent.x + (double)endPosition.x;
				double num3 = 3.0 * (double)startPosition.x + -6.0 * (double)startTangent.x + 3.0 * (double)endTangent.x;
				double num4 = -3.0 * (double)startPosition.x + 3.0 * (double)startTangent.x;
				double num5 = startPosition.x;
				double num6 = (double)(0f - startPosition.y) + 3.0 * (double)startTangent.y + -3.0 * (double)endTangent.y + (double)endPosition.y;
				double num7 = 3.0 * (double)startPosition.y + -6.0 * (double)startTangent.y + 3.0 * (double)endTangent.y;
				double num8 = -3.0 * (double)startPosition.y + 3.0 * (double)startTangent.y;
				double num9 = startPosition.y;
				double num10 = (double)(0f - startPosition.z) + 3.0 * (double)startTangent.z + -3.0 * (double)endTangent.z + (double)endPosition.z;
				double num11 = 3.0 * (double)startPosition.z + -6.0 * (double)startTangent.z + 3.0 * (double)endTangent.z;
				double num12 = -3.0 * (double)startPosition.z + 3.0 * (double)startTangent.z;
				double num13 = startPosition.z;
				Vector3[] array = approximations.Positions.Array;
				array[0] = startPosition;
				for (int i = 1; i < elementCount - 1; i++)
				{
					float num14 = (float)i * num;
					array[i].x = (float)(((num2 * (double)num14 + num3) * (double)num14 + num4) * (double)num14 + num5);
					array[i].y = (float)(((num6 * (double)num14 + num7) * (double)num14 + num8) * (double)num14 + num9);
					array[i].z = (float)(((num10 * (double)num14 + num11) * (double)num14 + num12) * (double)num14 + num13);
				}
				array[elementCount - 1] = endPosition;
			}

			public static void SetPositionsToBSpline([NotNull] Approximations approximations, int elementCount, SubArray<Vector3> splineP0Array, BSplineApproximationParameters bSplineParameters)
			{
				approximations.ResizePositions(elementCount);
				float num = 1f / (float)(elementCount - 1) / (float)bSplineParameters.SegmentsCount;
				int count = bSplineParameters.ControlPoints.Count;
				int bSplineN = BSplineHelper.GetBSplineN(count, bSplineParameters.Degree, bSplineParameters.IsClosed);
				int num2 = int.MinValue;
				int nPlus = bSplineN + 1;
				Vector3[] array = approximations.Positions.Array;
				SubArray<Vector3> subArray = splineP0Array;
				Vector3[] array2 = subArray.Array;
				int count2 = subArray.Count;
				BSplineHelper.GetBSplineUAndK(bSplineParameters.StartTf, bSplineParameters.IsClamped, bSplineParameters.Degree, bSplineN, out var u, out var k);
				GetBSplineP0s(bSplineParameters.ControlPoints, count, bSplineParameters.Degree, k, array2);
				array[0] = (bSplineParameters.IsClamped ? BSplineHelper.DeBoorClamped(bSplineParameters.Degree, k, u, nPlus, array2) : BSplineHelper.DeBoorUnclamped(bSplineParameters.Degree, k, u, array2));
				SubArray<Vector3> subArray2 = ArrayPools.Vector3.Allocate(count2);
				Vector3[] array3 = subArray2.Array;
				for (int i = 1; i < elementCount - 1; i++)
				{
					BSplineHelper.GetBSplineUAndK(bSplineParameters.StartTf + num * (float)i, bSplineParameters.IsClamped, bSplineParameters.Degree, bSplineN, out var u2, out var k2);
					if (k2 != num2)
					{
						GetBSplineP0s(bSplineParameters.ControlPoints, count, bSplineParameters.Degree, k2, array2);
						num2 = k2;
					}
					Array.Copy(array2, 0, array3, 0, count2);
					array[i] = (bSplineParameters.IsClamped ? BSplineHelper.DeBoorClamped(bSplineParameters.Degree, k2, u2, nPlus, array3) : BSplineHelper.DeBoorUnclamped(bSplineParameters.Degree, k2, u2, array3));
				}
				ArrayPools.Vector3.Free(subArray2);
				BSplineHelper.GetBSplineUAndK(bSplineParameters.EndTf, bSplineParameters.IsClamped, bSplineParameters.Degree, bSplineN, out var u3, out var k3);
				GetBSplineP0s(bSplineParameters.ControlPoints, count, bSplineParameters.Degree, k3, array2);
				array[elementCount - 1] = (bSplineParameters.IsClamped ? BSplineHelper.DeBoorClamped(bSplineParameters.Degree, k3, u3, nPlus, array2) : BSplineHelper.DeBoorUnclamped(bSplineParameters.Degree, k3, u3, array2));
			}

			public static void SetOrientationToNone([NotNull] Approximations approximations, int elementCount)
			{
				approximations.ResizeUps(elementCount);
				Array.Clear(approximations.Ups.Array, 0, approximations.Ups.Count);
			}

			public static void SetOrientationToStatic([NotNull] Approximations approximations, int elementCount, Vector3 startUp, Vector3 endUp)
			{
				approximations.ResizeUps(elementCount);
				Vector3[] array = approximations.Ups.Array;
				array[0] = startUp;
				if (approximations.Ups.Count > 1)
				{
					float num = 1f / (float)(elementCount - 1);
					for (int i = 1; i < elementCount - 1; i++)
					{
						array[i] = Vector3.SlerpUnclamped(startUp, endUp, (float)i * num);
					}
					array[elementCount - 1] = endUp;
				}
			}

			public static void SetOrientationToDynamic([NotNull] Approximations approximations, int elementCount, Vector3 startUp)
			{
				approximations.ResizeUps(elementCount);
				Vector3[] array = approximations.Ups.Array;
				Vector3[] array2 = approximations.Tangents.Array;
				int count = approximations.Ups.Count;
				array[0] = startUp;
				Vector3 axis = default(Vector3);
				for (int i = 1; i < count; i++)
				{
					Vector3 vector = array2[i - 1];
					Vector3 vector2 = array2[i];
					axis.x = vector.y * vector2.z - vector.z * vector2.y;
					axis.y = vector.z * vector2.x - vector.x * vector2.z;
					axis.z = vector.x * vector2.y - vector.y * vector2.x;
					float num = (float)Math.Atan2(Math.Sqrt(axis.x * axis.x + axis.y * axis.y + axis.z * axis.z), vector.x * vector2.x + vector.y * vector2.y + vector.z * vector2.z);
					array[i] = Quaternion.AngleAxis(57.29578f * num, axis) * array[i - 1];
				}
			}

			public static float SetPointTangentsAndDistances([NotNull] Approximations approximations, Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPosition, Quaternion currentRotation)
			{
				approximations.ResizeTangents(1);
				approximations.ResizeDistances(1);
				approximations.Distances.Array[0] = 0f;
				if (currentPosition != nextPosition)
				{
					approximations.Tangents.Array[0] = nextPosition.Subtraction(currentPosition).normalized;
				}
				else if (currentPosition != previousPosition)
				{
					approximations.Tangents.Array[0] = currentPosition.Subtraction(previousPosition).normalized;
				}
				else
				{
					approximations.Tangents.Array[0] = currentRotation * Vector3.forward;
				}
				return 0f;
			}

			public static float SetSegmentTangentsAnDistances([NotNull] Approximations approximations, int elementCount)
			{
				approximations.ResizeTangents(elementCount);
				approximations.ResizeDistances(elementCount);
				Vector3[] array = approximations.Positions.Array;
				float num = 0f;
				Vector3[] array2 = approximations.Tangents.Array;
				float[] array3 = approximations.Distances.Array;
				array3[0] = 0f;
				Vector3 vector = default(Vector3);
				for (int i = 1; i < elementCount; i++)
				{
					vector.x = array[i].x - array[i - 1].x;
					vector.y = array[i].y - array[i - 1].y;
					vector.z = array[i].z - array[i - 1].z;
					float num2 = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
					num = (array3[i] = num + num2);
					if ((double)num2 > 9.99999974737875E-06)
					{
						float num3 = 1f / num2;
						array2[i - 1].x = vector.x * num3;
						array2[i - 1].y = vector.y * num3;
						array2[i - 1].z = vector.z * num3;
					}
					else
					{
						array2[i - 1].x = 0f;
						array2[i - 1].y = 0f;
						array2[i - 1].z = 0f;
					}
				}
				array2[elementCount - 1] = array2[elementCount - 2];
				return num;
			}
		}

		private struct BSplineApproximationParameters : IEquatable<BSplineApproximationParameters>
		{
			public int Degree { get; }

			public bool IsClamped { get; }

			public bool IsClosed { get; }

			public float StartTf { get; }

			public float EndTf { get; }

			[NotNull]
			public ReadOnlyCollection<CurvySplineSegment> ControlPoints { get; }

			public int SegmentsCount { get; }

			public BSplineApproximationParameters([NotNull] CurvySplineSegment segment)
			{
				CurvySpline spline = segment.Spline;
				Degree = spline.BSplineDegree;
				IsClamped = spline.IsBSplineClamped;
				IsClosed = spline.Closed;
				StartTf = spline.SegmentToTF(segment);
				EndTf = spline.SegmentToTF(segment, 1f);
				ControlPoints = spline.ControlPointsList;
				SegmentsCount = spline.Count;
			}

			public bool Equals(BSplineApproximationParameters other)
			{
				if (Degree == other.Degree && IsClamped == other.IsClamped && IsClosed == other.IsClosed && StartTf.Equals(other.StartTf) && EndTf.Equals(other.EndTf) && ControlPoints.Equals(other.ControlPoints))
				{
					return SegmentsCount == other.SegmentsCount;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is BSplineApproximationParameters other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((((((((((Degree * 397) ^ IsClamped.GetHashCode()) * 397) ^ IsClosed.GetHashCode()) * 397) ^ StartTf.GetHashCode()) * 397) ^ EndTf.GetHashCode()) * 397) ^ ControlPoints.GetHashCode()) * 397) ^ SegmentsCount;
			}

			public static bool operator ==(BSplineApproximationParameters left, BSplineApproximationParameters right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(BSplineApproximationParameters left, BSplineApproximationParameters right)
			{
				return !left.Equals(right);
			}
		}

		internal readonly struct ControlPointExtrinsicProperties : IEquatable<ControlPointExtrinsicProperties>
		{
			private readonly bool isVisible;

			private readonly float tf;

			private readonly short segmentIndex;

			private readonly short controlPointIndex;

			private readonly short nextControlPointIndex;

			private readonly short previousControlPointIndex;

			private readonly bool previousControlPointIsSegment;

			private readonly bool nextControlPointIsSegment;

			private readonly bool canHaveFollowUp;

			internal bool IsVisible => isVisible;

			internal float TF => tf;

			internal short SegmentIndex => segmentIndex;

			internal short ControlPointIndex => controlPointIndex;

			internal short NextControlPointIndex => nextControlPointIndex;

			internal short PreviousControlPointIndex => previousControlPointIndex;

			internal bool PreviousControlPointIsSegment => previousControlPointIsSegment;

			internal bool NextControlPointIsSegment => nextControlPointIsSegment;

			internal bool CanHaveFollowUp => canHaveFollowUp;

			internal bool IsSegment => SegmentIndex != -1;

			[UsedImplicitly]
			[Obsolete("Use CurvySpline.GetControlPointOrientationAnchorIndex() instead")]
			internal short OrientationAnchorIndex
			{
				get
				{
					throw new NotSupportedException("Use CurvySpline.GetControlPointOrientationAnchorIndex() instead");
				}
			}

			[UsedImplicitly]
			[Obsolete("Use the other constructor")]
			internal ControlPointExtrinsicProperties(bool isVisible, float tf, short segmentIndex, short controlPointIndex, short previousControlPointIndex, short nextControlPointIndex, bool previousControlPointIsSegment, bool nextControlPointIsSegment, bool canHaveFollowUp, short orientationAnchorIndex)
				: this(isVisible, tf, segmentIndex, controlPointIndex, previousControlPointIndex, nextControlPointIndex, previousControlPointIsSegment, nextControlPointIsSegment, canHaveFollowUp)
			{
			}

			internal ControlPointExtrinsicProperties(bool isVisible, float tf, short segmentIndex, short controlPointIndex, short previousControlPointIndex, short nextControlPointIndex, bool previousControlPointIsSegment, bool nextControlPointIsSegment, bool canHaveFollowUp)
			{
				this.isVisible = isVisible;
				this.tf = tf;
				this.segmentIndex = segmentIndex;
				this.controlPointIndex = controlPointIndex;
				this.nextControlPointIndex = nextControlPointIndex;
				this.previousControlPointIndex = previousControlPointIndex;
				this.previousControlPointIsSegment = previousControlPointIsSegment;
				this.nextControlPointIsSegment = nextControlPointIsSegment;
				this.canHaveFollowUp = canHaveFollowUp;
			}

			public bool Equals(ControlPointExtrinsicProperties other)
			{
				if (IsVisible == other.IsVisible && TF == other.TF && SegmentIndex == other.SegmentIndex && ControlPointIndex == other.ControlPointIndex && NextControlPointIndex == other.NextControlPointIndex && PreviousControlPointIndex == other.PreviousControlPointIndex && PreviousControlPointIsSegment == other.PreviousControlPointIsSegment && NextControlPointIsSegment == other.NextControlPointIsSegment)
				{
					return CanHaveFollowUp == other.CanHaveFollowUp;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					return false;
				}
				if (obj is ControlPointExtrinsicProperties)
				{
					return Equals((ControlPointExtrinsicProperties)obj);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((((((((((((((IsVisible.GetHashCode() * 397) ^ TF.GetHashCode()) * 397) ^ SegmentIndex.GetHashCode()) * 397) ^ ControlPointIndex.GetHashCode()) * 397) ^ NextControlPointIndex.GetHashCode()) * 397) ^ PreviousControlPointIndex.GetHashCode()) * 397) ^ PreviousControlPointIsSegment.GetHashCode()) * 397) ^ NextControlPointIsSegment.GetHashCode()) * 397) ^ CanHaveFollowUp.GetHashCode();
			}

			public static bool operator ==(ControlPointExtrinsicProperties left, ControlPointExtrinsicProperties right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(ControlPointExtrinsicProperties left, ControlPointExtrinsicProperties right)
			{
				return !left.Equals(right);
			}
		}

		private class ThreadSafeData
		{
			public Vector3 ThreadSafeLocalPosition;

			public Vector3 ThreadSafeNextCpLocalPosition;

			public Vector3 ThreadSafePreviousCpLocalPosition;

			public Quaternion ThreadSafeLocalRotation;

			internal void Set(bool useFollowUp, CurvySplineSegment curvySplineSegment, out CurvySplineSegment nextCP)
			{
				CurvySpline spline = curvySplineSegment.Spline;
				Transform cachedTransform = curvySplineSegment.cachedTransform;
				CurvySplineSegment previousControlPoint = spline.GetPreviousControlPoint(curvySplineSegment);
				nextCP = spline.GetNextControlPoint(curvySplineSegment);
				ThreadSafeLocalPosition = cachedTransform.localPosition;
				ThreadSafeLocalRotation = cachedTransform.localRotation;
				if (useFollowUp)
				{
					bool num = curvySplineSegment.FollowUp != null;
					CurvySplineSegment curvySplineSegment2 = ((!num || (object)spline.FirstVisibleControlPoint != curvySplineSegment) ? previousControlPoint : CurvySpline.GetFollowUpHeadingControlPoint(curvySplineSegment.FollowUp, curvySplineSegment.FollowUpHeading));
					CurvySplineSegment curvySplineSegment3 = ((!num || (object)spline.LastVisibleControlPoint != curvySplineSegment) ? nextCP : CurvySpline.GetFollowUpHeadingControlPoint(curvySplineSegment.FollowUp, curvySplineSegment.FollowUpHeading));
					if (curvySplineSegment2 != null)
					{
						ThreadSafePreviousCpLocalPosition = (((object)curvySplineSegment2.Spline == spline) ? curvySplineSegment2.cachedTransform.localPosition : spline.transform.InverseTransformPoint(curvySplineSegment2.cachedTransform.position));
					}
					else
					{
						ThreadSafePreviousCpLocalPosition = ThreadSafeLocalPosition;
					}
					if (curvySplineSegment3 != null)
					{
						ThreadSafeNextCpLocalPosition = (((object)curvySplineSegment3.Spline == spline) ? curvySplineSegment3.cachedTransform.localPosition : spline.transform.InverseTransformPoint(curvySplineSegment3.cachedTransform.position));
					}
					else
					{
						ThreadSafeNextCpLocalPosition = ThreadSafeLocalPosition;
					}
				}
				else
				{
					ThreadSafePreviousCpLocalPosition = previousControlPoint?.cachedTransform.localPosition ?? ThreadSafeLocalPosition;
					ThreadSafeNextCpLocalPosition = (((object)nextCP != null) ? nextCP.cachedTransform.localPosition : ThreadSafeLocalPosition);
				}
			}
		}

		public static readonly Color GizmoTangentColor = new Color(0f, 0.7f, 0f);

		[Group("General")]
		[FieldAction("CBBakeOrientation", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[Label("Bake Orientation", "Automatically apply orientation to CP transforms?")]
		[SerializeField]
		private bool m_AutoBakeOrientation;

		[Group("General")]
		[Tooltip("Check to use this transform's rotation")]
		[FieldCondition("IsOrientationAnchorEditable", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private bool m_OrientationAnchor;

		[Label("Swirl", "Add Swirl to orientation?")]
		[Group("General")]
		[FieldCondition("CanHaveSwirl", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		private CurvyOrientationSwirl m_Swirl;

		[Label("Turns", "Number of swirl turns")]
		[Group("General")]
		[FieldCondition("CanHaveSwirl", true, false, FluffyUnderware.DevTools.ConditionalAttribute.OperatorEnum.AND, "m_Swirl", CurvyOrientationSwirl.None, true)]
		[SerializeField]
		private float m_SwirlTurns;

		[Section("Bezier Options", true, false, 100, Sort = 1, HelpURL = "https://curvyeditor.com/doclink/curvysplinesegment_bezier")]
		[GroupCondition("Interpolation", CurvyInterpolation.Bezier, false)]
		[SerializeField]
		private bool m_AutoHandles = true;

		[RangeEx(0f, 1f, "Distance %", "Handle length by distance to neighbours")]
		[FieldCondition("m_AutoHandles", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below, Action = ActionAttribute.ActionEnum.Enable)]
		[SerializeField]
		private float m_AutoHandleDistance = 0.39f;

		[VectorEx("", "", Precision = 3, Options = (AttributeOptionsFlags)1152, Color = "#FFFF00")]
		[SerializeField]
		[FormerlySerializedAs("HandleIn")]
		private Vector3 m_HandleIn = CurvySplineSegmentDefaultValues.HandleIn;

		[VectorEx("", "", Precision = 3, Options = (AttributeOptionsFlags)1152, Color = "#00FF00")]
		[SerializeField]
		[FormerlySerializedAs("HandleOut")]
		private Vector3 m_HandleOut = CurvySplineSegmentDefaultValues.HandleOut;

		[Section("TCB Options", true, false, 100, Sort = 1, HelpURL = "https://curvyeditor.com/doclink/curvysplinesegment_tcb")]
		[GroupCondition("Interpolation", CurvyInterpolation.TCB, false)]
		[GroupAction("TCBOptionsGUI", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[Label("Local Tension", "Override Spline Tension?")]
		[SerializeField]
		[FormerlySerializedAs("OverrideGlobalTension")]
		private bool m_OverrideGlobalTension;

		[Label("Local Continuity", "Override Spline Continuity?")]
		[SerializeField]
		[FormerlySerializedAs("OverrideGlobalContinuity")]
		private bool m_OverrideGlobalContinuity;

		[Label("Local Bias", "Override Spline Bias?")]
		[SerializeField]
		[FormerlySerializedAs("OverrideGlobalBias")]
		private bool m_OverrideGlobalBias;

		[Tooltip("Synchronize Start and End Values")]
		[SerializeField]
		[FormerlySerializedAs("SynchronizeTCB")]
		private bool m_SynchronizeTCB = true;

		[Label("Tension", "")]
		[FieldCondition("m_OverrideGlobalTension", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("StartTension")]
		private float m_StartTension;

		[Label("Tension (End)", "")]
		[FieldCondition("m_OverrideGlobalTension", true, false, FluffyUnderware.DevTools.ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
		[SerializeField]
		[FormerlySerializedAs("EndTension")]
		private float m_EndTension;

		[Label("Continuity", "")]
		[FieldCondition("m_OverrideGlobalContinuity", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("StartContinuity")]
		private float m_StartContinuity;

		[Label("Continuity (End)", "")]
		[FieldCondition("m_OverrideGlobalContinuity", true, false, FluffyUnderware.DevTools.ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
		[SerializeField]
		[FormerlySerializedAs("EndContinuity")]
		private float m_EndContinuity;

		[Label("Bias", "")]
		[FieldCondition("m_OverrideGlobalBias", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("StartBias")]
		private float m_StartBias;

		[Label("Bias (End)", "")]
		[FieldCondition("m_OverrideGlobalBias", true, false, FluffyUnderware.DevTools.ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
		[SerializeField]
		[FormerlySerializedAs("EndBias")]
		private float m_EndBias;

		[SerializeField]
		[HideInInspector]
		private CurvySplineSegment m_FollowUp;

		[SerializeField]
		[HideInInspector]
		private ConnectionHeadingEnum m_FollowUpHeading = ConnectionHeadingEnum.Auto;

		[SerializeField]
		[HideInInspector]
		private bool m_ConnectionSyncPosition;

		[SerializeField]
		[HideInInspector]
		private bool m_ConnectionSyncRotation;

		[SerializeField]
		[HideInInspector]
		private CurvyConnection m_Connection;

		private Transform cachedTransform;

		[CanBeNull]
		private CurvySplineSegment cachedNextControlPoint;

		[CanBeNull]
		private ThreadSafeData threadSafeData;

		private CurvySpline mSpline;

		private Bounds? mBounds;

		private readonly HashSet<CurvyMetadataBase> mMetadata = new HashSet<CurvyMetadataBase>();

		private Vector3? lastProcessedLocalPosition;

		private Quaternion? lastProcessedLocalRotation;

		private float distance = -1f;

		private float length = -1f;

		private SubArray<Vector3> bSplineP0Array;

		private ControlPointExtrinsicProperties extrinsicProperties;

		[NotNull]
		private readonly Approximations approximations = new Approximations();

		[UsedImplicitly]
		[Obsolete("Use GetPositionsApproximation instead")]
		public Vector3[] Approximation
		{
			get
			{
				return PositionsApproximation.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				ArrayPools.Vector3.Free(approximations.Positions);
				approximations.Positions = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use GetDistancesApproximation instead")]
		public float[] ApproximationDistances
		{
			get
			{
				return DistancesApproximation.CopyToArray(ArrayPools.Single);
			}
			set
			{
				ArrayPools.Single.Free(approximations.Distances);
				approximations.Distances = new SubArray<float>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use GetUpsApproximation instead")]
		public Vector3[] ApproximationUp
		{
			get
			{
				return UpsApproximation.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				ArrayPools.Vector3.Free(approximations.Ups);
				approximations.Ups = new SubArray<Vector3>(value);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use GetTangentsApproximation instead")]
		public Vector3[] ApproximationT
		{
			get
			{
				return TangentsApproximation.CopyToArray(ArrayPools.Vector3);
			}
			set
			{
				ArrayPools.Vector3.Free(approximations.Tangents);
				approximations.Tangents = new SubArray<Vector3>(value);
			}
		}

		public bool AutoBakeOrientation
		{
			get
			{
				return m_AutoBakeOrientation;
			}
			set
			{
				m_AutoBakeOrientation = value;
			}
		}

		public bool SerializedOrientationAnchor
		{
			get
			{
				return m_OrientationAnchor;
			}
			set
			{
				if (m_OrientationAnchor != value)
				{
					m_OrientationAnchor = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
					}
				}
			}
		}

		public CurvyOrientationSwirl Swirl
		{
			get
			{
				return m_Swirl;
			}
			set
			{
				if (m_Swirl != value)
				{
					m_Swirl = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
					}
				}
			}
		}

		public float SwirlTurns
		{
			get
			{
				return m_SwirlTurns;
			}
			set
			{
				if (m_SwirlTurns != value)
				{
					m_SwirlTurns = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
					}
				}
			}
		}

		public Vector3 HandleIn
		{
			get
			{
				return m_HandleIn;
			}
			set
			{
				if (m_HandleIn != value)
				{
					m_HandleIn = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public Vector3 HandleOut
		{
			get
			{
				return m_HandleOut;
			}
			set
			{
				if (m_HandleOut != value)
				{
					m_HandleOut = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public Vector3 HandleInPosition
		{
			get
			{
				return cachedTransform.position + Spline.transform.rotation * HandleIn;
			}
			set
			{
				HandleIn = Spline.transform.InverseTransformDirection(value - cachedTransform.position);
			}
		}

		public Vector3 HandleOutPosition
		{
			get
			{
				return cachedTransform.position + Spline.transform.rotation * HandleOut;
			}
			set
			{
				HandleOut = Spline.transform.InverseTransformDirection(value - cachedTransform.position);
			}
		}

		public bool AutoHandles
		{
			get
			{
				return m_AutoHandles;
			}
			set
			{
				if (SetAutoHandles(value) && CanTouchItsSpline)
				{
					Spline.SetDirty(this, SplineDirtyingType.Everything);
				}
			}
		}

		public float AutoHandleDistance
		{
			get
			{
				return m_AutoHandleDistance;
			}
			set
			{
				if (m_AutoHandleDistance == value)
				{
					return;
				}
				float num = Mathf.Clamp01(value);
				if (m_AutoHandleDistance != num)
				{
					m_AutoHandleDistance = num;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public bool SynchronizeTCB
		{
			get
			{
				return m_SynchronizeTCB;
			}
			set
			{
				if (m_SynchronizeTCB != value)
				{
					m_SynchronizeTCB = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public bool OverrideGlobalTension
		{
			get
			{
				return m_OverrideGlobalTension;
			}
			set
			{
				if (m_OverrideGlobalTension != value)
				{
					m_OverrideGlobalTension = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public bool OverrideGlobalContinuity
		{
			get
			{
				return m_OverrideGlobalContinuity;
			}
			set
			{
				if (m_OverrideGlobalContinuity != value)
				{
					m_OverrideGlobalContinuity = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public bool OverrideGlobalBias
		{
			get
			{
				return m_OverrideGlobalBias;
			}
			set
			{
				if (m_OverrideGlobalBias != value)
				{
					m_OverrideGlobalBias = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public float StartTension
		{
			get
			{
				return m_StartTension;
			}
			set
			{
				if (m_StartTension != value)
				{
					m_StartTension = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public float StartContinuity
		{
			get
			{
				return m_StartContinuity;
			}
			set
			{
				if (m_StartContinuity != value)
				{
					m_StartContinuity = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public float StartBias
		{
			get
			{
				return m_StartBias;
			}
			set
			{
				if (m_StartBias != value)
				{
					m_StartBias = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public float EndTension
		{
			get
			{
				return m_EndTension;
			}
			set
			{
				if (m_EndTension != value)
				{
					m_EndTension = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public float EndContinuity
		{
			get
			{
				return m_EndContinuity;
			}
			set
			{
				if (m_EndContinuity != value)
				{
					m_EndContinuity = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public float EndBias
		{
			get
			{
				return m_EndBias;
			}
			set
			{
				if (m_EndBias != value)
				{
					m_EndBias = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public TcbParameters EffectiveTcbParameters
		{
			get
			{
				CurvySpline spline = Spline;
				TcbParameters result = default(TcbParameters);
				ref TcbParameters reference = ref result;
				ref TcbParameters reference2 = ref result;
				if (!OverrideGlobalTension)
				{
					float tension = spline.Tension;
					float tension2 = spline.Tension;
					float num2 = (reference.StartTension = tension);
					num2 = (reference2.EndTension = tension2);
				}
				else
				{
					float tension2 = StartTension;
					float tension = EndTension;
					float num2 = (reference.StartTension = tension2);
					num2 = (reference2.EndTension = tension);
				}
				reference2 = ref result;
				reference = ref result;
				if (!OverrideGlobalContinuity)
				{
					float tension = spline.Continuity;
					float tension2 = spline.Continuity;
					float num2 = (reference2.StartContinuity = tension);
					num2 = (reference.EndContinuity = tension2);
				}
				else
				{
					float tension2 = StartContinuity;
					float tension = EndContinuity;
					float num2 = (reference2.StartContinuity = tension2);
					num2 = (reference.EndContinuity = tension);
				}
				reference = ref result;
				reference2 = ref result;
				if (!OverrideGlobalBias)
				{
					float tension = spline.Bias;
					float tension2 = spline.Bias;
					float num2 = (reference.StartBias = tension);
					num2 = (reference2.EndBias = tension2);
				}
				else
				{
					float tension2 = StartBias;
					float tension = EndBias;
					float num2 = (reference.StartBias = tension2);
					num2 = (reference2.EndBias = tension);
				}
				return result;
			}
		}

		[CanBeNull]
		public CurvySplineSegment FollowUp
		{
			get
			{
				return m_FollowUp;
			}
			private set
			{
				if ((object)m_FollowUp != value)
				{
					m_FollowUp = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public ConnectionHeadingEnum FollowUpHeading
		{
			get
			{
				return GetValidateConnectionHeading(m_FollowUpHeading, FollowUp);
			}
			set
			{
				value = GetValidateConnectionHeading(value, FollowUp);
				if (m_FollowUpHeading != value)
				{
					m_FollowUpHeading = value;
					if (CanTouchItsSpline)
					{
						Spline.SetDirty(this, SplineDirtyingType.Everything);
					}
				}
			}
		}

		public bool ConnectionSyncPosition
		{
			get
			{
				return m_ConnectionSyncPosition;
			}
			set
			{
				m_ConnectionSyncPosition = value;
			}
		}

		public bool ConnectionSyncRotation
		{
			get
			{
				return m_ConnectionSyncRotation;
			}
			set
			{
				m_ConnectionSyncRotation = value;
			}
		}

		public CurvyConnection Connection
		{
			get
			{
				return m_Connection;
			}
			internal set
			{
				if (SetConnection(value) && CanTouchItsSpline)
				{
					Spline.SetDirty(this, SplineDirtyingType.Everything);
				}
			}
		}

		public SubArray<Vector3> PositionsApproximation
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return approximations.Positions;
			}
		}

		public SubArray<Vector3> TangentsApproximation
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return approximations.Tangents;
			}
		}

		public SubArray<Vector3> UpsApproximation
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return approximations.Ups;
			}
		}

		public SubArray<float> DistancesApproximation
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return approximations.Distances;
			}
		}

		public int CacheSize => PositionsApproximation.Count - 1;

		public Bounds Bounds
		{
			get
			{
				if (!mBounds.HasValue)
				{
					int count = PositionsApproximation.Count;
					Bounds value;
					if (count == 0)
					{
						value = new Bounds(cachedTransform.position, Vector3.zero);
					}
					else
					{
						Vector3[] array = PositionsApproximation.Array;
						Matrix4x4 localToWorldMatrix = Spline.transform.localToWorldMatrix;
						value = new Bounds(localToWorldMatrix.MultiplyPoint3x4(array[0]), Vector3.zero);
						for (int i = 1; i < count; i++)
						{
							value.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(array[i]));
						}
					}
					mBounds = value;
				}
				return mBounds.Value;
			}
		}

		public float Length
		{
			get
			{
				return length;
			}
			private set
			{
				length = value;
			}
		}

		public float Distance
		{
			get
			{
				return distance;
			}
			internal set
			{
				distance = value;
			}
		}

		public float TF
		{
			get
			{
				return mSpline.SegmentToTF(this);
			}
			[UsedImplicitly]
			[Obsolete("Setting a TF value is not allowed anymore")]
			internal set
			{
				UnityEngine.Debug.LogError("[Curvy] CurvySplineSegment.TF: Setting a TF value is not allowed");
			}
		}

		public bool IsFirstControlPoint => Spline.GetControlPointIndex(this) == 0;

		public bool IsLastControlPoint => Spline.GetControlPointIndex(this) == Spline.ControlPointCount - 1;

		public HashSet<CurvyMetadataBase> Metadata => mMetadata;

		[CanBeNull]
		public CurvySpline Spline => mSpline;

		public bool HasUnprocessedLocalPosition
		{
			get
			{
				if (lastProcessedLocalPosition.HasValue)
				{
					return !cachedTransform.localPosition.Approximately(lastProcessedLocalPosition.Value);
				}
				return true;
			}
		}

		public bool HasUnprocessedLocalOrientation
		{
			get
			{
				if (lastProcessedLocalRotation.HasValue)
				{
					return cachedTransform.localRotation.DifferentOrientation(lastProcessedLocalRotation.Value);
				}
				return true;
			}
		}

		[UsedImplicitly]
		[Obsolete("Use OrientationInfluencesSpline instead")]
		public bool OrientatinInfluencesSpline => OrientationInfluencesSpline;

		public bool OrientationInfluencesSpline
		{
			get
			{
				if (mSpline != null)
				{
					if (mSpline.Orientation != CurvyOrientation.Static)
					{
						return mSpline.IsControlPointAnOrientationAnchor(this);
					}
					return true;
				}
				return false;
			}
		}

		private CurvyInterpolation Interpolation
		{
			get
			{
				if (!Spline)
				{
					return CurvyInterpolation.Linear;
				}
				return Spline.Interpolation;
			}
		}

		private bool IsDynamicOrientation
		{
			get
			{
				if ((bool)Spline)
				{
					return Spline.Orientation == CurvyOrientation.Dynamic;
				}
				return false;
			}
		}

		private bool IsOrientationAnchorEditable
		{
			get
			{
				CurvySpline spline = Spline;
				if (IsDynamicOrientation && spline.IsControlPointVisible(this) && spline.FirstVisibleControlPoint != this)
				{
					return spline.LastVisibleControlPoint != this;
				}
				return false;
			}
		}

		private bool CanHaveSwirl
		{
			get
			{
				CurvySpline spline = Spline;
				if (IsDynamicOrientation && (bool)spline && spline.IsControlPointAnOrientationAnchor(this))
				{
					if (!spline.Closed)
					{
						return spline.LastVisibleControlPoint != this;
					}
					return true;
				}
				return false;
			}
		}

		private SubArray<Vector3> BSplineP0Array
		{
			get
			{
				if (bSplineP0Array.Count != mSpline.BSplineDegree + 1)
				{
					ArrayPool<Vector3> vector = ArrayPools.Vector3;
					if (bSplineP0Array.Count > 0)
					{
						vector.Free(bSplineP0Array);
					}
					bSplineP0Array = vector.Allocate(Spline.BSplineDegree + 1, clearArray: false);
				}
				return bSplineP0Array;
			}
		}

		private bool CanTouchItsSpline
		{
			get
			{
				if (base.IsActiveAndEnabled)
				{
					return mSpline != null;
				}
				return false;
			}
		}

		public void SetBezierHandleIn(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
		{
			if (space == Space.Self)
			{
				HandleIn = position;
			}
			else
			{
				HandleInPosition = position;
			}
			bool flag = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
			bool flag2 = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
			bool flag3 = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;
			if (flag)
			{
				HandleOut = HandleOut.magnitude * (HandleIn.normalized * -1f);
			}
			if (flag2)
			{
				HandleOut = HandleIn.magnitude * ((HandleOut == Vector3.zero) ? (HandleIn.normalized * -1f) : HandleOut.normalized);
			}
			if (!((bool)Connection && flag3) || !(flag || flag2))
			{
				return;
			}
			ReadOnlyCollection<CurvySplineSegment> controlPointsList = Connection.ControlPointsList;
			for (int i = 0; i < controlPointsList.Count; i++)
			{
				CurvySplineSegment curvySplineSegment = controlPointsList[i];
				if (!(curvySplineSegment == this))
				{
					if (curvySplineSegment.HandleIn.magnitude == 0f)
					{
						curvySplineSegment.HandleIn = HandleIn;
					}
					if (flag)
					{
						curvySplineSegment.SetBezierHandleIn(curvySplineSegment.HandleIn.magnitude * HandleIn.normalized * Mathf.Sign(Vector3.Dot(HandleIn, curvySplineSegment.HandleIn)), Space.Self, CurvyBezierModeEnum.Direction);
					}
					if (flag2)
					{
						curvySplineSegment.SetBezierHandleIn(curvySplineSegment.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
					}
				}
			}
		}

		public void SetBezierHandleOut(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
		{
			if (space == Space.Self)
			{
				HandleOut = position;
			}
			else
			{
				HandleOutPosition = position;
			}
			bool flag = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
			bool flag2 = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
			bool flag3 = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;
			if (flag)
			{
				HandleIn = HandleIn.magnitude * (HandleOut.normalized * -1f);
			}
			if (flag2)
			{
				HandleIn = HandleOut.magnitude * ((HandleIn == Vector3.zero) ? (HandleOut.normalized * -1f) : HandleIn.normalized);
			}
			if (!((bool)Connection && flag3) || !(flag || flag2))
			{
				return;
			}
			for (int i = 0; i < Connection.ControlPointsList.Count; i++)
			{
				CurvySplineSegment curvySplineSegment = Connection.ControlPointsList[i];
				if (!(curvySplineSegment == this))
				{
					if (curvySplineSegment.HandleOut.magnitude == 0f)
					{
						curvySplineSegment.HandleOut = HandleOut;
					}
					if (flag)
					{
						curvySplineSegment.SetBezierHandleOut(curvySplineSegment.HandleOut.magnitude * HandleOut.normalized * Mathf.Sign(Vector3.Dot(HandleOut, curvySplineSegment.HandleOut)), Space.Self, CurvyBezierModeEnum.Direction);
					}
					if (flag2)
					{
						curvySplineSegment.SetBezierHandleOut(curvySplineSegment.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
					}
				}
			}
		}

		public void SetBezierHandles(float distanceFrag = -1f, bool setIn = true, bool setOut = true, bool noDirtying = false)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (distanceFrag == -1f)
			{
				distanceFrag = AutoHandleDistance;
			}
			if (distanceFrag > 0f)
			{
				CurvySpline spline = Spline;
				CurvySplineSegment nextControlPoint = spline.GetNextControlPoint(this);
				Transform transform = (nextControlPoint ? nextControlPoint.transform : cachedTransform);
				CurvySplineSegment previousControlPoint = spline.GetPreviousControlPoint(this);
				Transform obj = (previousControlPoint ? previousControlPoint.transform : cachedTransform);
				Vector3 localPosition = cachedTransform.localPosition;
				Vector3 p = obj.localPosition - localPosition;
				Vector3 n = transform.localPosition - localPosition;
				SetBezierHandles(distanceFrag, p, n, setIn, setOut, noDirtying);
				return;
			}
			if (setIn)
			{
				if (noDirtying)
				{
					m_HandleIn = zero;
				}
				else
				{
					HandleIn = zero;
				}
			}
			if (setOut)
			{
				if (noDirtying)
				{
					m_HandleOut = zero2;
				}
				else
				{
					HandleOut = zero2;
				}
			}
		}

		public void SetBezierHandles(float distanceFrag, Vector3 p, Vector3 n, bool setIn = true, bool setOut = true, bool noDirtying = false)
		{
			float magnitude = p.magnitude;
			float magnitude2 = n.magnitude;
			Vector3 handleIn = Vector3.zero;
			Vector3 handleOut = Vector3.zero;
			if (magnitude != 0f || magnitude2 != 0f)
			{
				Vector3 normalized = (magnitude / magnitude2 * n - p).normalized;
				handleIn = -normalized * (magnitude * distanceFrag);
				handleOut = normalized * (magnitude2 * distanceFrag);
			}
			if (setIn)
			{
				if (noDirtying)
				{
					m_HandleIn = handleIn;
				}
				else
				{
					HandleIn = handleIn;
				}
			}
			if (setOut)
			{
				if (noDirtying)
				{
					m_HandleOut = handleOut;
				}
				else
				{
					HandleOut = handleOut;
				}
			}
		}

		public void SetFollowUp(CurvySplineSegment target, ConnectionHeadingEnum heading = ConnectionHeadingEnum.Auto)
		{
			if (target == null)
			{
				FollowUp = target;
				FollowUpHeading = heading;
			}
			else if (Spline.CanControlPointHaveFollowUp(this))
			{
				if (Connection == null || Connection != target.Connection)
				{
					DTLog.LogError("[Curvy] Trying to set as a Follow-Up a Control Point that is not part of the same connection", this);
					return;
				}
				FollowUp = target;
				FollowUpHeading = heading;
			}
			else
			{
				DTLog.LogError("[Curvy] Setting a Follow-Up to a Control Point that can't have one", this);
			}
		}

		public void ResetConnectionUnrelatedProperties()
		{
			m_AutoBakeOrientation = false;
			m_OrientationAnchor = false;
			m_Swirl = CurvyOrientationSwirl.None;
			m_SwirlTurns = 0f;
			m_AutoHandles = true;
			m_AutoHandleDistance = 0.39f;
			m_HandleIn = CurvySplineSegmentDefaultValues.HandleIn;
			m_HandleOut = CurvySplineSegmentDefaultValues.HandleOut;
			m_OverrideGlobalTension = false;
			m_OverrideGlobalContinuity = false;
			m_OverrideGlobalBias = false;
			m_SynchronizeTCB = true;
			m_StartTension = 0f;
			m_EndTension = 0f;
			m_StartContinuity = 0f;
			m_EndContinuity = 0f;
			m_StartBias = 0f;
			m_EndBias = 0f;
			if (CanTouchItsSpline)
			{
				Spline.SetDirty(this, SplineDirtyingType.Everything);
			}
		}

		public void Disconnect()
		{
			Disconnect(destroyEmptyConnection: true);
		}

		public void Disconnect(bool destroyEmptyConnection)
		{
			if ((bool)Connection)
			{
				Connection.RemoveControlPoint(this, destroyEmptyConnection);
			}
			FollowUp = null;
			FollowUpHeading = ConnectionHeadingEnum.Auto;
			ConnectionSyncPosition = false;
			ConnectionSyncRotation = false;
		}

		public Vector3 Interpolate(float localF, Space space = Space.Self)
		{
			CurvySpline spline = Spline;
			CurvyInterpolation interpolation = spline.Interpolation;
			localF = Mathf.Clamp01(localF);
			Vector3 vector;
			switch (interpolation)
			{
			case CurvyInterpolation.BSpline:
				vector = BSpline(spline.ControlPointsList, spline.SegmentToTF(this, localF), spline.IsBSplineClamped, spline.Closed, spline.BSplineDegree, BSplineP0Array.Array);
				break;
			case CurvyInterpolation.Bezier:
				vector = CurvySpline.Bezier(threadSafeData.ThreadSafeLocalPosition.Addition(HandleOut), threadSafeData.ThreadSafeLocalPosition, threadSafeData.ThreadSafeNextCpLocalPosition, threadSafeData.ThreadSafeNextCpLocalPosition.Addition(cachedNextControlPoint.HandleIn), localF);
				break;
			case CurvyInterpolation.TCB:
			{
				TcbParameters effectiveTcbParameters = EffectiveTcbParameters;
				vector = CurvySpline.TCB(threadSafeData.ThreadSafePreviousCpLocalPosition, threadSafeData.ThreadSafeLocalPosition, threadSafeData.ThreadSafeNextCpLocalPosition, cachedNextControlPoint.threadSafeData.ThreadSafeNextCpLocalPosition, localF, effectiveTcbParameters.StartTension, effectiveTcbParameters.StartContinuity, effectiveTcbParameters.StartBias, effectiveTcbParameters.EndTension, effectiveTcbParameters.EndContinuity, effectiveTcbParameters.EndBias);
				break;
			}
			case CurvyInterpolation.CatmullRom:
				vector = CurvySpline.CatmullRom(threadSafeData.ThreadSafePreviousCpLocalPosition, threadSafeData.ThreadSafeLocalPosition, threadSafeData.ThreadSafeNextCpLocalPosition, cachedNextControlPoint.threadSafeData.ThreadSafeNextCpLocalPosition, localF);
				break;
			case CurvyInterpolation.Linear:
				vector = threadSafeData.ThreadSafeLocalPosition.LerpUnclamped(threadSafeData.ThreadSafeNextCpLocalPosition, localF);
				break;
			default:
				DTLog.LogError("[Curvy] Invalid interpolation value " + interpolation, this);
				return Vector3.zero;
			}
			if (space == Space.World)
			{
				vector = spline.ToWorldPosition(vector);
			}
			return vector;
		}

		public Vector3 InterpolateFast(float localF, Space space = Space.Self)
		{
			SubArray<Vector3> positionsApproximation = PositionsApproximation;
			Vector3 vector;
			if (positionsApproximation.Count > 1)
			{
				float frag;
				int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
				vector = positionsApproximation.Array[approximationIndexINTERNAL].LerpUnclamped(positionsApproximation.Array[approximationIndexINTERNAL + 1], frag);
			}
			else
			{
				vector = positionsApproximation.Array[0];
			}
			if (space == Space.World)
			{
				vector = Spline.ToWorldPosition(vector);
			}
			return vector;
		}

		public Vector3 GetTangent(float localF, Space space = Space.Self)
		{
			localF = Mathf.Clamp01(localF);
			Vector3 position = Interpolate(localF, space);
			return GetTangent(localF, position, space);
		}

		public Vector3 GetTangent(float localF, Vector3 position, Space space = Space.Self)
		{
			CurvySpline spline = Spline;
			int num = 2;
			Vector3 vector;
			do
			{
				float num2 = localF + 0.01f;
				if (num2 > 1f)
				{
					CurvySplineSegment nextSegment = spline.GetNextSegment(this);
					if (!nextSegment)
					{
						num2 = localF - 0.01f;
						return OptimizedOperators.Normalize(position.Subtraction(Interpolate(num2, space)));
					}
					vector = nextSegment.Interpolate(num2 - 1f, space);
				}
				else
				{
					vector = Interpolate(num2, space);
				}
				localF += 0.01f;
			}
			while (vector == position && --num > 0);
			return OptimizedOperators.Normalize(vector.Subtraction(position));
		}

		public Vector3 GetTangentFast(float localF, Space space = Space.Self)
		{
			SubArray<Vector3> tangentsApproximation = TangentsApproximation;
			Vector3 vector;
			if (tangentsApproximation.Count > 1)
			{
				float frag;
				int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
				vector = Vector3.SlerpUnclamped(tangentsApproximation.Array[approximationIndexINTERNAL], tangentsApproximation.Array[approximationIndexINTERNAL + 1], frag);
			}
			else
			{
				vector = tangentsApproximation.Array[0];
			}
			if (space == Space.World)
			{
				vector = Spline.ToWorldDirection(vector);
			}
			return vector;
		}

		public void InterpolateAndGetTangent(float localF, out Vector3 position, out Vector3 tangent, Space space = Space.Self)
		{
			localF = Mathf.Clamp01(localF);
			position = Interpolate(localF, space);
			tangent = GetTangent(localF, position, space);
		}

		public void InterpolateAndGetTangentFast(float localF, out Vector3 position, out Vector3 tangent, Space space = Space.Self)
		{
			SubArray<Vector3> tangentsApproximation = TangentsApproximation;
			SubArray<Vector3> positionsApproximation = PositionsApproximation;
			if (positionsApproximation.Count > 1)
			{
				float frag;
				int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
				int num = approximationIndexINTERNAL + 1;
				position = positionsApproximation.Array[approximationIndexINTERNAL].LerpUnclamped(positionsApproximation.Array[num], frag);
				tangent = Vector3.SlerpUnclamped(tangentsApproximation.Array[approximationIndexINTERNAL], tangentsApproximation.Array[num], frag);
			}
			else
			{
				position = positionsApproximation.Array[0];
				tangent = tangentsApproximation.Array[0];
			}
			if (space == Space.World)
			{
				position = Spline.ToWorldPosition(position);
				tangent = Spline.ToWorldDirection(tangent);
			}
		}

		public Vector3 GetOrientationUpFast(float localF, Space space = Space.Self)
		{
			SubArray<Vector3> upsApproximation = UpsApproximation;
			Vector3 vector;
			if (upsApproximation.Count > 1)
			{
				float frag;
				int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
				vector = Vector3.SlerpUnclamped(upsApproximation.Array[approximationIndexINTERNAL], upsApproximation.Array[approximationIndexINTERNAL + 1], frag);
			}
			else
			{
				vector = upsApproximation.Array[0];
			}
			if (space == Space.World)
			{
				vector = Spline.ToWorldDirection(vector);
			}
			return vector;
		}

		public Quaternion GetOrientationFast(float localF, bool inverse = false, Space space = Space.Self)
		{
			Vector3 tangentFast = GetTangentFast(localF, space);
			if (tangentFast != Vector3.zero)
			{
				if (inverse)
				{
					tangentFast *= -1f;
				}
				return Quaternion.LookRotation(tangentFast, GetOrientationUpFast(localF, space));
			}
			return Quaternion.identity;
		}

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public void ReloadMetaData()
		{
			Metadata.Clear();
			CurvyMetadataBase[] components = GetComponents<CurvyMetadataBase>();
			foreach (CurvyMetadataBase item in components)
			{
				Metadata.Add(item);
			}
			CheckAgainstMetaDataDuplication();
		}

		public void RegisterMetaData(CurvyMetadataBase metaData)
		{
			Metadata.Add(metaData);
			CheckAgainstMetaDataDuplication();
		}

		public void UnregisterMetaData(CurvyMetadataBase metaData)
		{
			Metadata.Remove(metaData);
		}

		public T GetMetadata<T>(bool autoCreate = false) where T : CurvyMetadataBase
		{
			Type typeFromHandle = typeof(T);
			T val = null;
			foreach (CurvyMetadataBase metadatum in Metadata)
			{
				if (metadatum != null && metadatum.GetType() == typeFromHandle)
				{
					val = (T)metadatum;
					break;
				}
			}
			if (autoCreate && val == null)
			{
				val = base.gameObject.AddComponent<T>();
				Metadata.Add(val);
			}
			return val;
		}

		public U GetInterpolatedMetadata<T, U>(float f) where T : CurvyInterpolatableMetadataBase<U>
		{
			T metadata = GetMetadata<T>();
			if (metadata != null)
			{
				CurvySplineSegment nextControlPointUsingFollowUp = Spline.GetNextControlPointUsingFollowUp(this);
				CurvyInterpolatableMetadataBase<U> nextMetadata = null;
				if ((bool)nextControlPointUsingFollowUp)
				{
					nextMetadata = nextControlPointUsingFollowUp.GetMetadata<T>();
				}
				return metadata.Interpolate(nextMetadata, f);
			}
			return default(U);
		}

		[UsedImplicitly]
		[Obsolete]
		public void DeleteMetadata()
		{
			List<CurvyMetadataBase> list = Metadata.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].Destroy(isUndoable: true, doPrefabCheck: false);
			}
		}

		public float GetNearestPointF(Vector3 position, Space space = Space.Self)
		{
			if (space == Space.World)
			{
				position = Spline.ToLocalPosition(position);
			}
			SubArray<Vector3> positionsApproximation = PositionsApproximation;
			CurvyUtility.GetNearestPointIndex(position, positionsApproximation.Array, positionsApproximation.Count, out var index, out var fragement);
			return ((float)index + fragement) / (float)(positionsApproximation.Count - 1);
		}

		public float DistanceToLocalF(float localDistance)
		{
			SubArray<float> distancesApproximation = DistancesApproximation;
			float[] array = distancesApproximation.Array;
			int count = distancesApproximation.Count;
			if (count <= 1 || localDistance == 0f)
			{
				return 0f;
			}
			int num = CurvyUtility.InterpolationSearch(array, count, localDistance);
			if (num == count - 1)
			{
				return 1f;
			}
			float num2 = (localDistance - array[num]) / (array[num + 1] - array[num]);
			return ((float)num + num2) / (float)(count - 1);
		}

		public float LocalFToDistance(float localF)
		{
			SubArray<float> distancesApproximation = DistancesApproximation;
			if (distancesApproximation.Count <= 1 || localF == 0f)
			{
				return 0f;
			}
			if (localF == 1f)
			{
				return Length;
			}
			float frag;
			int approximationIndexINTERNAL = getApproximationIndexINTERNAL(localF, out frag);
			float num = distancesApproximation.Array[approximationIndexINTERNAL + 1] - distancesApproximation.Array[approximationIndexINTERNAL];
			return distancesApproximation.Array[approximationIndexINTERNAL] + num * frag;
		}

		public float LocalFToTF(float localF)
		{
			return Spline.SegmentToTF(this, localF);
		}

		public override string ToString()
		{
			if (Spline != null)
			{
				return Spline.name + "." + base.name;
			}
			return base.ToString();
		}

		public void BakeOrientationToTransform()
		{
			Quaternion orientationFast = GetOrientationFast(0f);
			if (cachedTransform.localRotation.DifferentOrientation(orientationFast))
			{
				SetLocalRotation(orientationFast);
			}
		}

		public int getApproximationIndexINTERNAL(float localF, out float frag)
		{
			int count = PositionsApproximation.Count;
			float num = localF * (float)(count - 1);
			int num2 = (int)num;
			int num3 = ((num2 > 0) ? ((num2 >= count - 2) ? (count - 2) : num2) : 0);
			float num4 = num - (float)num3;
			frag = ((num4 <= 0f) ? 0f : ((num4 >= 1f) ? 1f : num4));
			return num3;
		}

		public void LinkToSpline(CurvySpline spline)
		{
			mSpline = spline;
		}

		[UsedImplicitly]
		[Obsolete("Use the other overload instead")]
		public void UnlinkFromSpline()
		{
			mSpline = null;
		}

		public void UnlinkFromSpline(CurvySpline spline)
		{
			if (mSpline == spline)
			{
				mSpline = null;
			}
		}

		public void SetLocalPosition(Vector3 newPosition)
		{
			if (cachedTransform.localPosition != newPosition)
			{
				cachedTransform.localPosition = newPosition;
				Spline.SetDirtyPartial(this, SplineDirtyingType.Everything);
				if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
				{
					Connection.SetSynchronisationPositionAndRotation(ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position, ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
				}
			}
		}

		public void SetPosition(Vector3 value)
		{
			if (cachedTransform.position != value)
			{
				cachedTransform.position = value;
				Spline.SetDirtyPartial(this, SplineDirtyingType.Everything);
				if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
				{
					Connection.SetSynchronisationPositionAndRotation(ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position, ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
				}
			}
		}

		public void SetLocalRotation(Quaternion value)
		{
			if (cachedTransform.localRotation != value)
			{
				cachedTransform.localRotation = value;
				if (OrientationInfluencesSpline)
				{
					Spline.SetDirtyPartial(this, SplineDirtyingType.OrientationOnly);
				}
				if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
				{
					Connection.SetSynchronisationPositionAndRotation(ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position, ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
				}
			}
		}

		public void SetRotation(Quaternion value)
		{
			if (cachedTransform.rotation != value)
			{
				cachedTransform.rotation = value;
				if (OrientationInfluencesSpline)
				{
					Spline.SetDirtyPartial(this, SplineDirtyingType.OrientationOnly);
				}
				if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
				{
					Connection.SetSynchronisationPositionAndRotation(ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position, ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
				}
			}
		}

		public static bool CanFollowUpHeadToStart([NotNull] CurvySplineSegment followUp)
		{
			return followUp.Spline.GetPreviousControlPointIndex(followUp) != -1;
		}

		public static bool CanFollowUpHeadToEnd([NotNull] CurvySplineSegment followUp)
		{
			return followUp.Spline.GetNextControlPointIndex(followUp) != -1;
		}

		public static Vector3 BSpline([NotNull] ReadOnlyCollection<CurvySplineSegment> controlPoints, float tf, bool isClamped, bool isClosed, int degree, [NotNull] Vector3[] p0Array)
		{
			int count = controlPoints.Count;
			int bSplineN = BSplineHelper.GetBSplineN(count, degree, isClosed);
			BSplineHelper.GetBSplineUAndK(tf, isClamped, degree, bSplineN, out var u, out var k);
			GetBSplineP0s(controlPoints, count, degree, k, p0Array);
			if (!isClamped)
			{
				return BSplineHelper.DeBoorUnclamped(degree, k, u, p0Array);
			}
			return BSplineHelper.DeBoorClamped(degree, k, u, bSplineN + 1, p0Array);
		}

		public void OnBeforePush()
		{
			this.StripComponents();
			Disconnect();
			DeleteMetadata();
			base.transform.DeleteChildren(isUndoable: false, doPrefabCheck: true);
		}

		public void OnAfterPop()
		{
			ResetConnectionUnrelatedProperties();
		}

		[UsedImplicitly]
		private void Awake()
		{
			cachedTransform = base.transform;
			DoInitialValidations();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			DoInitialValidations();
			if (CanTouchItsSpline)
			{
				Spline.SetDirtyAll(SplineDirtyingType.Everything, Connection != null);
			}
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			Disconnect();
			ArrayPools.Vector3.Free(bSplineP0Array);
			approximations.Clear();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			AutoHandles = m_AutoHandles;
			Connection = m_Connection;
			if (CanTouchItsSpline)
			{
				Spline.SetDirty(this, SplineDirtyingType.Everything);
			}
		}

		[UsedImplicitly]
		[Obsolete("Use ResetConnectionUnrelatedProperties instead")]
		public new void Reset()
		{
			ResetConnectionUnrelatedProperties();
			base.Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void GetBSplineP0s([NotNull] ReadOnlyCollection<CurvySplineSegment> controlPoints, int controlPointsCount, int degree, int k, [NotNull] Vector3[] pArray)
		{
			for (int i = 0; i <= degree; i++)
			{
				int num = i + k - degree;
				pArray[i] = controlPoints[(num < controlPointsCount) ? num : (num - controlPointsCount)].threadSafeData.ThreadSafeLocalPosition;
			}
		}

		internal void SetExtrinsicPropertiesINTERNAL(ControlPointExtrinsicProperties value)
		{
			extrinsicProperties = value;
		}

		internal ref readonly ControlPointExtrinsicProperties GetExtrinsicPropertiesINTERNAL()
		{
			return ref extrinsicProperties;
		}

		private void DoInitialValidations()
		{
			if ((bool)Connection && !Connection.ControlPointsList.Contains(this))
			{
				SetConnection(null);
			}
			ReloadMetaData();
		}

		private void CheckAgainstMetaDataDuplication()
		{
			if (Metadata.Count <= 1)
			{
				return;
			}
			HashSet<Type> hashSet = new HashSet<Type>();
			foreach (CurvyMetadataBase metadatum in Metadata)
			{
				Type type = metadatum.GetType();
				if (hashSet.Contains(type))
				{
					DTLog.LogWarning($"[Curvy] Game object '{ToString()}' has multiple Components of type '{type}'. Control Points should have no more than one Component instance for each MetaData type.", this);
				}
				else
				{
					hashSet.Add(type);
				}
			}
		}

		private bool SetConnection(CurvyConnection newConnection)
		{
			bool result = false;
			if (m_Connection != newConnection)
			{
				result = true;
				m_Connection = newConnection;
			}
			if (m_Connection == null && m_FollowUp != null)
			{
				result = true;
				m_FollowUp = null;
			}
			return result;
		}

		private static ConnectionHeadingEnum GetValidateConnectionHeading(ConnectionHeadingEnum connectionHeading, [CanBeNull] CurvySplineSegment followUp)
		{
			if (followUp == null)
			{
				return connectionHeading;
			}
			if ((connectionHeading == ConnectionHeadingEnum.Minus && !CanFollowUpHeadToStart(followUp)) || (connectionHeading == ConnectionHeadingEnum.Plus && !CanFollowUpHeadToEnd(followUp)))
			{
				return ConnectionHeadingEnum.Auto;
			}
			return connectionHeading;
		}

		private bool SetAutoHandles(bool newValue)
		{
			bool flag = false;
			if ((bool)Connection)
			{
				ReadOnlyCollection<CurvySplineSegment> controlPointsList = Connection.ControlPointsList;
				for (int i = 0; i < controlPointsList.Count; i++)
				{
					CurvySplineSegment curvySplineSegment = controlPointsList[i];
					flag = flag || curvySplineSegment.m_AutoHandles != newValue;
					curvySplineSegment.m_AutoHandles = newValue;
				}
			}
			else
			{
				flag = m_AutoHandles != newValue;
				m_AutoHandles = newValue;
			}
			return flag;
		}

		internal void PrepareThreadCompatibleDataINTERNAL(bool useFollowUp)
		{
			if (threadSafeData == null)
			{
				threadSafeData = new ThreadSafeData();
			}
			threadSafeData.Set(useFollowUp, this, out var nextCP);
			cachedNextControlPoint = nextCP;
		}

		internal void refreshCurveINTERNAL()
		{
			CurvySpline spline = Spline;
			Approximations approximations = this.approximations;
			Vector3 threadSafeLocalPosition = threadSafeData.ThreadSafeLocalPosition;
			Vector3 threadSafeNextCpLocalPosition = threadSafeData.ThreadSafeNextCpLocalPosition;
			float num;
			if (spline.IsControlPointASegment(this))
			{
				int elementCount = GetSegmentCacheSize() + 1;
				CurvySplineSegment curvySplineSegment = cachedNextControlPoint;
				switch (spline.Interpolation)
				{
				case CurvyInterpolation.BSpline:
					ApproximationsSetter.SetPositionsToBSpline(approximations, elementCount, BSplineP0Array, new BSplineApproximationParameters(this));
					break;
				case CurvyInterpolation.Bezier:
					ApproximationsSetter.SetPositionsToBezier(approximations, elementCount, threadSafeLocalPosition, threadSafeLocalPosition + HandleOut, threadSafeNextCpLocalPosition, threadSafeNextCpLocalPosition + curvySplineSegment.HandleIn);
					break;
				case CurvyInterpolation.CatmullRom:
					ApproximationsSetter.SetPositionsToCatmullRom(approximations, elementCount, threadSafeLocalPosition, threadSafeNextCpLocalPosition, threadSafeData.ThreadSafePreviousCpLocalPosition, curvySplineSegment.threadSafeData.ThreadSafeNextCpLocalPosition);
					break;
				case CurvyInterpolation.TCB:
					ApproximationsSetter.SetPositionsToTCB(approximations, elementCount, EffectiveTcbParameters, threadSafeLocalPosition, threadSafeNextCpLocalPosition, threadSafeData.ThreadSafePreviousCpLocalPosition, curvySplineSegment.threadSafeData.ThreadSafeNextCpLocalPosition);
					break;
				case CurvyInterpolation.Linear:
					ApproximationsSetter.SetPositionsToLinear(approximations, elementCount, threadSafeLocalPosition, threadSafeNextCpLocalPosition);
					break;
				default:
					throw new ArgumentOutOfRangeException("Interpolation");
				}
				num = ApproximationsSetter.SetSegmentTangentsAnDistances(approximations, elementCount);
			}
			else
			{
				ApproximationsSetter.SetPositionsToPoint(approximations, threadSafeLocalPosition);
				num = ApproximationsSetter.SetPointTangentsAndDistances(approximations, threadSafeData.ThreadSafePreviousCpLocalPosition, threadSafeLocalPosition, threadSafeNextCpLocalPosition, threadSafeData.ThreadSafeLocalRotation);
			}
			Length = num;
			ClearBoundsINTERNAL();
			UpdateLasProcessedLocalPosition();
		}

		private int GetSegmentCacheSize()
		{
			return CurvySpline.CalculateCacheSize(Spline.CacheDensity, threadSafeData.ThreadSafeNextCpLocalPosition.Subtraction(threadSafeData.ThreadSafeLocalPosition).magnitude, Spline.MaxPointsPerUnit);
		}

		internal void refreshOrientationNoneINTERNAL()
		{
			ApproximationsSetter.SetOrientationToNone(approximations, CacheSize + 1);
			UpdateLasProcessedLocalRotation();
		}

		internal void refreshOrientationStaticINTERNAL()
		{
			ApproximationsSetter.SetOrientationToStatic(approximations, CacheSize + 1, getOrthoUp0INTERNAL(), getOrthoUp1INTERNAL());
			UpdateLasProcessedLocalRotation();
		}

		internal void refreshOrientationDynamicINTERNAL(Vector3 initialUp)
		{
			ApproximationsSetter.SetOrientationToDynamic(approximations, CacheSize + 1, initialUp);
			UpdateLasProcessedLocalRotation();
		}

		[UsedImplicitly]
		private void UpdateLasProcessedLocalPosition()
		{
			lastProcessedLocalPosition = threadSafeData.ThreadSafeLocalPosition;
		}

		[UsedImplicitly]
		private void UpdateLasProcessedLocalRotation()
		{
			lastProcessedLocalRotation = threadSafeData.ThreadSafeLocalRotation;
		}

		internal void ClearBoundsINTERNAL()
		{
			mBounds = null;
		}

		internal Vector3 getOrthoUp0INTERNAL()
		{
			Vector3 tangent = threadSafeData.ThreadSafeLocalRotation * Vector3.up;
			Vector3.OrthoNormalize(ref TangentsApproximation.Array[0], ref tangent);
			return tangent;
		}

		private Vector3 getOrthoUp1INTERNAL()
		{
			CurvySplineSegment nextControlPoint = Spline.GetNextControlPoint(this);
			Vector3 tangent = (nextControlPoint ? nextControlPoint.threadSafeData.ThreadSafeLocalRotation : threadSafeData.ThreadSafeLocalRotation) * Vector3.up;
			Vector3.OrthoNormalize(ref TangentsApproximation.Array[CacheSize], ref tangent);
			return tangent;
		}

		internal void UnsetFollowUpWithoutDirtyingINTERNAL()
		{
			m_FollowUp = null;
			m_FollowUpHeading = ConnectionHeadingEnum.Auto;
		}

		[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS")]
		private void DoSanityChecks()
		{
			if (Spline == null)
			{
				DTLog.LogError("[Curvy] Calling public method on an orphan segment.", this);
			}
			if (!Spline.IsInitialized)
			{
				DTLog.LogError("[Curvy] Calling public method on non initialized spline.", Spline);
			}
			if (Spline.Dirty)
			{
				DTLog.LogWarning(string.Format(CultureInfo.InvariantCulture, "[Curvy] Calling public method on a dirty spline. The returned result will not be up to date. Either refresh the spline manually by calling Refresh(), or wait for it to be refreshed automatically at the next {0} call", Spline.UpdateIn.ToString()), Spline);
			}
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		[UsedImplicitly]
		private void UpdateSelectionIfNeeded()
		{
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private static void ForceHierarchyDrawing()
		{
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			lastProcessedLocalPosition = null;
			lastProcessedLocalRotation = null;
			mBounds = null;
			threadSafeData = null;
			distance = (length = -1f);
			approximations.Clear();
		}
	}
}
