using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[HelpURL("https://curvyeditor.com/doclink/nearestsplinepoint")]
	[AddComponentMenu("Curvy/Misc/Nearest Spline Point")]
	[ExecuteAlways]
	public class NearestSplinePoint : DTVersionedMonoBehaviour
	{
		[Tooltip("The spline on which the nearest position is searched for")]
		public CurvySpline Spline;

		[Tooltip("A transform which position will be used as the input position for the lookup")]
		public Transform SourcePosition;

		[Tooltip("A transform which position will be updated with the nearest point on Spline to Source Position")]
		public Transform TargetPosition;

		[Tooltip("When to run the lookup")]
		public CurvyUpdateMethod UpdateIn;

		[Tooltip("At each update, this event is called with the result of the lookup")]
		public UnityEventEx<Vector3> OnUpdated = new UnityEventEx<Vector3>();

		private void Process()
		{
			if ((bool)SourcePosition && (bool)Spline && Spline.IsInitialized && !Spline.Dirty)
			{
				Vector3 nearestPoint = Spline.GetNearestPoint(SourcePosition.position, Space.World);
				if ((bool)TargetPosition)
				{
					TargetPosition.position = nearestPoint;
				}
				OnUpdated?.Invoke(nearestPoint);
			}
		}

		[UsedImplicitly]
		private void Update()
		{
			if (UpdateIn == CurvyUpdateMethod.Update)
			{
				Process();
			}
		}

		[UsedImplicitly]
		private void LateUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.LateUpdate || (!Application.isPlaying && UpdateIn == CurvyUpdateMethod.FixedUpdate))
			{
				Process();
			}
		}

		[UsedImplicitly]
		private void FixedUpdate()
		{
			if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
			{
				Process();
			}
		}
	}
}
