using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[RequireComponent(typeof(CurvySplineSegment))]
	[ExecuteAlways]
	public abstract class CurvyMetadataBase : DTVersionedMonoBehaviour
	{
		private CurvySplineSegment mCP;

		public CurvySplineSegment ControlPoint => mCP;

		public CurvySpline Spline
		{
			get
			{
				if (!mCP)
				{
					return null;
				}
				return mCP.Spline;
			}
		}

		protected virtual void Awake()
		{
			mCP = GetComponent<CurvySplineSegment>();
			mCP.RegisterMetaData(this);
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			mCP.UnregisterMetaData(this);
		}

		public T GetPreviousData<T>(bool autoCreate = true, bool segmentsOnly = true, bool useFollowUp = false) where T : CurvyMetadataBase
		{
			if ((bool)ControlPoint)
			{
				CurvySplineSegment controlPoint = ControlPoint;
				CurvySpline spline = Spline;
				CurvySplineSegment curvySplineSegment;
				if (!spline || spline.ControlPointsList.Count == 0)
				{
					curvySplineSegment = null;
				}
				else
				{
					curvySplineSegment = (useFollowUp ? spline.GetPreviousControlPointUsingFollowUp(controlPoint) : spline.GetPreviousControlPoint(controlPoint));
					if (segmentsOnly && (bool)curvySplineSegment && !curvySplineSegment.Spline.IsControlPointASegment(curvySplineSegment))
					{
						curvySplineSegment = null;
					}
				}
				if ((bool)curvySplineSegment)
				{
					return curvySplineSegment.GetMetadata<T>(autoCreate);
				}
			}
			return null;
		}

		public T GetNextData<T>(bool autoCreate = true, bool segmentsOnly = true, bool useFollowUp = false) where T : CurvyMetadataBase
		{
			if ((bool)ControlPoint)
			{
				CurvySplineSegment controlPoint = ControlPoint;
				CurvySpline spline = Spline;
				CurvySplineSegment curvySplineSegment;
				if (!spline || spline.ControlPointsList.Count == 0)
				{
					curvySplineSegment = null;
				}
				else
				{
					curvySplineSegment = (useFollowUp ? spline.GetNextControlPointUsingFollowUp(controlPoint) : spline.GetNextControlPoint(controlPoint));
					if (segmentsOnly && (bool)curvySplineSegment && !curvySplineSegment.Spline.IsControlPointASegment(curvySplineSegment))
					{
						curvySplineSegment = null;
					}
				}
				if ((bool)curvySplineSegment)
				{
					return curvySplineSegment.GetMetadata<T>(autoCreate);
				}
			}
			return null;
		}

		protected void NotifyModification()
		{
			CurvySpline spline = Spline;
			if ((bool)spline && spline.IsInitialized)
			{
				spline.NotifyMetaDataModification();
			}
		}
	}
}
