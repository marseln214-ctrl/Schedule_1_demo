using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.ImportExport
{
	[Serializable]
	public class SerializedCurvySpline
	{
		public string Name;

		public Vector3 Position;

		public Vector3 Rotation;

		public CurvyInterpolation Interpolation;

		public bool RestrictTo2D;

		public bool Closed;

		public bool AutoEndTangents;

		public CurvyOrientation Orientation;

		public float AutoHandleDistance;

		public int CacheDensity;

		public float MaxPointsPerUnit;

		public bool UsePooling;

		public bool UseThreading;

		public bool CheckTransform;

		public CurvyUpdateMethod UpdateIn;

		public bool IsBSplineClamped;

		public int BSplineDegree;

		public SerializedCurvySplineSegment[] ControlPoints;

		public SerializedCurvySpline()
		{
			Interpolation = CurvyGlobalManager.DefaultInterpolation;
			AutoEndTangents = true;
			Orientation = CurvyOrientation.Dynamic;
			AutoHandleDistance = 0.39f;
			CacheDensity = 50;
			MaxPointsPerUnit = 8f;
			UsePooling = true;
			CheckTransform = true;
			UpdateIn = CurvyUpdateMethod.Update;
			BSplineDegree = 2;
			IsBSplineClamped = true;
			ControlPoints = new SerializedCurvySplineSegment[0];
		}

		public SerializedCurvySpline([NotNull] CurvySpline spline, CurvySerializationSpace space)
		{
			Name = spline.name;
			Position = ((space == CurvySerializationSpace.Local) ? spline.transform.localPosition : spline.transform.position);
			Rotation = ((space == CurvySerializationSpace.Local) ? spline.transform.localRotation.eulerAngles : spline.transform.rotation.eulerAngles);
			Interpolation = spline.Interpolation;
			RestrictTo2D = spline.RestrictTo2D;
			Closed = spline.Closed;
			AutoEndTangents = spline.AutoEndTangents;
			Orientation = spline.Orientation;
			AutoHandleDistance = spline.AutoHandleDistance;
			CacheDensity = spline.CacheDensity;
			MaxPointsPerUnit = spline.MaxPointsPerUnit;
			UsePooling = spline.UsePooling;
			UseThreading = spline.UseThreading;
			CheckTransform = spline.CheckTransform;
			UpdateIn = spline.UpdateIn;
			BSplineDegree = spline.BSplineDegree;
			IsBSplineClamped = spline.IsBSplineClamped;
			ControlPoints = new SerializedCurvySplineSegment[spline.ControlPointCount];
			for (int i = 0; i < spline.ControlPointCount; i++)
			{
				ControlPoints[i] = new SerializedCurvySplineSegment(spline.ControlPointsList[i], space);
			}
		}

		public void WriteIntoSpline([NotNull] CurvySpline deserializedSpline, CurvySerializationSpace space)
		{
			deserializedSpline.name = Name;
			if (space == CurvySerializationSpace.Local)
			{
				deserializedSpline.transform.localPosition = Position;
				deserializedSpline.transform.localRotation = Quaternion.Euler(Rotation);
			}
			else
			{
				deserializedSpline.transform.position = Position;
				deserializedSpline.transform.rotation = Quaternion.Euler(Rotation);
			}
			deserializedSpline.Interpolation = Interpolation;
			deserializedSpline.RestrictTo2D = RestrictTo2D;
			deserializedSpline.Closed = Closed;
			deserializedSpline.AutoEndTangents = AutoEndTangents;
			deserializedSpline.Orientation = Orientation;
			deserializedSpline.AutoHandleDistance = AutoHandleDistance;
			deserializedSpline.CacheDensity = CacheDensity;
			deserializedSpline.MaxPointsPerUnit = MaxPointsPerUnit;
			deserializedSpline.UsePooling = UsePooling;
			deserializedSpline.UseThreading = UseThreading;
			deserializedSpline.CheckTransform = CheckTransform;
			deserializedSpline.UpdateIn = UpdateIn;
			SerializedCurvySplineSegment[] controlPoints = ControlPoints;
			for (int i = 0; i < controlPoints.Length; i++)
			{
				controlPoints[i].WriteIntoControlPoint(deserializedSpline.InsertAfter(null, skipRefreshingAndEvents: true), space);
			}
			deserializedSpline.BSplineDegree = BSplineDegree;
			deserializedSpline.IsBSplineClamped = IsBSplineClamped;
			deserializedSpline.SetDirtyAll();
		}
	}
}
