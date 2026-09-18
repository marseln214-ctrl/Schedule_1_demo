using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.ImportExport
{
	[Serializable]
	public class SerializedCurvySplineSegment
	{
		public Vector3 Position;

		public Vector3 Rotation;

		public bool AutoBakeOrientation;

		public bool OrientationAnchor;

		public CurvyOrientationSwirl Swirl;

		public float SwirlTurns;

		public bool AutoHandles;

		public bool SynchronizeTCB;

		public float AutoHandleDistance;

		public Vector3 HandleOut;

		public Vector3 HandleIn;

		public SerializedCurvySplineSegment()
		{
			Swirl = CurvyOrientationSwirl.None;
			SynchronizeTCB = true;
			AutoHandles = true;
			AutoHandleDistance = 0.39f;
			HandleOut = CurvySplineSegmentDefaultValues.HandleOut;
			HandleIn = CurvySplineSegmentDefaultValues.HandleIn;
		}

		public SerializedCurvySplineSegment([NotNull] CurvySplineSegment segment, CurvySerializationSpace space)
		{
			Position = ((space == CurvySerializationSpace.Global) ? segment.transform.position : segment.transform.localPosition);
			Rotation = ((space == CurvySerializationSpace.Global) ? segment.transform.rotation.eulerAngles : segment.transform.localRotation.eulerAngles);
			AutoBakeOrientation = segment.AutoBakeOrientation;
			OrientationAnchor = segment.SerializedOrientationAnchor;
			Swirl = segment.Swirl;
			SwirlTurns = segment.SwirlTurns;
			AutoHandles = segment.AutoHandles;
			SynchronizeTCB = segment.SynchronizeTCB;
			AutoHandleDistance = segment.AutoHandleDistance;
			HandleOut = segment.HandleOut;
			HandleIn = segment.HandleIn;
		}

		public void WriteIntoControlPoint([NotNull] CurvySplineSegment controlPoint, CurvySerializationSpace space)
		{
			if (space == CurvySerializationSpace.Global)
			{
				controlPoint.transform.position = Position;
				controlPoint.transform.rotation = Quaternion.Euler(Rotation);
			}
			else
			{
				controlPoint.transform.localPosition = Position;
				controlPoint.transform.localRotation = Quaternion.Euler(Rotation);
			}
			controlPoint.AutoBakeOrientation = AutoBakeOrientation;
			controlPoint.SerializedOrientationAnchor = OrientationAnchor;
			controlPoint.Swirl = Swirl;
			controlPoint.SynchronizeTCB = SynchronizeTCB;
			controlPoint.SwirlTurns = SwirlTurns;
			controlPoint.AutoHandles = AutoHandles;
			controlPoint.AutoHandleDistance = AutoHandleDistance;
			controlPoint.SetBezierHandleIn(HandleIn);
			controlPoint.SetBezierHandleOut(HandleOut);
		}
	}
}
