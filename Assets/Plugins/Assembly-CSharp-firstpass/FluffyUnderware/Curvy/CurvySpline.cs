using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy
{
	[HelpURL("https://curvyeditor.com/doclink/curvyspline")]
	[AddComponentMenu("Curvy/Curvy Spline")]
	[ExecuteAlways]
	public class CurvySpline : DTVersionedMonoBehaviour
	{
		private class ControlPointsSynchronizer
		{
			public enum SynchronizationRequest
			{
				None = 0,
				SplineToHierarchy = 1,
				HierarchyToSpline = 2
			}

			[NotNull]
			private readonly CurvySpline spline;

			private bool processing;

			public SynchronizationRequest CurrentRequest { get; private set; }

			public ControlPointsSynchronizer([NotNull] CurvySpline spline)
			{
				this.spline = spline;
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void RequestSplineToHierarchy()
			{
				if (!processing && CurrentRequest != SynchronizationRequest.HierarchyToSpline)
				{
					CurrentRequest = SynchronizationRequest.SplineToHierarchy;
				}
			}

			public void RequestHierarchyToSpline()
			{
				if (!processing && CurrentRequest != SynchronizationRequest.SplineToHierarchy)
				{
					CurrentRequest = SynchronizationRequest.HierarchyToSpline;
				}
			}

			public void ProcessRequests()
			{
				processing = true;
				try
				{
					switch (CurrentRequest)
					{
					case SynchronizationRequest.HierarchyToSpline:
						SynchronizeHierarchyToSpline();
						break;
					default:
						throw new ArgumentOutOfRangeException();
					case SynchronizationRequest.None:
					case SynchronizationRequest.SplineToHierarchy:
						break;
					}
				}
				finally
				{
					processing = false;
				}
				CurrentRequest = SynchronizationRequest.None;
			}

			public void CancelRequests()
			{
				CurrentRequest = SynchronizationRequest.None;
			}

			private void SynchronizeHierarchyToSpline()
			{
				spline.ClearControlPoints(invalidateAndDirty: true, requestSplineToHierarchySynchronization: false);
				Transform transform = spline.transform;
				for (int i = 0; i < transform.childCount; i++)
				{
					CurvySplineSegment component = transform.GetChild(i).GetComponent<CurvySplineSegment>();
					if (!(component == null))
					{
						spline.AddControlPoint(component, invalidateAndDirty: false, requestSplineToHierarchySynchronization: false);
					}
				}
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			private void SynchronizeSplineToHierarchy()
			{
				for (short num = 0; num < spline.ControlPoints.Count; num++)
				{
					CurvySplineSegment curvySplineSegment = spline.ControlPoints[num];
					if ((bool)curvySplineSegment)
					{
						curvySplineSegment.transform.SetSiblingIndex(num);
					}
				}
			}

			[System.Diagnostics.Conditional("CURVY_DEBUG")]
			private static void DebugLog(string message)
			{
				UnityEngine.Debug.Log(message);
			}

			[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS")]
			private static void LogIgnoredRequest()
			{
				UnityEngine.Debug.LogWarning("Ignored request while processing");
			}

			[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS")]
			private void AssertIsNotProcessing()
			{
			}
		}

		private class DirtinessManager : IDisposable
		{
			private bool dirtyCurve;

			private bool dirtyOrientation;

			private bool allControlPointsAreDirty;

			private readonly HashSet<CurvySplineSegment> dirtyControlPointsMinimalSet = new HashSet<CurvySplineSegment>();

			[NotNull]
			private CurvySpline spline;

			private bool processingDirtyControlPoints;

			private readonly ThreadPoolWorker<CurvySplineSegment> threadWorker = new ThreadPoolWorker<CurvySplineSegment>();

			private readonly List<CurvySplineSegment> persistedSegmentsList = new List<CurvySplineSegment>();

			private readonly OrientationGroup persistedOrientationGroup = new OrientationGroup();

			private readonly Action<CurvySplineSegment, int, int> refreshOrientationStaticAction = delegate(CurvySplineSegment controlPoint, int cpIndex, int cpsCount)
			{
				controlPoint.refreshOrientationStaticINTERNAL();
			};

			private bool DirtyCurve
			{
				get
				{
					return dirtyCurve;
				}
				set
				{
					dirtyCurve = value;
				}
			}

			private bool DirtyOrientation
			{
				get
				{
					return dirtyOrientation;
				}
				set
				{
					dirtyOrientation = value;
				}
			}

			public bool AllControlPointsAreDirty
			{
				get
				{
					return allControlPointsAreDirty;
				}
				private set
				{
					allControlPointsAreDirty = value;
				}
			}

			public bool Dirty
			{
				get
				{
					if (!AllControlPointsAreDirty)
					{
						return dirtyControlPointsMinimalSet.Count > 0;
					}
					return true;
				}
			}

			public DirtinessManager([NotNull] CurvySpline spline)
			{
				this.spline = spline;
				Reset();
			}

			public void SetDirtyAll(SplineDirtyingType dirtyingType, bool dirtyConnectedControlPoints)
			{
				AllControlPointsAreDirty = true;
				SetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(dirtyingType);
				if (!dirtyConnectedControlPoints)
				{
					return;
				}
				for (int i = 0; i < spline.ControlPoints.Count; i++)
				{
					CurvySplineSegment curvySplineSegment = spline.ControlPoints[i];
					if (!curvySplineSegment || !curvySplineSegment.Connection)
					{
						continue;
					}
					ReadOnlyCollection<CurvySplineSegment> controlPointsList = curvySplineSegment.Connection.ControlPointsList;
					for (int j = 0; j < controlPointsList.Count; j++)
					{
						CurvySplineSegment curvySplineSegment2 = controlPointsList[j];
						CurvySpline curvySpline = ((curvySplineSegment2 != null) ? curvySplineSegment2.Spline : null);
						if ((bool)curvySpline && curvySpline != spline)
						{
							curvySpline.dirtinessManager.AddToMinimalSetAndSetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(curvySplineSegment2, dirtyingType);
						}
					}
				}
			}

			public void SetDirty(CurvySplineSegment controlPoint, SplineDirtyingType dirtyingType, CurvySplineSegment previousControlPoint, CurvySplineSegment nextControlPoint, bool ignoreConnectionOfInputControlPoint)
			{
				if ((object)spline != controlPoint.Spline)
				{
					throw new ArgumentException($"[Curvy] Method called with a control point '{controlPoint}' that is not part of the current spline '{spline.name}'");
				}
				if (!ignoreConnectionOfInputControlPoint && (bool)controlPoint.Connection)
				{
					ReadOnlyCollection<CurvySplineSegment> controlPointsList = controlPoint.Connection.ControlPointsList;
					for (int i = 0; i < controlPointsList.Count; i++)
					{
						CurvySplineSegment curvySplineSegment = controlPointsList[i];
						CurvySpline curvySpline = curvySplineSegment.Spline;
						if ((bool)curvySpline)
						{
							curvySpline.dirtinessManager.AddToMinimalSetAndSetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(curvySplineSegment, dirtyingType);
						}
					}
				}
				else
				{
					AddToMinimalSetAndSetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(controlPoint, dirtyingType);
				}
				if ((bool)previousControlPoint && (bool)previousControlPoint.Connection)
				{
					ReadOnlyCollection<CurvySplineSegment> controlPointsList2 = previousControlPoint.Connection.ControlPointsList;
					for (int j = 0; j < controlPointsList2.Count; j++)
					{
						CurvySplineSegment curvySplineSegment2 = controlPointsList2[j];
						CurvySpline curvySpline2 = curvySplineSegment2.Spline;
						if ((bool)curvySpline2 && curvySplineSegment2.FollowUp == previousControlPoint)
						{
							curvySpline2.dirtinessManager.AddToMinimalSetAndSetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(curvySplineSegment2, dirtyingType);
						}
					}
				}
				if (!nextControlPoint || !nextControlPoint.Connection)
				{
					return;
				}
				ReadOnlyCollection<CurvySplineSegment> controlPointsList3 = nextControlPoint.Connection.ControlPointsList;
				for (int k = 0; k < controlPointsList3.Count; k++)
				{
					CurvySplineSegment curvySplineSegment3 = controlPointsList3[k];
					CurvySpline curvySpline3 = curvySplineSegment3.Spline;
					if ((bool)curvySpline3 && curvySplineSegment3.FollowUp == nextControlPoint)
					{
						curvySpline3.dirtinessManager.AddToMinimalSetAndSetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(curvySplineSegment3, dirtyingType);
					}
				}
			}

			public void ClearMinimalSet()
			{
				dirtyControlPointsMinimalSet.Clear();
			}

			public void RemoveFromMinimalSet(CurvySplineSegment item)
			{
				dirtyControlPointsMinimalSet.Remove(item);
			}

			[MustUseReturnValue]
			public bool ProcessDirtyControlPoints()
			{
				spline.relationshipCache.EnsureIsValid();
				if (!Dirty)
				{
					return false;
				}
				if (!DirtyOrientation && !DirtyCurve)
				{
					throw new InvalidOperationException("[Curvy] Processing dirty control points while no dirtying flag is set");
				}
				ValidateConnectedSplines();
				processingDirtyControlPoints = true;
				bool result = true;
				try
				{
					if (spline.ControlPointCount != 0)
					{
						List<CurvySplineSegment> dirtyCpsExtendedList = persistedSegmentsList;
						FillDirtyCpsExtendedList(dirtyCpsExtendedList);
						spline.PrepareThreadCompatibleData();
						if (DirtyCurve)
						{
							ProcessDirtyCurve(dirtyCpsExtendedList);
						}
						if (DirtyOrientation)
						{
							ProcessDirtyOrientation(dirtyCpsExtendedList);
						}
					}
				}
				catch (Exception exception)
				{
					DTLog.LogException(exception, spline);
					result = false;
				}
				finally
				{
					processingDirtyControlPoints = false;
					DirtyCurve = false;
					DirtyOrientation = false;
					AllControlPointsAreDirty = false;
					dirtyControlPointsMinimalSet.Clear();
				}
				return result;
			}

			public void Reset()
			{
				DirtyCurve = true;
				DirtyOrientation = true;
				dirtyControlPointsMinimalSet.Clear();
				AllControlPointsAreDirty = true;
				processingDirtyControlPoints = false;
			}

			public void Dispose()
			{
				threadWorker.Dispose();
			}

			private void ProcessDirtyOrientation(List<CurvySplineSegment> dirtyCpsExtendedList)
			{
				if (dirtyCpsExtendedList.Count == 0)
				{
					throw new InvalidOperationException("[Curvy] No dirty control points to process");
				}
				switch (spline.Orientation)
				{
				case CurvyOrientation.None:
				{
					for (int j = 0; j < dirtyCpsExtendedList.Count; j++)
					{
						dirtyCpsExtendedList[j].refreshOrientationNoneINTERNAL();
					}
					break;
				}
				case CurvyOrientation.Static:
				{
					if (spline.UseThreading && FluffyUnderware.DevTools.Environment.IsThreadingSupported)
					{
						threadWorker.ParallelFor(refreshOrientationStaticAction, dirtyCpsExtendedList);
						break;
					}
					for (int i = 0; i < dirtyCpsExtendedList.Count; i++)
					{
						dirtyCpsExtendedList[i].refreshOrientationStaticINTERNAL();
					}
					break;
				}
				case CurvyOrientation.Dynamic:
					ProcessDirtyDynamicOrientation(dirtyCpsExtendedList);
					break;
				default:
					DTLog.LogError("[Curvy] Invalid Orientation value " + spline.Orientation, spline);
					break;
				}
				if (!spline.Closed && spline.Count > 0)
				{
					CurvySplineSegment previousControlPoint = spline.GetPreviousControlPoint(spline.LastVisibleControlPoint);
					spline.LastVisibleControlPoint.UpsApproximation.Array[0] = previousControlPoint.UpsApproximation.Array[previousControlPoint.CacheSize];
				}
			}

			private void ProcessDirtyDynamicOrientation(List<CurvySplineSegment> dirtyCpsExtendedList)
			{
				short[] orientationAnchorIndices = spline.GetOrientationAnchorIndices();
				int num = spline.ControlPointCount + 1;
				do
				{
					CurvySplineSegment curvySplineSegment = dirtyCpsExtendedList[0];
					if (!spline.IsControlPointASegment(curvySplineSegment))
					{
						curvySplineSegment.refreshOrientationDynamicINTERNAL(curvySplineSegment.getOrthoUp0INTERNAL());
						dirtyCpsExtendedList.RemoveAt(0);
						continue;
					}
					persistedOrientationGroup.SetupOrientationGroup(orientationAnchorIndices[curvySplineSegment.GetExtrinsicPropertiesINTERNAL().ControlPointIndex], curvySplineSegment.Spline.ControlPoints, orientationAnchorIndices);
					persistedOrientationGroup.UpdateOrientation();
					for (int i = 0; i < persistedOrientationGroup.Segments.Count; i++)
					{
						dirtyCpsExtendedList.Remove(persistedOrientationGroup.Segments[i]);
					}
				}
				while (dirtyCpsExtendedList.Count > 0 && num-- > 0);
				if (num <= 0)
				{
					DTLog.LogWarning("[Curvy] Deadloop in CurvySpline.Refresh! Please raise a bugreport!", spline);
				}
			}

			private void ProcessDirtyCurve(List<CurvySplineSegment> dirtyCpsExtendedList)
			{
				if (dirtyCpsExtendedList.Count == 0)
				{
					throw new InvalidOperationException("[Curvy] No dirty control points to process");
				}
				if (spline.Interpolation == CurvyInterpolation.Bezier)
				{
					for (int i = 0; i < dirtyCpsExtendedList.Count; i++)
					{
						CurvySplineSegment curvySplineSegment = dirtyCpsExtendedList[i];
						if (curvySplineSegment.AutoHandles)
						{
							curvySplineSegment.SetBezierHandles(-1f, setIn: true, setOut: true, noDirtying: true);
						}
					}
				}
				if (spline.UseThreading && FluffyUnderware.DevTools.Environment.IsThreadingSupported)
				{
					threadWorker.ParallelFor(spline.refreshCurveAction, dirtyCpsExtendedList);
				}
				else
				{
					for (int j = 0; j < dirtyCpsExtendedList.Count; j++)
					{
						dirtyCpsExtendedList[j].refreshCurveINTERNAL();
					}
				}
				if (spline.ControlPointCount > 0)
				{
					spline.UpdateControlPointDistances();
					spline.EnforceTangentContinuity();
				}
			}

			private void SetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(SplineDirtyingType dirtyingType)
			{
				DirtyCurve = DirtyCurve || dirtyingType == SplineDirtyingType.Everything;
				DirtyOrientation = true;
				if (DirtyCurve)
				{
					spline.InvalidateAccumulators();
				}
			}

			private void FillDirtyCpsExtendedList(List<CurvySplineSegment> dirtyCpsExtendedList)
			{
				dirtyCpsExtendedList.Clear();
				if (AllControlPointsAreDirty)
				{
					dirtyCpsExtendedList.AddRange(spline.ControlPoints);
					return;
				}
				int count = dirtyControlPointsMinimalSet.Count;
				for (int i = 0; i < count; i++)
				{
					CurvySplineSegment controlPoint = dirtyControlPointsMinimalSet.ElementAt(i);
					switch (spline.Interpolation)
					{
					case CurvyInterpolation.Linear:
					{
						CurvySplineSegment previousControlPoint = spline.GetPreviousControlPoint(controlPoint);
						if ((bool)previousControlPoint)
						{
							dirtyControlPointsMinimalSet.Add(previousControlPoint);
						}
						break;
					}
					case CurvyInterpolation.CatmullRom:
					case CurvyInterpolation.TCB:
					case CurvyInterpolation.Bezier:
					{
						CurvySplineSegment previousControlPoint2 = spline.GetPreviousControlPoint(controlPoint);
						if ((bool)previousControlPoint2)
						{
							dirtyControlPointsMinimalSet.Add(previousControlPoint2);
						}
						if ((bool)previousControlPoint2)
						{
							CurvySplineSegment previousControlPoint3 = spline.GetPreviousControlPoint(previousControlPoint2);
							if ((bool)previousControlPoint3)
							{
								dirtyControlPointsMinimalSet.Add(previousControlPoint3);
							}
						}
						CurvySplineSegment nextControlPoint = spline.GetNextControlPoint(controlPoint);
						if ((bool)nextControlPoint)
						{
							dirtyControlPointsMinimalSet.Add(nextControlPoint);
						}
						break;
					}
					case CurvyInterpolation.BSpline:
					{
						int count2 = spline.ControlPoints.Count;
						int bSplineDegree = spline.BSplineDegree;
						bool closed = spline.Closed;
						bool isBSplineClamped = spline.IsBSplineClamped;
						int bSplineN = BSplineHelper.GetBSplineN(count2, bSplineDegree, closed);
						int controlPointIndex = spline.GetControlPointIndex(controlPoint);
						for (int j = 0; j < count2; j++)
						{
							CurvySplineSegment curvySplineSegment = spline.ControlPoints[j];
							BSplineHelper.GetBSplineUAndK(spline.SegmentToTF(curvySplineSegment), isBSplineClamped, bSplineDegree, bSplineN, out var u, out var k);
							if (controlPointIndex >= k - bSplineDegree && controlPointIndex <= k)
							{
								dirtyCpsExtendedList.Add(curvySplineSegment);
								continue;
							}
							BSplineHelper.GetBSplineUAndK(spline.SegmentToTF(curvySplineSegment, 1f), isBSplineClamped, bSplineDegree, bSplineN, out u, out var k2);
							if (controlPointIndex >= k2 - bSplineDegree && controlPointIndex <= k2)
							{
								dirtyCpsExtendedList.Add(curvySplineSegment);
							}
							else
							{
								if (!closed)
								{
									continue;
								}
								int num = controlPointIndex + count2;
								if (num >= k - bSplineDegree && num <= k)
								{
									dirtyCpsExtendedList.Add(curvySplineSegment);
									continue;
								}
								BSplineHelper.GetBSplineUAndK(spline.SegmentToTF(curvySplineSegment, 1f), isBSplineClamped, bSplineDegree, bSplineN, out u, out k2);
								if (num >= k2 - bSplineDegree && num <= k2)
								{
									dirtyCpsExtendedList.Add(curvySplineSegment);
								}
							}
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				dirtyCpsExtendedList.AddRange(dirtyControlPointsMinimalSet);
			}

			private void AddToMinimalSetAndSetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(CurvySplineSegment controlPoint, SplineDirtyingType dirtyingType)
			{
				dirtyControlPointsMinimalSet.Add(controlPoint);
				SetDirtyingFlagsAndInvalidateSplineCurveCachesIfNeeded(dirtyingType);
			}

			private void ValidateConnectedSplines()
			{
				List<CurvySplineSegment> list = (from cp in spline.ControlPoints.Where((CurvySplineSegment cp) => cp.Connection != null).SelectMany((CurvySplineSegment cp) => cp.Connection.ControlPointsList)
					where cp.Spline != spline
					select cp).ToList();
				SynchronizeSplinesWithNullCps(list);
				SynchronizeUninitializedSplines(list);
			}

			private void SynchronizeSplinesWithNullCps([NotNull] List<CurvySplineSegment> controlPoints)
			{
				foreach (CurvySpline item in (from cp in controlPoints
					where cp.Spline != null
					select cp.Spline).Distinct())
				{
					if (item.ControlPoints.Exists((CurvySplineSegment cp) => cp == null))
					{
						item.SyncSplineFromHierarchy();
					}
				}
			}

			private static void SynchronizeUninitializedSplines([NotNull] List<CurvySplineSegment> connectedCPs)
			{
				foreach (CurvySplineSegment item in connectedCPs.Where((CurvySplineSegment cp) => cp.Spline == null))
				{
					CurvySpline component = item.transform.parent.GetComponent<CurvySpline>();
					if (component != null)
					{
						component.SyncSplineFromHierarchy();
					}
				}
			}

			[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS")]
			private void DoSanityChecks()
			{
				if (processingDirtyControlPoints)
				{
					throw new InvalidOperationException("[Curvy] Dirtying while processing dirty state is not allowed");
				}
			}
		}

		private class OrientationGroup
		{
			[NotNull]
			[ItemNotNull]
			private readonly List<CurvySplineSegment> segments;

			private SegmentGroupMetrics currentMetrics;

			[NotNull]
			private float[] accumulatedSwirlAngles;

			[NotNull]
			private readonly List<int> accumulatedCacheSizes;

			[NotNull]
			public List<CurvySplineSegment> Segments => segments;

			public OrientationGroup()
			{
				segments = new List<CurvySplineSegment>();
				accumulatedSwirlAngles = Array.Empty<float>();
				accumulatedCacheSizes = new List<int>();
			}

			public void SetupOrientationGroup(short anchorIndex, [NotNull][ItemNotNull] List<CurvySplineSegment> splineControlPoints, [NotNull] short[] orientationAnchorIndices)
			{
				segments.Clear();
				accumulatedCacheSizes.Clear();
				currentMetrics = default(SegmentGroupMetrics);
				short num = anchorIndex;
				do
				{
					CurvySplineSegment curvySplineSegment = splineControlPoints[num];
					segments.Add(curvySplineSegment);
					currentMetrics.Increment(curvySplineSegment);
					accumulatedCacheSizes.Add(currentMetrics.CacheSize);
					num = curvySplineSegment.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex;
				}
				while (orientationAnchorIndices[num] != num);
			}

			public void UpdateOrientation()
			{
				ApplyParallelTransport();
				ApplySwirlAndSmoothing();
			}

			private void ApplySwirlAndSmoothing()
			{
				float orientationGap = GetOrientationGap();
				bool num = orientationGap == 0f;
				CurvySplineSegment curvySplineSegment = segments[0];
				bool flag = curvySplineSegment.Swirl == CurvyOrientationSwirl.None || curvySplineSegment.SwirlTurns == 0f;
				if (num && flag)
				{
					return;
				}
				float[] array = GetAccumulatedSwirlAngles();
				float num2 = orientationGap / (float)currentMetrics.CacheSize;
				for (int i = 0; i < segments.Count; i++)
				{
					CurvySplineSegment curvySplineSegment2 = segments[i];
					Vector3[] array2 = curvySplineSegment2.TangentsApproximation.Array;
					SubArray<Vector3> upsApproximation = curvySplineSegment2.UpsApproximation;
					Vector3[] array3 = upsApproximation.Array;
					int count = upsApproximation.Count;
					int num3;
					float num4;
					if (i == 0)
					{
						num3 = 0;
						num4 = 0f;
					}
					else
					{
						int num5 = i - 1;
						num3 = accumulatedCacheSizes[num5];
						num4 = array[num5];
					}
					float num6 = (array[i] - num4) / (float)curvySplineSegment2.CacheSize;
					for (int j = 0; j < count; j++)
					{
						float angle = num4 + (float)j * num6 + (float)(num3 + j) * num2;
						array3[j] = Quaternion.AngleAxis(angle, array2[j]) * array3[j];
					}
				}
			}

			private void ApplyParallelTransport()
			{
				for (int i = 0; i < segments.Count; i++)
				{
					CurvySplineSegment curvySplineSegment = segments[i];
					Vector3 initialUp;
					if (i == 0)
					{
						initialUp = curvySplineSegment.getOrthoUp0INTERNAL();
					}
					else
					{
						CurvySplineSegment curvySplineSegment2 = segments[i - 1];
						initialUp = curvySplineSegment2.UpsApproximation.Array[curvySplineSegment2.UpsApproximation.Count - 1];
					}
					curvySplineSegment.refreshOrientationDynamicINTERNAL(initialUp);
				}
			}

			private float GetOrientationGap()
			{
				CurvySplineSegment curvySplineSegment = segments[segments.Count - 1];
				CurvySplineSegment curvySplineSegment2 = curvySplineSegment.Spline.ControlPoints[curvySplineSegment.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex];
				Vector3 a = curvySplineSegment.UpsApproximation.Array[curvySplineSegment.UpsApproximation.Count - 1];
				Vector3 orthoUp0INTERNAL = curvySplineSegment2.getOrthoUp0INTERNAL();
				return a.AngleSigned(orthoUp0INTERNAL, curvySplineSegment2.TangentsApproximation.Array[0]);
			}

			[NotNull]
			private float[] GetAccumulatedSwirlAngles()
			{
				if (segments.Count > accumulatedSwirlAngles.Length)
				{
					Array.Resize(ref accumulatedSwirlAngles, segments.Count);
				}
				CurvySplineSegment curvySplineSegment = segments[0];
				float swirlTurns = curvySplineSegment.SwirlTurns;
				SegmentGroupMetrics segmentGroupMetrics = currentMetrics;
				int count = segments.Count;
				switch (curvySplineSegment.Swirl)
				{
				case CurvyOrientationSwirl.Segment:
				{
					float num3 = swirlTurns * 360f;
					for (int j = 0; j < count; j++)
					{
						accumulatedSwirlAngles[j] = num3 * (float)(j + 1);
					}
					break;
				}
				case CurvyOrientationSwirl.AnchorGroup:
				{
					float num4 = swirlTurns * 360f / (float)segmentGroupMetrics.SegmentCount;
					for (int k = 0; k < count; k++)
					{
						accumulatedSwirlAngles[k] = num4 * (float)(k + 1);
					}
					break;
				}
				case CurvyOrientationSwirl.AnchorGroupAbs:
				{
					float num = swirlTurns * 360f / segmentGroupMetrics.Length;
					float num2 = 0f;
					for (int i = 0; i < count; i++)
					{
						num2 += num * segments[i].Length;
						accumulatedSwirlAngles[i] = num2;
					}
					break;
				}
				case CurvyOrientationSwirl.None:
					Array.Clear(accumulatedSwirlAngles, 0, count);
					break;
				default:
					Array.Clear(accumulatedSwirlAngles, 0, count);
					DTLog.LogError($"[Curvy] Invalid Swirl value {curvySplineSegment.Swirl}");
					break;
				}
				return accumulatedSwirlAngles;
			}
		}

		private class RelationshipCache
		{
			[NotNull]
			private readonly CurvySpline spline;

			[NotNull]
			private readonly object lockObject = new object();

			[CanBeNull]
			private CurvySplineSegment firstSegment;

			[CanBeNull]
			private CurvySplineSegment lastSegment;

			[CanBeNull]
			private CurvySplineSegment firstVisibleControlPoint;

			[CanBeNull]
			private CurvySplineSegment lastVisibleControlPoint;

			[CanBeNull]
			public CurvySplineSegment FirstVisibleControlPoint
			{
				get
				{
					EnsureIsValid();
					return firstVisibleControlPoint;
				}
			}

			[CanBeNull]
			public CurvySplineSegment LastVisibleControlPoint
			{
				get
				{
					EnsureIsValid();
					return lastVisibleControlPoint;
				}
			}

			[CanBeNull]
			public CurvySplineSegment FirstSegment
			{
				get
				{
					EnsureIsValid();
					return firstSegment;
				}
			}

			[CanBeNull]
			public CurvySplineSegment LastSegment
			{
				get
				{
					EnsureIsValid();
					return lastSegment;
				}
			}

			public bool IsValid { get; private set; }

			public RelationshipCache([NotNull] CurvySpline spline)
			{
				this.spline = spline;
			}

			public void Invalidate()
			{
				if (!IsValid)
				{
					return;
				}
				lock (lockObject)
				{
					IsValid = false;
					firstSegment = (lastSegment = (firstVisibleControlPoint = (lastVisibleControlPoint = null)));
				}
			}

			public void EnsureIsValid()
			{
				if (!IsValid)
				{
					RebuildAndFixNonCoherentControlPoints();
				}
			}

			private void RebuildAndFixNonCoherentControlPoints()
			{
				bool flag = true;
				lock (lockObject)
				{
					if (IsValid)
					{
						return;
					}
					spline.BSplineDegree = spline.bSplineDegree;
					int count = spline.ControlPoints.Count;
					spline.mSegments.Clear();
					spline.mSegments.Capacity = count;
					if (count > 0)
					{
						CurvySplineSegment curvySplineSegment = null;
						bool flag2 = false;
						CurvySplineSegment curvySplineSegment2 = null;
						CurvySplineSegment.ControlPointExtrinsicProperties extrinsicPropertiesINTERNAL = new CurvySplineSegment.ControlPointExtrinsicProperties(isVisible: false, -1f, -1, -1, -1, -1, previousControlPointIsSegment: false, nextControlPointIsSegment: false, canHaveFollowUp: false);
						bool closed = spline.Closed;
						bool flag3 = spline.Interpolation == CurvyInterpolation.CatmullRom || spline.Interpolation == CurvyInterpolation.TCB;
						bool flag4 = !spline.AutoEndTangents && flag3;
						bool isBSpline = spline.Interpolation == CurvyInterpolation.BSpline;
						float num = (flag4 ? (1f / (float)((count <= 3) ? 1 : (count - 3))) : ((!closed) ? (1f / (float)((count <= 1) ? 1 : (count - 1))) : (1f / (float)count)));
						short num2 = 0;
						for (short num3 = 0; num3 < count; num3++)
						{
							CurvySplineSegment curvySplineSegment3 = spline.ControlPoints[num3];
							short previousControlPointIndex = GetPreviousControlPointIndex(num3, closed, count);
							short nextControlPointIndex = GetNextControlPointIndex(num3, closed, count);
							bool flag5 = IsControlPointASegment(num3, count, closed, flag4, isBSpline, spline.bSplineDegree);
							bool flag6 = flag5 || extrinsicPropertiesINTERNAL.IsSegment;
							bool flag7 = flag6 && (nextControlPointIndex == -1 || previousControlPointIndex == -1);
							float tf = ((!flag4) ? (num * (float)num3) : (num * (float)((num3 != 0) ? ((num3 == count - 1) ? Math.Max(0, num3 - 2) : (num3 - 1)) : 0)));
							extrinsicPropertiesINTERNAL = new CurvySplineSegment.ControlPointExtrinsicProperties(flag6, tf, (short)(flag5 ? num2 : (-1)), num3, previousControlPointIndex, nextControlPointIndex, previousControlPointIndex != -1 && IsControlPointASegment(previousControlPointIndex, count, closed, flag4, isBSpline, spline.bSplineDegree), nextControlPointIndex != -1 && IsControlPointASegment(nextControlPointIndex, count, closed, flag4, isBSpline, spline.bSplineDegree), flag7);
							curvySplineSegment3.SetExtrinsicPropertiesINTERNAL(extrinsicPropertiesINTERNAL);
							if (flag5)
							{
								spline.mSegments.Add(curvySplineSegment3);
								num2++;
								if (!flag2)
								{
									flag2 = true;
									curvySplineSegment = curvySplineSegment3;
								}
								curvySplineSegment2 = curvySplineSegment3;
							}
							if (flag && !flag7)
							{
								curvySplineSegment3.UnsetFollowUpWithoutDirtyingINTERNAL();
							}
						}
						firstSegment = curvySplineSegment;
						lastSegment = curvySplineSegment2;
						firstVisibleControlPoint = firstSegment;
						lastVisibleControlPoint = (((object)lastSegment != null) ? spline.ControlPoints[lastSegment.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex] : null);
					}
					else
					{
						firstSegment = (lastSegment = (firstVisibleControlPoint = (lastVisibleControlPoint = null)));
					}
					IsValid = true;
				}
			}
		}

		private class SanityChecker
		{
			[NotNull]
			private readonly CurvySpline spline;

			private int sanityErrorLogsThisFrame;

			private int sanityWaringLogsThisFrame;

			public SanityChecker([NotNull] CurvySpline spline)
			{
				this.spline = spline;
			}

			[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS")]
			public void OnUpdate()
			{
				sanityWaringLogsThisFrame = 0;
				sanityErrorLogsThisFrame = 0;
			}

			[System.Diagnostics.Conditional("CURVY_SANITY_CHECKS")]
			public void Check()
			{
				if (!spline.IsInitialized)
				{
					if (sanityErrorLogsThisFrame < 20)
					{
						if (sanityErrorLogsThisFrame == 19)
						{
							DTLog.LogError("[Curvy] Too many errors to display.", spline);
						}
						else
						{
							DTLog.LogError("[Curvy] Calling public method on non initialized spline.", spline);
						}
						sanityErrorLogsThisFrame++;
					}
				}
				else if (spline.Dirty && sanityWaringLogsThisFrame < 20)
				{
					if (sanityWaringLogsThisFrame == 19)
					{
						DTLog.LogWarning("[Curvy] Too many warnings to display.", spline);
					}
					else
					{
						DTLog.LogWarning(string.Format(CultureInfo.InvariantCulture, "[Curvy] Calling public method on a dirty spline. The returned result will not be up to date. Either refresh the spline manually by calling Refresh(), or wait for it to be refreshed automatically at the next {0} call", spline.UpdateIn.ToString()), spline);
					}
					sanityWaringLogsThisFrame++;
				}
			}
		}

		private struct SegmentGroupMetrics : IEquatable<SegmentGroupMetrics>
		{
			public int CacheSize;

			public int SegmentCount;

			public float Length;

			public void Increment([NotNull] CurvySplineSegment segment)
			{
				CacheSize += segment.CacheSize;
				SegmentCount++;
				Length += segment.Length;
			}

			public bool Equals(SegmentGroupMetrics other)
			{
				if (CacheSize == other.CacheSize && SegmentCount == other.SegmentCount)
				{
					return Length.Equals(other.Length);
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is SegmentGroupMetrics other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((CacheSize * 397) ^ SegmentCount) * 397) ^ Length.GetHashCode();
			}

			public static bool operator ==(SegmentGroupMetrics left, SegmentGroupMetrics right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(SegmentGroupMetrics left, SegmentGroupMetrics right)
			{
				return !left.Equals(right);
			}
		}

		private class ControlPointNamer
		{
			[NotNull]
			private readonly CurvySpline spline;

			private bool requestRename;

			[NotNull]
			[ItemNotNull]
			private static readonly string[] ControlPointNames = GetControlPointNames();

			public ControlPointNamer([NotNull] CurvySpline curvySpline)
			{
				spline = curvySpline;
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void RequestRename()
			{
				requestRename = true;
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void ProcessRequests()
			{
				if (requestRename)
				{
					RenameControlPoints(spline.ControlPoints);
					requestRename = false;
				}
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			public void CancelRequests()
			{
				requestRename = false;
			}

			private static void RenameControlPoints([NotNull] List<CurvySplineSegment> splineControlPoints)
			{
				short num = (short)splineControlPoints.Count;
				for (short num2 = 0; num2 < num; num2++)
				{
					splineControlPoints[num2].name = GetControlPointName(num2);
				}
			}

			[NotNull]
			private static string GetControlPointName(short controlPointIndex)
			{
				if (controlPointIndex >= 250)
				{
					return MakeControlPointName(controlPointIndex);
				}
				return ControlPointNames[controlPointIndex];
			}

			[NotNull]
			[ItemNotNull]
			private static string[] GetControlPointNames()
			{
				string[] array = new string[250];
				for (short num = 0; num < 250; num++)
				{
					array[num] = MakeControlPointName(num);
				}
				return array;
			}

			[NotNull]
			private static string MakeControlPointName(short controlPointIndex)
			{
				string text = controlPointIndex.ToString("D4", CultureInfo.InvariantCulture);
				return "CP" + text;
			}
		}

		[Obsolete("Use FluffyUnderware.Curvy.AssetInformation instead")]
		public const string VERSION = "8.5.0";

		[Obsolete("Use FluffyUnderware.Curvy.AssetInformation instead")]
		public const string APIVERSION = "850";

		[Obsolete("Use FluffyUnderware.Curvy.AssetInformation instead")]
		public const string WEBROOT = "https://curvyeditor.com/";

		[Obsolete("Use FluffyUnderware.Curvy.AssetInformation instead")]
		public const string DOCLINK = "https://curvyeditor.com/doclink/";

		[HideInInspector]
		public bool ShowGizmos = true;

		[SerializeField]
		[HideInInspector]
		[NotNull]
		private List<CurvySplineSegment> ControlPoints = new List<CurvySplineSegment>();

		[Section("General", true, false, 100, HelpURL = "https://curvyeditor.com/doclink/curvyspline_general")]
		[Tooltip("Interpolation Method")]
		[SerializeField]
		[FormerlySerializedAs("Interpolation")]
		private CurvyInterpolation m_Interpolation = CurvyGlobalManager.DefaultInterpolation;

		[Tooltip("Restrict Control Points to a local 2D plane")]
		[SerializeField]
		private bool m_RestrictTo2D;

		[Tooltip("The local 2D plane to restrict the spline's control points to")]
		[SerializeField]
		[FieldCondition("RestrictTo2D", true, false, ActionAttribute.ActionEnum.Show, null, ActionAttribute.ActionPositionEnum.Below)]
		[FieldAction("CBCheck2DPlanar", ActionAttribute.ActionEnum.Callback)]
		private CurvyPlane restricted2DPlane;

		[SerializeField]
		[FormerlySerializedAs("Closed")]
		private bool m_Closed;

		[FieldCondition("CanHaveManualEndCp", Action = ActionAttribute.ActionEnum.Enable)]
		[Tooltip("Handle End Control Points automatically?")]
		[SerializeField]
		[FormerlySerializedAs("AutoEndTangents")]
		private bool m_AutoEndTangents = true;

		[Tooltip("Orientation Flow")]
		[SerializeField]
		[FormerlySerializedAs("Orientation")]
		private CurvyOrientation m_Orientation = CurvyOrientation.Dynamic;

		[Section("Global Bezier Options", true, false, 100, HelpURL = "https://curvyeditor.com/doclink/curvyspline_bezier")]
		[GroupCondition("m_Interpolation", CurvyInterpolation.Bezier, false)]
		[RangeEx(0f, 1f, "Default Distance %", "Handle length by distance to neighbours")]
		[SerializeField]
		private float m_AutoHandleDistance = 0.39f;

		[Section("Global TCB Options", true, false, 100, HelpURL = "https://curvyeditor.com/doclink/curvyspline_tcb")]
		[GroupCondition("m_Interpolation", CurvyInterpolation.TCB, false)]
		[GroupAction("TCBOptionsGUI", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[SerializeField]
		[FormerlySerializedAs("Tension")]
		private float m_Tension;

		[SerializeField]
		[FormerlySerializedAs("Continuity")]
		private float m_Continuity;

		[SerializeField]
		[FormerlySerializedAs("Bias")]
		private float m_Bias;

		[Section("B-Spline Options", true, false, 100, HelpURL = "https://curvyeditor.com/doclink/curvyspline_bspline")]
		[GroupCondition("m_Interpolation", CurvyInterpolation.BSpline, false)]
		[RangeEx(2f, "MaxBSplineDegree", "Degree", "The degree of the piecewise polynomial functions.\nIs in the range [2; control points count - 1]")]
		[SerializeField]
		private int bSplineDegree = 2;

		[FieldCondition("CanBeClamped", Action = ActionAttribute.ActionEnum.Enable)]
		[Label("Clamped", "Make the curve pass through the first and last control points by increasing the multiplicity of the first and last knots.\n\nIn technical terms, when this parameter is true, the knot vector is [0, 0, ...,0, 1, 2, ..., N-1, N, N, ..., N]. When false, it is [0, 1, 2, ..., N-1, N]")]
		[SerializeField]
		private bool isBSplineClamped = true;

		[Section("Advanced Settings", true, false, 100, HelpURL = "https://curvyeditor.com/doclink/curvyspline_advanced")]
		[FieldAction("ShowGizmoGUI", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Above)]
		[Label("Color", "Gizmo color")]
		[SerializeField]
		private Color m_GizmoColor = CurvyGlobalManager.DefaultGizmoColor;

		[FieldAction("CheckGizmoColor", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Above)]
		[FieldAction("CheckGizmoSelectionColor", ActionAttribute.ActionEnum.Callback, Position = ActionAttribute.ActionPositionEnum.Below)]
		[Label("Active Color", "Selected Gizmo color")]
		[SerializeField]
		private Color m_GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;

		[RangeEx(1f, 100f, "", "")]
		[SerializeField]
		[FormerlySerializedAs("Granularity")]
		[Tooltip("Defines how densely the cached points are. When the value is 100, the number of cached points per world distance unit is equal to the spline's MaxPointsPerUnit")]
		private int m_CacheDensity = 50;

		[SerializeField]
		[Tooltip("The maximum number of sampling points per world distance unit. Sampling is used in caching or shape extrusion for example")]
		private float m_MaxPointsPerUnit = 8f;

		[SerializeField]
		[Tooltip("Use a GameObject pool at runtime")]
		private bool m_UsePooling = true;

		[SerializeField]
		[Tooltip("Use threading where applicable. Threading is is currently not supported when targetting WebGL and Universal Windows Platform")]
		private bool m_UseThreading;

		[Tooltip("Refresh when Control Point position change?")]
		[SerializeField]
		[FormerlySerializedAs("AutoRefresh")]
		private bool m_CheckTransform = true;

		[SerializeField]
		private CurvyUpdateMethod m_UpdateIn;

		[Group("Events", Expanded = false, Sort = 1000, HelpURL = "https://curvyeditor.com/doclink/curvyspline_events")]
		[SerializeField]
		protected CurvySplineEvent onInitialized = new CurvySplineEvent();

		[Group("Events", Sort = 1000)]
		[SerializeField]
		protected CurvySplineEvent m_OnRefresh = new CurvySplineEvent();

		[Group("Events", Sort = 1000)]
		[SerializeField]
		protected CurvySplineEvent m_OnAfterControlPointChanges = new CurvySplineEvent();

		[Group("Events", Sort = 1000)]
		[SerializeField]
		protected CurvyControlPointEvent m_OnBeforeControlPointAdd = new CurvyControlPointEvent();

		[Group("Events", Sort = 1000)]
		[SerializeField]
		protected CurvyControlPointEvent m_OnAfterControlPointAdd = new CurvyControlPointEvent();

		[Group("Events", Sort = 1000)]
		[SerializeField]
		protected CurvyControlPointEvent m_OnBeforeControlPointDelete = new CurvyControlPointEvent();

		private Action<CurvySpline> onGlobalCoordinatesChanged;

		private bool mIsInitialized;

		private bool isStarted;

		private bool sendOnRefreshEventNextUpdate;

		private readonly List<CurvySplineSegment> mSegments = new List<CurvySplineSegment>();

		private readonly DirtinessManager dirtinessManager;

		private readonly RelationshipCache relationshipCache;

		[NotNull]
		private readonly SanityChecker sanityChecker;

		[NotNull]
		private readonly ControlPointsSynchronizer cpsSynchronizer;

		[NotNull]
		private readonly ControlPointNamer controlPointNamer;

		[CanBeNull]
		private TransformMonitor transformMonitor;

		private Transform cachedTransform;

		private ReadOnlyCollection<CurvySplineSegment> readOnlyControlPoints;

		private short[] cachedShortsArray = Array.Empty<short>();

		private float[] controlPointsDistances = Array.Empty<float>();

		private readonly Action<CurvySplineSegment, int, int> refreshCurveAction;

		private float length = -1f;

		private int mCacheSize = -1;

		private Bounds? mBounds;

		private readonly CurvySplineEventArgs defaultSplineEventArgs;

		private readonly CurvyControlPointEventArgs defaultAddAfterEventArgs;

		private readonly CurvyControlPointEventArgs defaultDeleteEventArgs;

		private const short CachedControlPointsNameCount = 250;

		private const float MinimalMaxPointsPerUnit = 0.0001f;

		private const float MaxSegmentCacheSize = 1000000f;

		private const string InvalidCPErrorMessage = "[Curvy] Method called with a control point '{0}' that is not part of the current spline '{1}'";

		private const int MinBSplineDegree = 2;

		public CurvyInterpolation Interpolation
		{
			get
			{
				return m_Interpolation;
			}
			set
			{
				if (m_Interpolation != value)
				{
					m_Interpolation = value;
					relationshipCache.Invalidate();
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
				AutoEndTangents = m_AutoEndTangents;
			}
		}

		public bool RestrictTo2D
		{
			get
			{
				return m_RestrictTo2D;
			}
			set
			{
				if (m_RestrictTo2D != value)
				{
					m_RestrictTo2D = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public CurvyPlane Restricted2DPlane
		{
			get
			{
				return restricted2DPlane;
			}
			set
			{
				if (restricted2DPlane != value)
				{
					restricted2DPlane = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
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
				float num = Mathf.Clamp01(value);
				if (m_AutoHandleDistance != num)
				{
					m_AutoHandleDistance = num;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public bool Closed
		{
			get
			{
				return m_Closed;
			}
			set
			{
				if (m_Closed != value)
				{
					m_Closed = value;
					relationshipCache.Invalidate();
					SetDirtyAll(SplineDirtyingType.Everything, base.IsActiveAndEnabled);
				}
				AutoEndTangents = m_AutoEndTangents;
			}
		}

		public bool AutoEndTangents
		{
			get
			{
				return m_AutoEndTangents;
			}
			set
			{
				bool flag = !CanHaveManualEndCp() || value;
				if (m_AutoEndTangents != flag)
				{
					m_AutoEndTangents = flag;
					relationshipCache.Invalidate();
					SetDirtyAll(SplineDirtyingType.Everything, base.IsActiveAndEnabled);
				}
			}
		}

		public CurvyOrientation Orientation
		{
			get
			{
				return m_Orientation;
			}
			set
			{
				if (m_Orientation != value)
				{
					m_Orientation = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public CurvyUpdateMethod UpdateIn
		{
			get
			{
				return m_UpdateIn;
			}
			set
			{
				m_UpdateIn = value;
			}
		}

		public Color GizmoColor
		{
			get
			{
				return m_GizmoColor;
			}
			set
			{
				m_GizmoColor = value;
			}
		}

		public Color GizmoSelectionColor
		{
			get
			{
				return m_GizmoSelectionColor;
			}
			set
			{
				m_GizmoSelectionColor = value;
			}
		}

		public int CacheDensity
		{
			get
			{
				return m_CacheDensity;
			}
			set
			{
				int num = Mathf.Clamp(value, 1, 100);
				if (m_CacheDensity != num)
				{
					m_CacheDensity = num;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public float MaxPointsPerUnit
		{
			get
			{
				return m_MaxPointsPerUnit;
			}
			set
			{
				float num = Mathf.Clamp(value, 0.0001f, 1000f);
				if (m_MaxPointsPerUnit != num)
				{
					m_MaxPointsPerUnit = num;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public bool UsePooling
		{
			get
			{
				return m_UsePooling;
			}
			set
			{
				m_UsePooling = value;
			}
		}

		public bool UseThreading
		{
			get
			{
				return m_UseThreading;
			}
			set
			{
				m_UseThreading = value;
			}
		}

		public bool CheckTransform
		{
			get
			{
				return m_CheckTransform;
			}
			set
			{
				if (m_CheckTransform != value)
				{
					m_CheckTransform = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public float Tension
		{
			get
			{
				return m_Tension;
			}
			set
			{
				if (m_Tension != value)
				{
					m_Tension = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public float Continuity
		{
			get
			{
				return m_Continuity;
			}
			set
			{
				if (m_Continuity != value)
				{
					m_Continuity = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public float Bias
		{
			get
			{
				return m_Bias;
			}
			set
			{
				if (m_Bias != value)
				{
					m_Bias = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public int BSplineDegree
		{
			get
			{
				return bSplineDegree;
			}
			set
			{
				value = Mathf.Min(Mathf.Max(2, value), MaxBSplineDegree);
				if (bSplineDegree != value)
				{
					bSplineDegree = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public bool IsBSplineClamped
		{
			get
			{
				if (CanBeClamped())
				{
					return isBSplineClamped;
				}
				return false;
			}
			set
			{
				if (isBSplineClamped != value)
				{
					isBSplineClamped = value;
					SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
				}
			}
		}

		public bool IsInitialized => mIsInitialized;

		public Bounds Bounds
		{
			get
			{
				if (!mBounds.HasValue)
				{
					Bounds bounds2;
					if (Count > 0)
					{
						Bounds bounds = this[0].Bounds;
						for (int i = 1; i < Count; i++)
						{
							bounds.Encapsulate(this[i].Bounds);
						}
						bounds2 = bounds;
					}
					else
					{
						bounds2 = new Bounds(base.transform.position, Vector3.zero);
					}
					if (!Dirty)
					{
						mBounds = bounds2;
					}
					return bounds2;
				}
				return mBounds.Value;
			}
		}

		public int Count => Segments.Count;

		public int ControlPointCount => ControlPoints.Count;

		public int CacheSize
		{
			get
			{
				if (mCacheSize < 0)
				{
					int num = 0;
					List<CurvySplineSegment> segments = Segments;
					for (int i = 0; i < segments.Count; i++)
					{
						num += segments[i].CacheSize;
					}
					if (!Dirty)
					{
						mCacheSize = num;
					}
					return num;
				}
				return mCacheSize;
			}
		}

		public float Length
		{
			get
			{
				if (length < 0f)
				{
					float result = ((Segments.Count != 0) ? (Closed ? (this[Count - 1].Distance + this[Count - 1].Length) : LastVisibleControlPoint.Distance) : 0f);
					if (!Dirty)
					{
						length = result;
					}
					return result;
				}
				return length;
			}
		}

		public bool Dirty => dirtinessManager.Dirty;

		public CurvySplineSegment this[int idx] => Segments[idx];

		public ReadOnlyCollection<CurvySplineSegment> ControlPointsList
		{
			get
			{
				if (readOnlyControlPoints == null)
				{
					readOnlyControlPoints = ControlPoints.AsReadOnly();
				}
				return readOnlyControlPoints;
			}
		}

		[CanBeNull]
		public CurvySplineSegment FirstVisibleControlPoint => relationshipCache.FirstVisibleControlPoint;

		[CanBeNull]
		public CurvySplineSegment LastVisibleControlPoint => relationshipCache.LastVisibleControlPoint;

		[CanBeNull]
		public CurvySplineSegment FirstSegment => relationshipCache.FirstSegment;

		[CanBeNull]
		public CurvySplineSegment LastSegment => relationshipCache.LastSegment;

		public bool GlobalCoordinatesChangedThisFrame => TransformMonitor.HasChanged;

		[CanBeNull]
		public Action<CurvySpline> OnGlobalCoordinatesChanged
		{
			get
			{
				return onGlobalCoordinatesChanged;
			}
			set
			{
				onGlobalCoordinatesChanged = value;
			}
		}

		public CurvySplineEvent OnRefresh
		{
			get
			{
				return m_OnRefresh;
			}
			set
			{
				m_OnRefresh = value;
			}
		}

		public CurvySplineEvent OnInitialized
		{
			get
			{
				return onInitialized;
			}
			set
			{
				onInitialized = value;
			}
		}

		public CurvySplineEvent OnAfterControlPointChanges
		{
			get
			{
				return m_OnAfterControlPointChanges;
			}
			set
			{
				m_OnAfterControlPointChanges = value;
			}
		}

		public CurvyControlPointEvent OnBeforeControlPointAdd
		{
			get
			{
				return m_OnBeforeControlPointAdd;
			}
			set
			{
				m_OnBeforeControlPointAdd = value;
			}
		}

		public CurvyControlPointEvent OnAfterControlPointAdd
		{
			get
			{
				return m_OnAfterControlPointAdd;
			}
			set
			{
				m_OnAfterControlPointAdd = value;
			}
		}

		public CurvyControlPointEvent OnBeforeControlPointDelete
		{
			get
			{
				return m_OnBeforeControlPointDelete;
			}
			set
			{
				m_OnBeforeControlPointDelete = value;
			}
		}

		[NotNull]
		private TransformMonitor TransformMonitor
		{
			get
			{
				if (transformMonitor == null)
				{
					transformMonitor = new TransformMonitor(base.transform, monitorPosition: true, monitorRotation: true, monitorScale: true);
				}
				return transformMonitor;
			}
		}

		private List<CurvySplineSegment> Segments
		{
			get
			{
				relationshipCache.EnsureIsValid();
				return mSegments;
			}
		}

		private int MaxBSplineDegree => Mathf.Max(2, ControlPoints.Count - 1);

		public CurvySpline()
		{
			sanityChecker = new SanityChecker(this);
			dirtinessManager = new DirtinessManager(this);
			relationshipCache = new RelationshipCache(this);
			refreshCurveAction = delegate(CurvySplineSegment controlPoint, int controlPointIndex, int controlPointsCount)
			{
				controlPoint.refreshCurveINTERNAL();
			};
			defaultSplineEventArgs = new CurvySplineEventArgs(this, this);
			defaultAddAfterEventArgs = new CurvyControlPointEventArgs(this, this, null, CurvyControlPointEventArgs.ModeEnum.AddAfter);
			defaultDeleteEventArgs = new CurvyControlPointEventArgs(this, this, null, CurvyControlPointEventArgs.ModeEnum.Delete);
			cpsSynchronizer = new ControlPointsSynchronizer(this);
			controlPointNamer = new ControlPointNamer(this);
		}

		public static CurvySpline Create()
		{
			CurvySpline component = new GameObject("Curvy Spline", typeof(CurvySpline)).GetComponent<CurvySpline>();
			component.gameObject.layer = CurvyGlobalManager.SplineLayer;
			component.Start();
			return component;
		}

		public static CurvySpline Create(CurvySpline takeOptionsFrom)
		{
			CurvySpline curvySpline = Create();
			if ((bool)takeOptionsFrom)
			{
				curvySpline.RestrictTo2D = takeOptionsFrom.RestrictTo2D;
				curvySpline.GizmoColor = takeOptionsFrom.GizmoColor;
				curvySpline.GizmoSelectionColor = takeOptionsFrom.GizmoSelectionColor;
				curvySpline.Interpolation = takeOptionsFrom.Interpolation;
				curvySpline.Closed = takeOptionsFrom.Closed;
				curvySpline.AutoEndTangents = takeOptionsFrom.AutoEndTangents;
				curvySpline.CacheDensity = takeOptionsFrom.CacheDensity;
				curvySpline.MaxPointsPerUnit = takeOptionsFrom.MaxPointsPerUnit;
				curvySpline.Orientation = takeOptionsFrom.Orientation;
				curvySpline.CheckTransform = takeOptionsFrom.CheckTransform;
			}
			return curvySpline;
		}

		public static int CalculateCacheSize(int density, float segmentLength, float maxPointsPerUnit)
		{
			return Mathf.FloorToInt(Math.Min(CalculateSamplingPointsPerUnit(density, maxPointsPerUnit) * segmentLength, 999999f)) + 1;
		}

		public static float CalculateSamplingPointsPerUnit(int density, float maxPointsPerUnit)
		{
			int num = Mathf.Clamp(density, 1, 100);
			if (num != density)
			{
				DTLog.LogWarning("[Curvy] CalculateSamplingPointsPerUnit got an invalid density parameter. It should be between 1 and 100. The parameter value was " + density);
				density = num;
			}
			return DTTween.QuadIn(density - 1, 0.0001f, maxPointsPerUnit, 99f);
		}

		public static Vector3 Bezier(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
		{
			double num = (double)(0f - P0.x) + 3.0 * (double)T0.x + -3.0 * (double)T1.x + (double)P1.x;
			double num2 = 3.0 * (double)P0.x + -6.0 * (double)T0.x + 3.0 * (double)T1.x;
			double num3 = -3.0 * (double)P0.x + 3.0 * (double)T0.x;
			double num4 = P0.x;
			double num5 = (double)(0f - P0.y) + 3.0 * (double)T0.y + -3.0 * (double)T1.y + (double)P1.y;
			double num6 = 3.0 * (double)P0.y + -6.0 * (double)T0.y + 3.0 * (double)T1.y;
			double num7 = -3.0 * (double)P0.y + 3.0 * (double)T0.y;
			double num8 = P0.y;
			double num9 = (double)(0f - P0.z) + 3.0 * (double)T0.z + -3.0 * (double)T1.z + (double)P1.z;
			double num10 = 3.0 * (double)P0.z + -6.0 * (double)T0.z + 3.0 * (double)T1.z;
			double num11 = -3.0 * (double)P0.z + 3.0 * (double)T0.z;
			double num12 = P0.z;
			float x = (float)(((num * (double)f + num2) * (double)f + num3) * (double)f + num4);
			float y = (float)(((num5 * (double)f + num6) * (double)f + num7) * (double)f + num8);
			float z = (float)(((num9 * (double)f + num10) * (double)f + num11) * (double)f + num12);
			Vector3 result = default(Vector3);
			result.x = x;
			result.y = y;
			result.z = z;
			return result;
		}

		public static Vector3 BezierTangent(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
		{
			Vector3 vector = P1 - 3f * T1 + 3f * T0 - P0;
			Vector3 vector2 = 3f * T1 - 6f * T0 + 3f * P0;
			Vector3 vector3 = 3f * T0 - 3f * P0;
			return 3f * f * f * vector + 2f * f * vector2 + vector3;
		}

		public static Vector3 CatmullRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
		{
			double num = -0.5 * (double)T0.x + 1.5 * (double)P0.x + -1.5 * (double)P1.x + 0.5 * (double)T1.x;
			double num2 = (double)T0.x + -2.5 * (double)P0.x + 2.0 * (double)P1.x + -0.5 * (double)T1.x;
			double num3 = -0.5 * (double)T0.x + 0.5 * (double)P1.x;
			double num4 = P0.x;
			double num5 = -0.5 * (double)T0.y + 1.5 * (double)P0.y + -1.5 * (double)P1.y + 0.5 * (double)T1.y;
			double num6 = (double)T0.y + -2.5 * (double)P0.y + 2.0 * (double)P1.y + -0.5 * (double)T1.y;
			double num7 = -0.5 * (double)T0.y + 0.5 * (double)P1.y;
			double num8 = P0.y;
			double num9 = -0.5 * (double)T0.z + 1.5 * (double)P0.z + -1.5 * (double)P1.z + 0.5 * (double)T1.z;
			double num10 = (double)T0.z + -2.5 * (double)P0.z + 2.0 * (double)P1.z + -0.5 * (double)T1.z;
			double num11 = -0.5 * (double)T0.z + 0.5 * (double)P1.z;
			double num12 = P0.z;
			float x = (float)(((num * (double)f + num2) * (double)f + num3) * (double)f + num4);
			float y = (float)(((num5 * (double)f + num6) * (double)f + num7) * (double)f + num8);
			float z = (float)(((num9 * (double)f + num10) * (double)f + num11) * (double)f + num12);
			Vector3 result = default(Vector3);
			result.x = x;
			result.y = y;
			result.z = z;
			return result;
		}

		public static Vector3 TCB(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f, float FT0, float FC0, float FB0, float FT1, float FC1, float FB1)
		{
			double num = (1f - FT0) * (1f + FC0) * (1f + FB0);
			double num2 = (1f - FT0) * (1f - FC0) * (1f - FB0);
			double num3 = (1f - FT1) * (1f - FC1) * (1f + FB1);
			double num4 = (1f - FT1) * (1f + FC1) * (1f - FB1);
			double num5 = 2.0;
			double num6 = (0.0 - num) / num5;
			double num7 = (4.0 + num - num2 - num3) / num5;
			double num8 = (-4.0 + num2 + num3 - num4) / num5;
			double num9 = num4 / num5;
			double num10 = 2.0 * num / num5;
			double num11 = (-6.0 - 2.0 * num + 2.0 * num2 + num3) / num5;
			double num12 = (6.0 - 2.0 * num2 - num3 + num4) / num5;
			double num13 = (0.0 - num4) / num5;
			double num14 = (0.0 - num) / num5;
			double num15 = (num - num2) / num5;
			double num16 = num2 / num5;
			double num17 = 2.0 / num5;
			double num18 = num6 * (double)T0.x + num7 * (double)P0.x + num8 * (double)P1.x + num9 * (double)T1.x;
			double num19 = num10 * (double)T0.x + num11 * (double)P0.x + num12 * (double)P1.x + num13 * (double)T1.x;
			double num20 = num14 * (double)T0.x + num15 * (double)P0.x + num16 * (double)P1.x;
			double num21 = num17 * (double)P0.x;
			double num22 = num6 * (double)T0.y + num7 * (double)P0.y + num8 * (double)P1.y + num9 * (double)T1.y;
			double num23 = num10 * (double)T0.y + num11 * (double)P0.y + num12 * (double)P1.y + num13 * (double)T1.y;
			double num24 = num14 * (double)T0.y + num15 * (double)P0.y + num16 * (double)P1.y;
			double num25 = num17 * (double)P0.y;
			double num26 = num6 * (double)T0.z + num7 * (double)P0.z + num8 * (double)P1.z + num9 * (double)T1.z;
			double num27 = num10 * (double)T0.z + num11 * (double)P0.z + num12 * (double)P1.z + num13 * (double)T1.z;
			double num28 = num14 * (double)T0.z + num15 * (double)P0.z + num16 * (double)P1.z;
			double num29 = num17 * (double)P0.z;
			float x = (float)(((num18 * (double)f + num19) * (double)f + num20) * (double)f + num21);
			float y = (float)(((num22 * (double)f + num23) * (double)f + num24) * (double)f + num25);
			float z = (float)(((num26 * (double)f + num27) * (double)f + num28) * (double)f + num29);
			Vector3 result = default(Vector3);
			result.x = x;
			result.y = y;
			result.z = z;
			return result;
		}

		public static CurvySplineSegment GetFollowUpHeadingControlPoint([NotNull] CurvySplineSegment followUp, ConnectionHeadingEnum headingDirection)
		{
			return headingDirection.ResolveAuto(followUp) switch
			{
				ConnectionHeadingEnum.Minus => followUp.Spline.GetPreviousControlPoint(followUp), 
				ConnectionHeadingEnum.Plus => followUp.Spline.GetNextControlPoint(followUp), 
				ConnectionHeadingEnum.Sharp => followUp, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public Vector3 Interpolate(float tf, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.Interpolate(localF, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.position;
			}
			return Vector3.zero;
		}

		public Vector3 InterpolateFast(float tf, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.InterpolateFast(localF, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.position;
			}
			return Vector3.zero;
		}

		public Vector3 InterpolateByDistance(float distance, Space space = Space.Self)
		{
			return Interpolate(DistanceToTF(distance), space);
		}

		public Vector3 InterpolateByDistanceFast(float distance, Space space = Space.Self)
		{
			return InterpolateFast(DistanceToTF(distance), space);
		}

		public Vector3 GetTangent(float tf, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.GetTangent(localF, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.position;
			}
			return Vector3.zero;
		}

		public Vector3 GetTangent(float tf, Vector3 position, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.GetTangent(localF, position, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.position;
			}
			return Vector3.zero;
		}

		public Vector3 GetTangentFast(float tf, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.GetTangentFast(localF, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.position;
			}
			return Vector3.zero;
		}

		public Vector3 GetTangentByDistance(float distance, Space space = Space.Self)
		{
			return GetTangent(DistanceToTF(distance), space);
		}

		public Vector3 GetTangentByDistanceFast(float distance, Space space = Space.Self)
		{
			return GetTangentFast(DistanceToTF(distance), space);
		}

		public void InterpolateAndGetTangent(float tf, out Vector3 position, out Vector3 tangent, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				curvySplineSegment.InterpolateAndGetTangent(localF, out position, out tangent, space);
			}
			else
			{
				position = (tangent = ((space == Space.Self) ? Vector3.zero : cachedTransform.position));
			}
		}

		public void InterpolateAndGetTangentFast(float tf, out Vector3 position, out Vector3 tangent, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				curvySplineSegment.InterpolateAndGetTangentFast(localF, out position, out tangent, space);
			}
			else
			{
				position = (tangent = ((space == Space.Self) ? Vector3.zero : cachedTransform.position));
			}
		}

		public Vector3 GetOrientationUpFast(float tf, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.GetOrientationUpFast(localF, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.position;
			}
			return Vector3.zero;
		}

		public Quaternion GetOrientationFast(float tf, bool inverse = false, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment != null)
			{
				return curvySplineSegment.GetOrientationFast(localF, inverse, space);
			}
			if (space != Space.Self)
			{
				return cachedTransform.rotation;
			}
			return Quaternion.identity;
		}

		public T GetMetadata<T>(float tf) where T : CurvyMetadataBase
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment == null)
			{
				return null;
			}
			return curvySplineSegment.GetMetadata<T>();
		}

		public U GetInterpolatedMetadata<T, U>(float tf) where T : CurvyInterpolatableMetadataBase<U>
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF);
			if ((object)curvySplineSegment == null)
			{
				return default(U);
			}
			return curvySplineSegment.GetInterpolatedMetadata<T, U>(localF);
		}

		public float TFToDistance(float tf, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			float num = Length;
			if (num == 0f)
			{
				return 0f;
			}
			if (tf == 0f)
			{
				return 0f;
			}
			if (tf == 1f)
			{
				return num;
			}
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(tf, out localF, clamping);
			return ((object)curvySplineSegment != null) ? (curvySplineSegment.Distance + curvySplineSegment.LocalFToDistance(localF)) : 0f;
		}

		public CurvySplineSegment TFToSegment(float tf, out float localF, out bool isOnSegmentStart, out bool isOnSegmentEnd, CurvyClamping clamping)
		{
			tf = CurvyUtility.ClampTF(tf, clamping);
			int count = Count;
			if (count == 0)
			{
				localF = 0f;
				isOnSegmentStart = false;
				isOnSegmentEnd = false;
				return null;
			}
			float num = tf * (float)count;
			int num2 = (int)num;
			localF = num - (float)num2;
			if (num2 == count)
			{
				num2--;
				localF = 1f;
			}
			isOnSegmentStart = num == (float)num2;
			isOnSegmentEnd = tf == 1f;
			return this[num2];
		}

		public CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
		{
			bool isOnSegmentStart;
			bool isOnSegmentEnd;
			return TFToSegment(tf, out localF, out isOnSegmentStart, out isOnSegmentEnd, clamping);
		}

		public CurvySplineSegment TFToSegment(float tf, CurvyClamping clamping)
		{
			float localF;
			return TFToSegment(tf, out localF, clamping);
		}

		public CurvySplineSegment TFToSegment(float tf)
		{
			float localF;
			return TFToSegment(tf, out localF, CurvyClamping.Clamp);
		}

		public CurvySplineSegment TFToSegment(float tf, out float localF)
		{
			return TFToSegment(tf, out localF, CurvyClamping.Clamp);
		}

		public float SegmentToTF(CurvySplineSegment segment)
		{
			relationshipCache.EnsureIsValid();
			return segment.GetExtrinsicPropertiesINTERNAL().TF;
		}

		public float SegmentToTF(CurvySplineSegment segment, float localF)
		{
			float num;
			if (IsControlPointASegment(segment))
			{
				num = SegmentToTF(segment) + localF / (float)Count;
				if (num > 1f)
				{
					num = 1f;
				}
			}
			else
			{
				num = SegmentToTF(segment);
			}
			return num;
		}

		public float DistanceToTF(float distance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			if (Length == 0f)
			{
				return 0f;
			}
			if (distance == 0f)
			{
				return 0f;
			}
			if (distance == Length)
			{
				return 1f;
			}
			float localDistance;
			CurvySplineSegment curvySplineSegment = DistanceToSegment(distance, out localDistance, clamping);
			return ((object)curvySplineSegment != null) ? SegmentToTF(curvySplineSegment, curvySplineSegment.DistanceToLocalF(localDistance)) : 0f;
		}

		public CurvySplineSegment DistanceToSegment(float distance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			float localDistance;
			return DistanceToSegment(distance, out localDistance, clamping);
		}

		public CurvySplineSegment DistanceToSegment(float distance, out float localDistance, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			bool isOnSegmentStart;
			bool isOnSegmentEnd;
			return DistanceToSegment(distance, out localDistance, out isOnSegmentStart, out isOnSegmentEnd, clamping);
		}

		public CurvySplineSegment DistanceToSegment(float distance, out float localDistance, out bool isOnSegmentStart, out bool isOnSegmentEnd, CurvyClamping clamping = CurvyClamping.Clamp)
		{
			distance = CurvyUtility.ClampDistance(distance, clamping, Length);
			CurvySplineSegment curvySplineSegment;
			if (Count > 0)
			{
				int num = CurvyUtility.InterpolationSearch(controlPointsDistances, controlPointsDistances.Length, distance);
				bool num2 = !AutoEndTangents;
				int count = ControlPointsList.Count;
				if (num2)
				{
					if (num == 0)
					{
						num = 1;
					}
					else if (num == count - 1 || num == count - 2)
					{
						num = count - 3;
					}
				}
				else if (!Closed && num == count - 1)
				{
					num = count - 2;
				}
				curvySplineSegment = ControlPointsList[num];
				localDistance = distance - curvySplineSegment.Distance;
				isOnSegmentStart = distance == curvySplineSegment.Distance;
				isOnSegmentEnd = distance == Length;
			}
			else
			{
				curvySplineSegment = null;
				localDistance = -1f;
				isOnSegmentStart = false;
				isOnSegmentEnd = false;
			}
			return curvySplineSegment;
		}

		public float ClampDistance(float distance, CurvyClamping clamping)
		{
			return CurvyUtility.ClampDistance(distance, clamping, Length);
		}

		public float ClampDistance(float distance, CurvyClamping clamping, float min, float max)
		{
			return CurvyUtility.ClampDistance(distance, clamping, Length, min, max);
		}

		public float ClampDistance(float distance, ref int dir, CurvyClamping clamping)
		{
			return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length);
		}

		public float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float min, float max)
		{
			return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length, min, max);
		}

		public CurvySplineSegment Add()
		{
			return InsertAfter(null);
		}

		public CurvySplineSegment[] Add(int controlPointsCount)
		{
			Vector3[] controlPointsLocalPositions = new Vector3[controlPointsCount];
			return Add(controlPointsLocalPositions);
		}

		public CurvySplineSegment Add(Vector3 controlPointPosition, Space space)
		{
			OnBeforeControlPointAddEvent(defaultAddAfterEventArgs);
			CurvySplineSegment result = InsertAfter(null, controlPointPosition, skipRefreshingAndEvents: true, space);
			Refresh();
			OnAfterControlPointAddEvent(defaultAddAfterEventArgs);
			OnAfterControlPointChangesEvent(defaultSplineEventArgs);
			return result;
		}

		public CurvySplineSegment[] Add(params Vector3[] controlPointsLocalPositions)
		{
			OnBeforeControlPointAddEvent(defaultAddAfterEventArgs);
			CurvySplineSegment[] array = new CurvySplineSegment[controlPointsLocalPositions.Length];
			for (int i = 0; i < controlPointsLocalPositions.Length; i++)
			{
				array[i] = InsertAfter(null, controlPointsLocalPositions[i], skipRefreshingAndEvents: true, Space.Self);
			}
			Refresh();
			OnAfterControlPointAddEvent(defaultAddAfterEventArgs);
			OnAfterControlPointChangesEvent(defaultSplineEventArgs);
			return array;
		}

		public CurvySplineSegment[] Add(Vector3[] controlPointsPositions, Space space)
		{
			OnBeforeControlPointAddEvent(defaultAddAfterEventArgs);
			CurvySplineSegment[] array = new CurvySplineSegment[controlPointsPositions.Length];
			for (int i = 0; i < controlPointsPositions.Length; i++)
			{
				array[i] = InsertAfter(null, controlPointsPositions[i], skipRefreshingAndEvents: true, space);
			}
			Refresh();
			OnAfterControlPointAddEvent(defaultAddAfterEventArgs);
			OnAfterControlPointChangesEvent(defaultSplineEventArgs);
			return array;
		}

		public CurvySplineSegment InsertBefore(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents = false)
		{
			CurvySplineSegment previousControlPoint;
			Vector3 position = ((!controlPoint || !(previousControlPoint = GetPreviousControlPoint(controlPoint))) ? base.transform.position : (IsControlPointASegment(previousControlPoint) ? previousControlPoint.Interpolate(0.5f, Space.World) : previousControlPoint.transform.position.LerpUnclamped(controlPoint.transform.position, 0.5f)));
			return InsertBefore(controlPoint, position, skipRefreshingAndEvents);
		}

		public CurvySplineSegment InsertBefore([CanBeNull] CurvySplineSegment controlPoint, Vector3 position, bool skipRefreshingAndEvents = false, Space space = Space.World)
		{
			return InsertAt(controlPoint, position, ((object)controlPoint != null) ? Mathf.Max(0, GetControlPointIndex(controlPoint)) : 0, CurvyControlPointEventArgs.ModeEnum.AddBefore, skipRefreshingAndEvents, space);
		}

		public CurvySplineSegment InsertAfter(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents = false)
		{
			Vector3 position;
			if ((bool)controlPoint)
			{
				if (IsControlPointASegment(controlPoint))
				{
					position = controlPoint.Interpolate(0.5f, Space.World);
				}
				else
				{
					CurvySplineSegment nextControlPoint = GetNextControlPoint(controlPoint);
					position = (nextControlPoint ? nextControlPoint.transform.position.LerpUnclamped(controlPoint.transform.position, 0.5f) : controlPoint.transform.position);
				}
			}
			else
			{
				position = base.transform.position;
			}
			return InsertAfter(controlPoint, position, skipRefreshingAndEvents);
		}

		public CurvySplineSegment InsertAfter([CanBeNull] CurvySplineSegment controlPoint, Vector3 position, bool skipRefreshingAndEvents = false, Space space = Space.World)
		{
			return InsertAt(controlPoint, position, ((object)controlPoint != null) ? (GetControlPointIndex(controlPoint) + 1) : ControlPoints.Count, CurvyControlPointEventArgs.ModeEnum.AddAfter, skipRefreshingAndEvents, space);
		}

		public void Clear(bool isUndoable = true)
		{
			OnBeforeControlPointDeleteEvent(defaultDeleteEventArgs);
			for (int num = ControlPointCount - 1; num >= 0; num--)
			{
				DisposeOfControlPoint(ControlPoints[num], isUndoable);
			}
			ClearControlPoints(invalidateAndDirty: true, requestSplineToHierarchySynchronization: false);
			Refresh();
			OnAfterControlPointChangesEvent(defaultSplineEventArgs);
		}

		public void Delete(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents = false)
		{
			Delete(controlPoint, skipRefreshingAndEvents, isUndoableDeletion: true);
		}

		public void Delete(CurvySplineSegment controlPoint, bool skipRefreshingAndEvents, bool isUndoableDeletion)
		{
			if ((bool)controlPoint)
			{
				if (!skipRefreshingAndEvents)
				{
					OnBeforeControlPointDeleteEvent(new CurvyControlPointEventArgs(this, this, controlPoint, CurvyControlPointEventArgs.ModeEnum.Delete));
				}
				RemoveControlPoint(controlPoint);
				controlPoint.transform.SetAsLastSibling();
				DisposeOfControlPoint(controlPoint, isUndoableDeletion);
				if (!skipRefreshingAndEvents)
				{
					Refresh();
					OnAfterControlPointChangesEvent(defaultSplineEventArgs);
				}
			}
		}

		public SubArray<Vector3> GetPositionsCache(Space space)
		{
			return GetSegmentApproximationsInSpace((CurvySplineSegment s) => s.PositionsApproximation, space);
		}

		[UsedImplicitly]
		[Obsolete("Use GetPositionsCache instead")]
		public Vector3[] GetApproximation(Space space = Space.Self)
		{
			return GetPositionsCache(space).CopyToArray(ArrayPools.Vector3);
		}

		public Vector3[] GetApproximation(float fromTF, float toTF, bool includeEndPoint = true, Space space = Space.Self)
		{
			float localF;
			CurvySplineSegment curvySplineSegment = TFToSegment(fromTF, out localF);
			float frag;
			int num = curvySplineSegment.getApproximationIndexINTERNAL(localF, out frag);
			float localF2;
			CurvySplineSegment curvySplineSegment2 = TFToSegment(toTF, out localF2);
			float frag2;
			int approximationIndexINTERNAL = curvySplineSegment2.getApproximationIndexINTERNAL(localF2, out frag2);
			CurvySplineSegment curvySplineSegment3 = curvySplineSegment;
			SubArray<Vector3> positionsApproximation = curvySplineSegment3.PositionsApproximation;
			Vector3[] array = positionsApproximation.Array;
			Vector3[] array2 = new Vector3[1] { Vector3.Lerp(array[num], array[num + 1], frag) };
			while ((bool)curvySplineSegment3 && (object)curvySplineSegment3 != curvySplineSegment2)
			{
				array2 = array2.AddRange(array.SubArray(num + 1, positionsApproximation.Count - 1));
				num = 1;
				curvySplineSegment3 = curvySplineSegment3.Spline.GetNextSegment(curvySplineSegment3);
			}
			if ((object)curvySplineSegment3 != null)
			{
				int num2 = ((!(curvySplineSegment == curvySplineSegment3)) ? 1 : (num + 1));
				array2 = array2.AddRange(array.SubArray(num2, approximationIndexINTERNAL - num2));
				if (includeEndPoint && (frag2 > 0f || frag2 < 1f))
				{
					array2 = array2.Add(Vector3.Lerp(array[approximationIndexINTERNAL], array[approximationIndexINTERNAL + 1], frag2));
				}
			}
			if (space == Space.World)
			{
				Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = localToWorldMatrix.MultiplyPoint3x4(array2[i]);
				}
			}
			return array2;
		}

		public SubArray<Vector3> GetTangentsCache(Space space)
		{
			return GetSegmentApproximationsInSpace((CurvySplineSegment s) => s.TangentsApproximation, space);
		}

		[UsedImplicitly]
		[Obsolete("Use GetTangentsCache instead")]
		public Vector3[] GetApproximationT(Space space = Space.Self)
		{
			return GetTangentsCache(space).CopyToArray(ArrayPools.Vector3);
		}

		public SubArray<Vector3> GetNormalsCache(Space space)
		{
			return GetSegmentApproximationsInSpace((CurvySplineSegment s) => s.UpsApproximation, space);
		}

		[UsedImplicitly]
		[Obsolete("Use GetNormalsCache instead")]
		public Vector3[] GetApproximationUpVectors(Space space = Space.Self)
		{
			return GetNormalsCache(space).CopyToArray(ArrayPools.Vector3);
		}

		public Vector3 GetNearestPoint(Vector3 position, Space space)
		{
			GetNearestPointTF(position, out var nearestPoint, out var _, out var _, 0, -1, space);
			return nearestPoint;
		}

		public float GetNearestPointTF(Vector3 localPosition)
		{
			Vector3 nearestPoint;
			CurvySplineSegment nearestSegment;
			float nearestPointLocalF;
			return GetNearestPointTF(localPosition, out nearestPoint, out nearestSegment, out nearestPointLocalF);
		}

		public float GetNearestPointTF(Vector3 position, Space space)
		{
			Vector3 nearestPoint;
			CurvySplineSegment nearestSegment;
			float nearestPointLocalF;
			return GetNearestPointTF(position, out nearestPoint, out nearestSegment, out nearestPointLocalF, 0, -1, space);
		}

		public float GetNearestPointTF(Vector3 localPosition, out Vector3 nearestPoint)
		{
			CurvySplineSegment nearestSegment;
			float nearestPointLocalF;
			return GetNearestPointTF(localPosition, out nearestPoint, out nearestSegment, out nearestPointLocalF);
		}

		public float GetNearestPointTF(Vector3 position, out Vector3 nearestPoint, Space space)
		{
			CurvySplineSegment nearestSegment;
			float nearestPointLocalF;
			return GetNearestPointTF(position, out nearestPoint, out nearestSegment, out nearestPointLocalF, 0, -1, space);
		}

		public float GetNearestPointTF(Vector3 position, int searchStartSegmentIndex = 0, int searchEndSegmentIndex = -1, Space space = Space.Self)
		{
			Vector3 nearestPoint;
			CurvySplineSegment nearestSegment;
			float nearestPointLocalF;
			return GetNearestPointTF(position, out nearestPoint, out nearestSegment, out nearestPointLocalF, searchStartSegmentIndex, searchEndSegmentIndex, space);
		}

		public float GetNearestPointTF(Vector3 position, out Vector3 nearestPoint, int searchStartSegmentIndex = 0, int searchEndSegmentIndex = -1, Space space = Space.Self)
		{
			CurvySplineSegment nearestSegment;
			float nearestPointLocalF;
			return GetNearestPointTF(position, out nearestPoint, out nearestSegment, out nearestPointLocalF, searchStartSegmentIndex, searchEndSegmentIndex, space);
		}

		public float GetNearestPointTF(Vector3 position, out Vector3 nearestPoint, [CanBeNull] out CurvySplineSegment nearestSegment, out float nearestPointLocalF, int searchStartSegmentIndex = 0, int searchEndSegmentIndex = -1, Space space = Space.Self)
		{
			nearestPoint = Vector3.zero;
			if (Count == 0)
			{
				nearestSegment = null;
				nearestPointLocalF = -1f;
				return -1f;
			}
			float num = float.MaxValue;
			float num2 = 0f;
			CurvySplineSegment curvySplineSegment = null;
			if (searchEndSegmentIndex == -1)
			{
				searchEndSegmentIndex = Count - 1;
			}
			searchStartSegmentIndex = Mathf.Clamp(searchStartSegmentIndex, 0, Count - 1);
			searchEndSegmentIndex = Mathf.Clamp(searchEndSegmentIndex + 1, searchStartSegmentIndex + 1, Count);
			for (int i = searchStartSegmentIndex; i < searchEndSegmentIndex; i++)
			{
				float nearestPointF = this[i].GetNearestPointF(position, space);
				Vector3 vector = this[i].Interpolate(nearestPointF, space);
				float sqrMagnitude = (vector - position).sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					curvySplineSegment = this[i];
					num2 = nearestPointF;
					nearestPoint = vector;
					num = sqrMagnitude;
				}
			}
			nearestSegment = curvySplineSegment;
			nearestPointLocalF = num2;
			return curvySplineSegment.LocalFToTF(num2);
		}

		public void Refresh()
		{
			if (dirtinessManager.ProcessDirtyControlPoints())
			{
				OnRefreshEvent(defaultSplineEventArgs);
			}
		}

		public void SetDirtyAll()
		{
			SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: true);
		}

		public void SetDirtyAll(SplineDirtyingType dirtyingType, bool dirtyConnectedControlPoints)
		{
			dirtinessManager.SetDirtyAll(dirtyingType, dirtyConnectedControlPoints);
		}

		public void SetDirty(CurvySplineSegment dirtyControlPoint, SplineDirtyingType dirtyingType)
		{
			dirtinessManager.SetDirty(dirtyControlPoint, dirtyingType, GetPreviousControlPoint(dirtyControlPoint), GetNextControlPoint(dirtyControlPoint), ignoreConnectionOfInputControlPoint: false);
		}

		public void SetDirtyPartial(CurvySplineSegment dirtyControlPoint, SplineDirtyingType dirtyingType)
		{
			dirtinessManager.SetDirty(dirtyControlPoint, dirtyingType, GetPreviousControlPoint(dirtyControlPoint), GetNextControlPoint(dirtyControlPoint), ignoreConnectionOfInputControlPoint: true);
		}

		public Vector3 ToWorldPosition(Vector3 localPosition)
		{
			return cachedTransform.TransformPoint(localPosition);
		}

		public Vector3 ToWorldDirection(Vector3 localDirection)
		{
			return cachedTransform.TransformDirection(localDirection);
		}

		public Vector3 ToLocalPosition(Vector3 worldPosition)
		{
			return cachedTransform.InverseTransformPoint(worldPosition);
		}

		public Vector3 ToLocalDirection(Vector3 localDirection)
		{
			return cachedTransform.InverseTransformDirection(localDirection);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void ApplyControlPointsNames()
		{
		}

		public void SyncSplineFromHierarchy()
		{
			cpsSynchronizer.CancelRequests();
			cpsSynchronizer.RequestHierarchyToSpline();
			cpsSynchronizer.ProcessRequests();
		}

		[Obsolete]
		[UsedImplicitly]
		public bool IsPlanar(out int ignoreAxis)
		{
			bool isYZ;
			bool isXZ;
			bool isXY;
			bool result = IsPlanar(out isYZ, out isXZ, out isXY);
			if (isYZ)
			{
				ignoreAxis = 0;
				return result;
			}
			if (isXZ)
			{
				ignoreAxis = 1;
				return result;
			}
			ignoreAxis = 2;
			return result;
		}

		public bool IsPlanar(out bool isYZ, out bool isXZ, out bool isXY)
		{
			isYZ = true;
			isXZ = true;
			isXY = true;
			if (ControlPointCount == 0)
			{
				return true;
			}
			Vector3 localPosition = ControlPoints[0].transform.localPosition;
			for (int i = 1; i < ControlPointCount; i++)
			{
				if (!Mathf.Approximately(ControlPoints[i].transform.localPosition.x, localPosition.x))
				{
					isYZ = false;
				}
				if (!Mathf.Approximately(ControlPoints[i].transform.localPosition.y, localPosition.y))
				{
					isXZ = false;
				}
				if (!Mathf.Approximately(ControlPoints[i].transform.localPosition.z, localPosition.z))
				{
					isXY = false;
				}
				if (!isYZ && !isXZ && !isXY)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsPlanar(CurvyPlane plane)
		{
			switch (plane)
			{
			case CurvyPlane.XY:
			{
				for (int j = 0; j < ControlPointCount; j++)
				{
					if (!ControlPoints[j].transform.localPosition.z.Approximately(0f))
					{
						return false;
					}
				}
				break;
			}
			case CurvyPlane.XZ:
			{
				for (int k = 0; k < ControlPointCount; k++)
				{
					if (!ControlPoints[k].transform.localPosition.y.Approximately(0f))
					{
						return false;
					}
				}
				break;
			}
			case CurvyPlane.YZ:
			{
				for (int i = 0; i < ControlPointCount; i++)
				{
					if (!ControlPoints[i].transform.localPosition.x.Approximately(0f))
					{
						return false;
					}
				}
				break;
			}
			}
			return true;
		}

		public void MakePlanar(CurvyPlane plane)
		{
			switch (plane)
			{
			case CurvyPlane.XY:
			{
				for (int j = 0; j < ControlPointCount; j++)
				{
					if (ControlPoints[j].transform.localPosition.z != 0f)
					{
						ControlPoints[j].SetLocalPosition(new Vector3(ControlPoints[j].transform.localPosition.x, ControlPoints[j].transform.localPosition.y, 0f));
					}
				}
				break;
			}
			case CurvyPlane.XZ:
			{
				for (int k = 0; k < ControlPointCount; k++)
				{
					if (ControlPoints[k].transform.localPosition.y != 0f)
					{
						ControlPoints[k].SetLocalPosition(new Vector3(ControlPoints[k].transform.localPosition.x, 0f, ControlPoints[k].transform.localPosition.z));
					}
				}
				break;
			}
			case CurvyPlane.YZ:
			{
				for (int i = 0; i < ControlPointCount; i++)
				{
					if (ControlPoints[i].transform.localPosition.x != 0f)
					{
						ControlPoints[i].SetLocalPosition(new Vector3(0f, ControlPoints[i].transform.localPosition.y, ControlPoints[i].transform.localPosition.z));
					}
				}
				break;
			}
			default:
				throw new NotImplementedException();
			}
			Refresh();
		}

		[UsedImplicitly]
		[Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
		public void MakePlanar(int axis)
		{
			Vector3 localPosition = ControlPoints[0].transform.localPosition;
			for (int i = 1; i < ControlPointCount; i++)
			{
				Vector3 localPosition2 = ControlPoints[i].transform.localPosition;
				switch (axis)
				{
				case 0:
					localPosition2.x = localPosition.x;
					break;
				case 1:
					localPosition2.y = localPosition.y;
					break;
				case 2:
					localPosition2.z = localPosition.z;
					break;
				}
				ControlPoints[i].transform.localPosition = localPosition2;
			}
			SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: true);
			Refresh();
		}

		public void Subdivide(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
		{
			if (!fromCP)
			{
				fromCP = FirstVisibleControlPoint;
			}
			if (!toCP)
			{
				toCP = LastVisibleControlPoint;
			}
			if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
			{
				UnityEngine.Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
				return;
			}
			int num = Mathf.Clamp(fromCP.Spline.GetControlPointIndex(fromCP), 0, ControlPointCount - 2);
			int num2 = Mathf.Clamp(toCP.Spline.GetControlPointIndex(toCP), num + 1, ControlPointCount - 1);
			if (num2 - num < 1)
			{
				UnityEngine.Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
				return;
			}
			OnBeforeControlPointAddEvent(defaultAddAfterEventArgs);
			Dictionary<int, Vector3> dictionary = new Dictionary<int, Vector3>();
			for (int num3 = num2 - 1; num3 >= num; num3--)
			{
				dictionary[num3] = ControlPoints[num3].Interpolate(0.5f);
			}
			for (int num4 = num2 - 1; num4 >= num; num4--)
			{
				CurvySplineSegment curvySplineSegment = ControlPoints[num4];
				CurvySplineSegment curvySplineSegment2 = ControlPoints[num4 + 1];
				CurvySplineSegment curvySplineSegment3 = InsertAfter(ControlPoints[num4], dictionary[num4], skipRefreshingAndEvents: true, Space.Self);
				if (Interpolation == CurvyInterpolation.Bezier)
				{
					Vector3 position = curvySplineSegment.transform.position;
					Vector3 handleOutPosition = curvySplineSegment.HandleOutPosition;
					Vector3 handleInPosition = curvySplineSegment2.HandleInPosition;
					Vector3 position2 = curvySplineSegment2.transform.position;
					Vector3 vector = (position + handleOutPosition) / 2f;
					Vector3 vector2 = (handleOutPosition + handleInPosition) / 2f;
					Vector3 vector3 = (handleInPosition + position2) / 2f;
					Vector3 handleInPosition2 = (vector + vector2) / 2f;
					Vector3 handleOutPosition2 = (vector2 + vector3) / 2f;
					curvySplineSegment.AutoHandles = false;
					curvySplineSegment.HandleOutPosition = vector;
					curvySplineSegment2.AutoHandles = false;
					curvySplineSegment2.HandleInPosition = vector3;
					curvySplineSegment3.AutoHandles = false;
					curvySplineSegment3.HandleInPosition = handleInPosition2;
					curvySplineSegment3.HandleOutPosition = handleOutPosition2;
				}
			}
			Refresh();
			OnAfterControlPointAddEvent(defaultAddAfterEventArgs);
			OnAfterControlPointChangesEvent(defaultSplineEventArgs);
		}

		public void Simplify(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
		{
			if (!fromCP)
			{
				fromCP = FirstVisibleControlPoint;
			}
			if (!toCP)
			{
				toCP = LastVisibleControlPoint;
			}
			if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
			{
				UnityEngine.Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
				return;
			}
			int num = Mathf.Clamp(fromCP.Spline.GetControlPointIndex(fromCP), 0, ControlPointCount - 2);
			int num2 = Mathf.Clamp(toCP.Spline.GetControlPointIndex(toCP), num + 2, ControlPointCount - 1);
			if (num2 - num < 2)
			{
				UnityEngine.Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
				return;
			}
			OnBeforeControlPointDeleteEvent(defaultDeleteEventArgs);
			for (int num3 = num2 - 2; num3 >= num; num3 -= 2)
			{
				Delete(ControlPoints[num3 + 1], skipRefreshingAndEvents: true);
			}
			Refresh();
			OnAfterControlPointChangesEvent(defaultSplineEventArgs);
		}

		public void Equalize(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
		{
			if (!fromCP)
			{
				fromCP = FirstVisibleControlPoint;
			}
			if (!toCP)
			{
				toCP = LastVisibleControlPoint;
			}
			if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
			{
				UnityEngine.Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
				return;
			}
			int num = Mathf.Clamp(GetControlPointIndex(fromCP), 0, ControlPointCount - 2);
			int num2 = Mathf.Clamp(GetControlPointIndex(toCP), num + 2, ControlPointCount - 1);
			if (num2 - num < 2)
			{
				UnityEngine.Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
				return;
			}
			float num3 = (ControlPoints[num2].Distance - ControlPoints[num].Distance) / (float)(num2 - num);
			float distance = ControlPoints[num].Distance;
			Vector3[] array = new Vector3[num2 - num - 1];
			for (int i = num + 1; i < num2; i++)
			{
				int num4 = i - num - 1;
				array[num4] = InterpolateByDistance(distance + (float)(num4 + 1) * num3);
			}
			for (int j = num + 1; j < num2; j++)
			{
				int num5 = j - num - 1;
				ControlPoints[j].SetLocalPosition(array[num5]);
			}
			Refresh();
		}

		public void Normalize()
		{
			Vector3 localScale = base.transform.localScale;
			if (localScale != Vector3.one)
			{
				base.transform.localScale = Vector3.one;
				for (int i = 0; i < ControlPointCount; i++)
				{
					CurvySplineSegment curvySplineSegment = ControlPoints[i];
					curvySplineSegment.SetLocalPosition(Vector3.Scale(curvySplineSegment.transform.localPosition, localScale));
					curvySplineSegment.HandleIn = Vector3.Scale(curvySplineSegment.HandleIn, localScale);
					curvySplineSegment.HandleOut = Vector3.Scale(curvySplineSegment.HandleOut, localScale);
				}
				Refresh();
			}
		}

		public Vector3 SetPivot(float xRel = 0f, float yRel = 0f, float zRel = 0f, bool preview = false)
		{
			Bounds bounds = Bounds;
			Vector3 vector = new Vector3(bounds.min.x + bounds.size.x * ((xRel + 1f) / 2f), bounds.max.y - bounds.size.y * ((yRel + 1f) / 2f), bounds.min.z + bounds.size.z * ((zRel + 1f) / 2f));
			Vector3 vector2 = base.transform.position - vector;
			if (preview)
			{
				return base.transform.position - vector2;
			}
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				ControlPoints[i].transform.position += vector2;
			}
			base.transform.position -= vector2;
			SetDirtyAll(SplineDirtyingType.Everything, base.IsActiveAndEnabled);
			return base.transform.position;
		}

		public void Flip()
		{
			if (ControlPointCount <= 1)
			{
				return;
			}
			switch (Interpolation)
			{
			case CurvyInterpolation.TCB:
			{
				Bias *= -1f;
				for (int num2 = ControlPointCount - 1; num2 >= 0; num2--)
				{
					CurvySplineSegment curvySplineSegment4 = ControlPoints[num2];
					int num3 = num2 - 1;
					if (num3 >= 0)
					{
						CurvySplineSegment curvySplineSegment5 = ControlPoints[num3];
						curvySplineSegment4.EndBias = curvySplineSegment5.StartBias * -1f;
						curvySplineSegment4.EndContinuity = curvySplineSegment5.StartContinuity;
						curvySplineSegment4.EndTension = curvySplineSegment5.StartTension;
						curvySplineSegment4.StartBias = curvySplineSegment5.EndBias * -1f;
						curvySplineSegment4.StartContinuity = curvySplineSegment5.EndContinuity;
						curvySplineSegment4.StartTension = curvySplineSegment5.EndTension;
						curvySplineSegment4.OverrideGlobalBias = curvySplineSegment5.OverrideGlobalBias;
						curvySplineSegment4.OverrideGlobalContinuity = curvySplineSegment5.OverrideGlobalContinuity;
						curvySplineSegment4.OverrideGlobalTension = curvySplineSegment5.OverrideGlobalTension;
						curvySplineSegment4.SynchronizeTCB = curvySplineSegment5.SynchronizeTCB;
					}
				}
				break;
			}
			case CurvyInterpolation.Bezier:
			{
				for (int num = ControlPointCount - 1; num >= 0; num--)
				{
					CurvySplineSegment curvySplineSegment2;
					CurvySplineSegment curvySplineSegment;
					CurvySplineSegment curvySplineSegment3 = (curvySplineSegment2 = (curvySplineSegment = ControlPoints[num]));
					Vector3 handleOut = curvySplineSegment3.HandleOut;
					Vector3 handleIn = curvySplineSegment3.HandleIn;
					Vector3 vector2 = (curvySplineSegment.HandleIn = handleOut);
					vector2 = (curvySplineSegment2.HandleOut = handleIn);
				}
				break;
			}
			}
			ReverseControlPoints();
			Refresh();
		}

		public void MoveControlPoints(int startIndex, int count, CurvySplineSegment destCP)
		{
			if ((bool)destCP && !(this == destCP.Spline) && destCP.Spline.GetControlPointIndex(destCP) != -1)
			{
				startIndex = Mathf.Clamp(startIndex, 0, ControlPointCount - 1);
				count = Mathf.Clamp(count, startIndex, ControlPointCount - startIndex);
				for (int i = 0; i < count; i++)
				{
					CurvySplineSegment curvySplineSegment = ControlPoints[startIndex];
					RemoveControlPoint(curvySplineSegment);
					destCP.Spline.InsertControlPoint(destCP.Spline.GetControlPointIndex(destCP) + i + 1, curvySplineSegment);
					curvySplineSegment.transform.UndoableSetParent(destCP.Spline.transform, worldPositionStays: true, "Move ControlPoints");
				}
				Refresh();
				destCP.Spline.Refresh();
			}
		}

		public void JoinWith(CurvySplineSegment destCP)
		{
			if (!(destCP.Spline == this))
			{
				MoveControlPoints(0, ControlPointCount, destCP);
				base.gameObject.Destroy(isUndoable: true, doPrefabCheck: true);
			}
		}

		public CurvySpline Split(CurvySplineSegment controlPoint)
		{
			CurvySpline curvySpline = Create(this);
			curvySpline.transform.UndoableSetParent(base.transform.parent, worldPositionStays: true, "Split Spline");
			curvySpline.name = base.name + "_parted";
			int segmentIndex = GetSegmentIndex(controlPoint);
			List<CurvySplineSegment> list = new List<CurvySplineSegment>(ControlPointCount - segmentIndex);
			for (int i = segmentIndex; i < ControlPointCount; i++)
			{
				list.Add(ControlPoints[i]);
			}
			for (int j = 0; j < list.Count; j++)
			{
				CurvySplineSegment curvySplineSegment = list[j];
				RemoveControlPoint(curvySplineSegment);
				curvySpline.AddControlPoint(curvySplineSegment, invalidateAndDirty: true, requestSplineToHierarchySynchronization: true);
				curvySplineSegment.transform.UndoableSetParent(curvySpline.transform, worldPositionStays: true, "Split Spline");
			}
			Refresh();
			curvySpline.Refresh();
			return curvySpline;
		}

		public void SetFirstControlPoint(CurvySplineSegment controlPoint)
		{
			short controlPointIndex = GetControlPointIndex(controlPoint);
			CurvySplineSegment[] array = new CurvySplineSegment[controlPointIndex];
			for (int i = 0; i < controlPointIndex; i++)
			{
				array[i] = ControlPoints[i];
			}
			foreach (CurvySplineSegment item in array)
			{
				RemoveControlPoint(item);
				AddControlPoint(item, invalidateAndDirty: true, requestSplineToHierarchySynchronization: true);
			}
			Refresh();
		}

		public bool IsControlPointAnOrientationAnchor(CurvySplineSegment controlPoint)
		{
			return IsControlPointAnOrientationAnchor(IsControlPointVisible(controlPoint), controlPoint.SerializedOrientationAnchor, (object)controlPoint == FirstVisibleControlPoint, (object)controlPoint == LastVisibleControlPoint);
		}

		public bool CanControlPointHaveFollowUp([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			return controlPoint.GetExtrinsicPropertiesINTERNAL().CanHaveFollowUp;
		}

		public short GetControlPointIndex([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			return controlPoint.GetExtrinsicPropertiesINTERNAL().ControlPointIndex;
		}

		public short GetSegmentIndex([NotNull] CurvySplineSegment segment)
		{
			relationshipCache.EnsureIsValid();
			return segment.GetExtrinsicPropertiesINTERNAL().SegmentIndex;
		}

		[CanBeNull]
		public CurvySplineSegment GetNextControlPoint([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			short nextControlPointIndex = controlPoint.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex;
			if (nextControlPointIndex != -1)
			{
				return ControlPoints[nextControlPointIndex];
			}
			return null;
		}

		[CanBeNull]
		public short GetNextControlPointIndex([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			return controlPoint.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex;
		}

		[CanBeNull]
		public CurvySplineSegment GetNextControlPointUsingFollowUp([NotNull] CurvySplineSegment controlPoint)
		{
			if (controlPoint.FollowUp == null || (object)LastVisibleControlPoint != controlPoint)
			{
				return GetNextControlPoint(controlPoint);
			}
			return GetFollowUpHeadingControlPoint(controlPoint.FollowUp, controlPoint.FollowUpHeading);
		}

		[CanBeNull]
		public CurvySplineSegment GetPreviousControlPoint([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			short previousControlPointIndex = controlPoint.GetExtrinsicPropertiesINTERNAL().PreviousControlPointIndex;
			if (previousControlPointIndex != -1)
			{
				return ControlPoints[previousControlPointIndex];
			}
			return null;
		}

		[CanBeNull]
		public short GetPreviousControlPointIndex([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			return controlPoint.GetExtrinsicPropertiesINTERNAL().PreviousControlPointIndex;
		}

		[CanBeNull]
		public CurvySplineSegment GetPreviousControlPointUsingFollowUp([NotNull] CurvySplineSegment controlPoint)
		{
			if (controlPoint.FollowUp == null || (object)FirstVisibleControlPoint != controlPoint)
			{
				return GetPreviousControlPoint(controlPoint);
			}
			return GetFollowUpHeadingControlPoint(controlPoint.FollowUp, controlPoint.FollowUpHeading);
		}

		[CanBeNull]
		public CurvySplineSegment GetNextSegment([NotNull] CurvySplineSegment segment)
		{
			relationshipCache.EnsureIsValid();
			CurvySplineSegment.ControlPointExtrinsicProperties extrinsicPropertiesINTERNAL = segment.GetExtrinsicPropertiesINTERNAL();
			if (!extrinsicPropertiesINTERNAL.NextControlPointIsSegment)
			{
				return null;
			}
			return ControlPoints[extrinsicPropertiesINTERNAL.NextControlPointIndex];
		}

		[CanBeNull]
		public CurvySplineSegment GetPreviousSegment([NotNull] CurvySplineSegment segment)
		{
			relationshipCache.EnsureIsValid();
			CurvySplineSegment.ControlPointExtrinsicProperties extrinsicPropertiesINTERNAL = segment.GetExtrinsicPropertiesINTERNAL();
			if (!extrinsicPropertiesINTERNAL.PreviousControlPointIsSegment)
			{
				return null;
			}
			return ControlPoints[extrinsicPropertiesINTERNAL.PreviousControlPointIndex];
		}

		public bool IsControlPointASegment([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			return controlPoint.GetExtrinsicPropertiesINTERNAL().IsSegment;
		}

		public bool IsControlPointVisible([NotNull] CurvySplineSegment controlPoint)
		{
			relationshipCache.EnsureIsValid();
			return controlPoint.GetExtrinsicPropertiesINTERNAL().IsVisible;
		}

		[UsedImplicitly]
		[Obsolete("No more used, will be removed. If you need this method, you can use IsControlPointAnOrientationAnchor while traversing the spline's control points to find the needed information.")]
		public short GetControlPointOrientationAnchorIndex([NotNull] CurvySplineSegment controlPoint)
		{
			return GetOrientationAnchorIndices()[GetControlPointIndex(controlPoint)];
		}

		public void SetFromString(string fieldAndValue)
		{
			string[] array = fieldAndValue.Split('=');
			if (array.Length != 2)
			{
				return;
			}
			FieldInfo fieldInfo = GetType().FieldByName(array[0], includeInherited: true);
			if (fieldInfo != null)
			{
				try
				{
					if (fieldInfo.FieldType.IsEnum)
					{
						fieldInfo.SetValue(this, Enum.Parse(fieldInfo.FieldType, array[1]));
					}
					else
					{
						fieldInfo.SetValue(this, Convert.ChangeType(array[1], fieldInfo.FieldType, CultureInfo.InvariantCulture));
					}
					return;
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogWarning(base.name + ".SetFromString(): " + ex);
					return;
				}
			}
			PropertyInfo propertyInfo = GetType().PropertyByName(array[0], includeInherited: true);
			if (!(propertyInfo != null))
			{
				return;
			}
			try
			{
				if (propertyInfo.PropertyType.IsEnum)
				{
					propertyInfo.SetValue(this, Enum.Parse(propertyInfo.PropertyType, array[1]), null);
				}
				else
				{
					propertyInfo.SetValue(this, Convert.ChangeType(array[1], propertyInfo.PropertyType, CultureInfo.InvariantCulture), null);
				}
			}
			catch (Exception ex2)
			{
				UnityEngine.Debug.LogWarning(base.name + ".SetFromString(): " + ex2);
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			MaxPointsPerUnit = m_MaxPointsPerUnit;
			AutoEndTangents = m_AutoEndTangents;
			if (base.IsActiveAndEnabled)
			{
				relationshipCache.Invalidate();
				SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: true);
			}
		}

		[UsedImplicitly]
		private void Awake()
		{
			cachedTransform = base.transform;
			if (UsePooling)
			{
				_ = DTSingleton<CurvyGlobalManager>.Instance;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (isStarted)
			{
				if (Initialize())
				{
					OnRefreshEvent(defaultSplineEventArgs);
				}
			}
			else
			{
				SyncSplineFromHierarchy();
			}
		}

		public void Start()
		{
			if (!isStarted)
			{
				bool num = Initialize();
				isStarted = true;
				if (num)
				{
					OnRefreshEvent(defaultSplineEventArgs);
				}
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			mIsInitialized = false;
			ClearControlPoints(invalidateAndDirty: false, requestSplineToHierarchySynchronization: false);
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			if (ShouldUseControlPointPooling(out var curvyGlobalManager))
			{
				PushChildCPsToPool(curvyGlobalManager.ControlPointPool);
			}
			dirtinessManager.Dispose();
			isStarted = false;
		}

		[UsedImplicitly]
		private void Update()
		{
			if (UpdateIn == CurvyUpdateMethod.Update && Application.isPlaying)
			{
				DoUpdate();
			}
		}

		[UsedImplicitly]
		private void LateUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.LateUpdate && Application.isPlaying)
			{
				DoUpdate();
			}
		}

		[UsedImplicitly]
		private void FixedUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.FixedUpdate && Application.isPlaying)
			{
				DoUpdate();
			}
		}

		[MustUseReturnValue]
		private bool Initialize()
		{
			SetDirtyAll(SplineDirtyingType.Everything, dirtyConnectedControlPoints: false);
			relationshipCache.Invalidate();
			SyncSplineFromHierarchy();
			bool result = dirtinessManager.ProcessDirtyControlPoints();
			TransformMonitor.ResetMonitoring();
			mIsInitialized = true;
			if (OnInitialized != null)
			{
				OnInitialized.Invoke(defaultSplineEventArgs);
			}
			return result;
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void HookEditorUpdate()
		{
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void UnhookEditorUpdate()
		{
		}

		private void DoUpdate()
		{
			cpsSynchronizer.ProcessRequests();
			int controlPointCount = ControlPointCount;
			for (int i = 0; i < controlPointCount; i++)
			{
				CurvySplineSegment curvySplineSegment = ControlPoints[i];
				if (curvySplineSegment.AutoBakeOrientation && curvySplineSegment.UpsApproximation.Count > 0)
				{
					curvySplineSegment.BakeOrientationToTransform();
				}
			}
			relationshipCache.EnsureIsValid();
			if (TransformMonitor.CheckForChanges())
			{
				ClearBounds();
			}
			if ((CheckTransform || !Application.isPlaying) && !dirtinessManager.AllControlPointsAreDirty)
			{
				for (int j = 0; j < controlPointCount; j++)
				{
					CurvySplineSegment curvySplineSegment2 = ControlPoints[j];
					bool hasUnprocessedLocalPosition = curvySplineSegment2.HasUnprocessedLocalPosition;
					if (hasUnprocessedLocalPosition || (curvySplineSegment2.HasUnprocessedLocalOrientation && curvySplineSegment2.OrientationInfluencesSpline))
					{
						curvySplineSegment2.Spline.SetDirty(curvySplineSegment2, hasUnprocessedLocalPosition ? SplineDirtyingType.Everything : SplineDirtyingType.OrientationOnly);
					}
				}
			}
			if (Dirty)
			{
				Refresh();
			}
			else if (sendOnRefreshEventNextUpdate)
			{
				OnRefreshEvent(defaultSplineEventArgs);
			}
			sendOnRefreshEventNextUpdate = false;
			if (TransformMonitor.HasChanged && OnGlobalCoordinatesChanged != null)
			{
				OnGlobalCoordinatesChanged(this);
			}
		}

		private void ClearBounds()
		{
			mBounds = null;
			int controlPointCount = ControlPointCount;
			for (int i = 0; i < controlPointCount; i++)
			{
				ControlPoints[i].ClearBoundsINTERNAL();
			}
		}

		private bool CanHaveManualEndCp()
		{
			if (!Closed)
			{
				if (Interpolation != CurvyInterpolation.CatmullRom)
				{
					return Interpolation == CurvyInterpolation.TCB;
				}
				return true;
			}
			return false;
		}

		private bool CanBeClamped()
		{
			if (!Closed)
			{
				return Interpolation == CurvyInterpolation.BSpline;
			}
			return false;
		}

		private void ReverseControlPoints()
		{
			ControlPoints.Reverse();
			relationshipCache.Invalidate();
			SetDirtyAll(SplineDirtyingType.Everything, base.IsActiveAndEnabled);
		}

		private static short GetNextControlPointIndex(short controlPointIndex, bool isSplineClosed, int controlPointsCount)
		{
			if (isSplineClosed && controlPointsCount <= 1)
			{
				return -1;
			}
			if (controlPointIndex + 1 < controlPointsCount)
			{
				return (short)(controlPointIndex + 1);
			}
			return (short)((!isSplineClosed) ? (-1) : 0);
		}

		private static short GetPreviousControlPointIndex(short controlPointIndex, bool isSplineClosed, int controlPointsCount)
		{
			if (isSplineClosed && controlPointsCount <= 1)
			{
				return -1;
			}
			if (controlPointIndex - 1 >= 0)
			{
				return (short)(controlPointIndex - 1);
			}
			return (short)(isSplineClosed ? (controlPointsCount - 1) : (-1));
		}

		private static bool IsControlPointASegment(int controlPointIndex, int controlPointCount, bool isClosed, bool notAutoEndTangentsAndIsCatmullRomOrTCB, bool isBSpline, int bSplineDegree)
		{
			if (!isBSpline || bSplineDegree < controlPointCount)
			{
				if (!isClosed || controlPointCount <= 1)
				{
					if (!notAutoEndTangentsAndIsCatmullRomOrTCB)
					{
						return controlPointIndex < controlPointCount - 1;
					}
					if (controlPointIndex > 0)
					{
						return controlPointIndex < controlPointCount - 2;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsControlPointAnOrientationAnchor(bool isVisible, bool isSerializedOrientationAnchor, bool isFirstVisibleControlPoint, bool isLastVisibleControlPoint)
		{
			if (!(isFirstVisibleControlPoint || isLastVisibleControlPoint))
			{
				return isSerializedOrientationAnchor && isVisible;
			}
			return true;
		}

		private void AddControlPoint([NotNull] CurvySplineSegment item, bool invalidateAndDirty, bool requestSplineToHierarchySynchronization)
		{
			ControlPoints.Add(item);
			item.LinkToSpline(this);
			if (invalidateAndDirty)
			{
				relationshipCache.Invalidate();
				short previousControlPointIndex = GetPreviousControlPointIndex((short)(ControlPoints.Count - 1), Closed, ControlPoints.Count);
				short nextControlPointIndex = GetNextControlPointIndex((short)(ControlPoints.Count - 1), Closed, ControlPoints.Count);
				dirtinessManager.SetDirty(item, SplineDirtyingType.Everything, (previousControlPointIndex != -1) ? ControlPoints[previousControlPointIndex] : null, (nextControlPointIndex != -1) ? ControlPoints[nextControlPointIndex] : null, ignoreConnectionOfInputControlPoint: false);
			}
		}

		private void InsertControlPoint(int index, CurvySplineSegment item)
		{
			ControlPoints.Insert(index, item);
			item.LinkToSpline(this);
			relationshipCache.Invalidate();
			short previousControlPointIndex = GetPreviousControlPointIndex((short)index, Closed, ControlPoints.Count);
			short nextControlPointIndex = GetNextControlPointIndex((short)index, Closed, ControlPoints.Count);
			dirtinessManager.SetDirty(item, SplineDirtyingType.Everything, (previousControlPointIndex == -1) ? null : ControlPoints[previousControlPointIndex], (nextControlPointIndex == -1) ? null : ControlPoints[nextControlPointIndex], ignoreConnectionOfInputControlPoint: false);
		}

		private void RemoveControlPoint(CurvySplineSegment item)
		{
			int controlPointIndex = GetControlPointIndex(item);
			if (ControlPoints.Count == 1)
			{
				SetDirtyAll(SplineDirtyingType.Everything, base.IsActiveAndEnabled);
			}
			else
			{
				short previousControlPointIndex = GetPreviousControlPointIndex((short)controlPointIndex, Closed, ControlPoints.Count);
				short nextControlPointIndex = GetNextControlPointIndex((short)controlPointIndex, Closed, ControlPoints.Count);
				if (previousControlPointIndex != -1)
				{
					SetDirty(ControlPoints[previousControlPointIndex], SplineDirtyingType.Everything);
				}
				if (nextControlPointIndex != -1)
				{
					SetDirty(ControlPoints[nextControlPointIndex], SplineDirtyingType.Everything);
				}
			}
			ControlPoints.RemoveAt(controlPointIndex);
			dirtinessManager.RemoveFromMinimalSet(item);
			item.UnlinkFromSpline(this);
			relationshipCache.Invalidate();
		}

		private void ClearControlPoints(bool invalidateAndDirty, bool requestSplineToHierarchySynchronization)
		{
			if (invalidateAndDirty)
			{
				SetDirtyAll(SplineDirtyingType.Everything, base.IsActiveAndEnabled);
			}
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				CurvySplineSegment curvySplineSegment = ControlPoints[i];
				if ((bool)curvySplineSegment)
				{
					curvySplineSegment.UnlinkFromSpline(this);
				}
			}
			ControlPoints.Clear();
			dirtinessManager.ClearMinimalSet();
			if (invalidateAndDirty)
			{
				relationshipCache.Invalidate();
			}
		}

		[UsedImplicitly]
		[Obsolete]
		internal void InvalidateControlPointsRelationshipCacheINTERNAL()
		{
			relationshipCache.Invalidate();
		}

		[UsedImplicitly]
		private void UpdateControlPointDistances()
		{
			int count = ControlPoints.Count;
			Array.Resize(ref controlPointsDistances, count);
			float[] array = controlPointsDistances;
			float num2 = (ControlPoints[0].Distance = 0f);
			array[0] = num2;
			for (int i = 1; i < count; i++)
			{
				float[] array2 = controlPointsDistances;
				int num3 = i;
				num2 = (ControlPoints[i].Distance = ControlPoints[i - 1].Distance + ControlPoints[i - 1].Length);
				array2[num3] = num2;
			}
		}

		private void EnforceTangentContinuity()
		{
			List<CurvySplineSegment> segments = Segments;
			int count = segments.Count;
			for (int i = 0; i < count; i++)
			{
				CurvySplineSegment curvySplineSegment = segments[i];
				CurvySplineSegment nextSegment = GetNextSegment(curvySplineSegment);
				if ((bool)nextSegment)
				{
					curvySplineSegment.TangentsApproximation.Array[curvySplineSegment.CacheSize] = nextSegment.TangentsApproximation.Array[0];
				}
				else
				{
					GetNextControlPoint(curvySplineSegment).TangentsApproximation.Array[0] = curvySplineSegment.TangentsApproximation.Array[curvySplineSegment.CacheSize];
				}
			}
		}

		private void PrepareThreadCompatibleData()
		{
			int controlPointCount = ControlPointCount;
			bool useFollowUp = Interpolation == CurvyInterpolation.CatmullRom || Interpolation == CurvyInterpolation.TCB;
			for (int i = 0; i < controlPointCount; i++)
			{
				ControlPoints[i].PrepareThreadCompatibleDataINTERNAL(useFollowUp);
			}
			if (Count > 0)
			{
				CurvySplineSegment previousControlPointUsingFollowUp = GetPreviousControlPointUsingFollowUp(FirstVisibleControlPoint);
				if ((object)previousControlPointUsingFollowUp != null && previousControlPointUsingFollowUp.Spline != this)
				{
					previousControlPointUsingFollowUp.PrepareThreadCompatibleDataINTERNAL(useFollowUp);
				}
				CurvySplineSegment nextControlPointUsingFollowUp = GetNextControlPointUsingFollowUp(LastVisibleControlPoint);
				if ((object)nextControlPointUsingFollowUp != null && nextControlPointUsingFollowUp.Spline != this)
				{
					nextControlPointUsingFollowUp.PrepareThreadCompatibleDataINTERNAL(useFollowUp);
				}
			}
		}

		private short[] GetOrientationAnchorIndices()
		{
			int count = ControlPoints.Count;
			if (count <= 0)
			{
				return Array.Empty<short>();
			}
			if (cachedShortsArray.Length < count)
			{
				Array.Resize(ref cachedShortsArray, count);
			}
			CurvySplineSegment firstVisibleControlPoint = relationshipCache.FirstVisibleControlPoint;
			CurvySplineSegment lastVisibleControlPoint = relationshipCache.LastVisibleControlPoint;
			short num = -1;
			for (short num2 = 0; num2 < count; num2++)
			{
				CurvySplineSegment curvySplineSegment = ControlPoints[num2];
				bool isVisible = curvySplineSegment.GetExtrinsicPropertiesINTERNAL().IsVisible;
				bool num3 = IsControlPointAnOrientationAnchor(isVisible, curvySplineSegment.SerializedOrientationAnchor, (object)curvySplineSegment == firstVisibleControlPoint, (object)curvySplineSegment == lastVisibleControlPoint);
				short num4 = (short)((!num3) ? (isVisible ? num : (-1)) : num2);
				cachedShortsArray[num2] = num4;
				if (num3)
				{
					num = num2;
				}
			}
			return cachedShortsArray;
		}

		private void InvalidateAccumulators()
		{
			mCacheSize = -1;
			length = -1f;
			mBounds = null;
		}

		internal void NotifyMetaDataModification()
		{
			sendOnRefreshEventNextUpdate = true;
		}

		private void DisposeOfControlPoint(CurvySplineSegment controlPoint, bool isUndoable)
		{
			if (ShouldUseControlPointPooling(out var curvyGlobalManager))
			{
				curvyGlobalManager.ControlPointPool.Push(controlPoint);
			}
			else
			{
				controlPoint.gameObject.Destroy(isUndoable, doPrefabCheck: true);
			}
		}

		private bool ShouldUseControlPointPooling(out CurvyGlobalManager curvyGlobalManager)
		{
			if (!UsePooling || !Application.isPlaying)
			{
				curvyGlobalManager = null;
				return false;
			}
			CurvyGlobalManager instance = DTSingleton<CurvyGlobalManager>.Instance;
			if (instance == null)
			{
				curvyGlobalManager = null;
				return false;
			}
			curvyGlobalManager = instance;
			return true;
		}

		private CurvySplineSegment InsertAt([CanBeNull] CurvySplineSegment beforeEventCP, Vector3 position, int insertionIndex, CurvyControlPointEventArgs.ModeEnum insertionMode, bool skipRefreshingAndEvents, Space space)
		{
			if (!skipRefreshingAndEvents)
			{
				OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this, this, beforeEventCP, insertionMode));
			}
			CurvySplineSegment curvySplineSegment = AcquireNewControlPoint();
			curvySplineSegment.gameObject.layer = base.gameObject.layer;
			InsertControlPoint(insertionIndex, curvySplineSegment);
			curvySplineSegment.AutoHandleDistance = AutoHandleDistance;
			curvySplineSegment.transform.SetParent(cachedTransform);
			if (space == Space.World)
			{
				curvySplineSegment.transform.position = position;
			}
			else
			{
				curvySplineSegment.transform.localPosition = position;
			}
			curvySplineSegment.transform.localRotation = Quaternion.identity;
			curvySplineSegment.transform.localScale = Vector3.one;
			if (!skipRefreshingAndEvents)
			{
				Refresh();
				OnAfterControlPointAddEvent(new CurvyControlPointEventArgs(this, this, curvySplineSegment, insertionMode));
				OnAfterControlPointChangesEvent(defaultSplineEventArgs);
			}
			return curvySplineSegment;
		}

		[NotNull]
		private CurvySplineSegment AcquireNewControlPoint()
		{
			if (!ShouldUseControlPointPooling(out var curvyGlobalManager))
			{
				return new GameObject("NewCP", typeof(CurvySplineSegment)).GetComponent<CurvySplineSegment>();
			}
			return (CurvySplineSegment)curvyGlobalManager.ControlPointPool.Pop();
		}

		private SubArray<Vector3> GetSegmentApproximationsInSpace([NotNull] Func<CurvySplineSegment, SubArray<Vector3>> approximationGetter, Space space)
		{
			SubArray<Vector3> subArray = ConcatenateSegmentApproximations(approximationGetter);
			if (space == Space.World)
			{
				TransformToWorldSpace(subArray);
			}
			return subArray;
		}

		private SubArray<Vector3> ConcatenateSegmentApproximations([NotNull] Func<CurvySplineSegment, SubArray<Vector3>> approximationGetter)
		{
			SubArray<Vector3> result = ArrayPools.Vector3.Allocate(CacheSize + 1);
			Vector3[] array = result.Array;
			int num = 0;
			for (int i = 0; i < Count; i++)
			{
				SubArray<Vector3> subArray = approximationGetter(this[i]);
				Array.Copy(subArray.Array, 0, array, num, subArray.Count);
				num += Mathf.Max(0, subArray.Count - 1);
			}
			return result;
		}

		private void TransformToWorldSpace(SubArray<Vector3> localSpaceVectors)
		{
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			Vector3[] array = localSpaceVectors.Array;
			for (int i = 0; i < localSpaceVectors.Count; i++)
			{
				array[i] = localToWorldMatrix.MultiplyPoint3x4(array[i]);
			}
		}

		private void PushChildCPsToPool([NotNull] ComponentPool controlPointPool)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				CurvySplineSegment component = base.transform.GetChild(i).GetComponent<CurvySplineSegment>();
				if (!(component == null))
				{
					controlPointPool.Push(component);
				}
			}
		}

		private CurvySplineEventArgs OnRefreshEvent(CurvySplineEventArgs e)
		{
			if (OnRefresh != null)
			{
				OnRefresh.Invoke(e);
			}
			return e;
		}

		private CurvyControlPointEventArgs OnBeforeControlPointAddEvent(CurvyControlPointEventArgs e)
		{
			if (OnBeforeControlPointAdd != null)
			{
				OnBeforeControlPointAdd.Invoke(e);
			}
			return e;
		}

		private CurvyControlPointEventArgs OnAfterControlPointAddEvent(CurvyControlPointEventArgs e)
		{
			if (OnAfterControlPointAdd != null)
			{
				OnAfterControlPointAdd.Invoke(e);
			}
			return e;
		}

		private CurvyControlPointEventArgs OnBeforeControlPointDeleteEvent(CurvyControlPointEventArgs e)
		{
			if (OnBeforeControlPointDelete != null)
			{
				OnBeforeControlPointDelete.Invoke(e);
			}
			return e;
		}

		private CurvySplineEventArgs OnAfterControlPointChangesEvent(CurvySplineEventArgs e)
		{
			if (OnAfterControlPointChanges != null)
			{
				OnAfterControlPointChanges.Invoke(e);
			}
			return e;
		}

		protected override void ResetOnEnable()
		{
			base.ResetOnEnable();
			relationshipCache.Invalidate();
			cpsSynchronizer.CancelRequests();
			controlPointsDistances = Array.Empty<float>();
			dirtinessManager.Reset();
			InvalidateAccumulators();
			sendOnRefreshEventNextUpdate = false;
		}
	}
}
