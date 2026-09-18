using FluffyUnderware.Curvy.Controllers;
using JetBrains.Annotations;
using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
	[ExecuteAlways]
	public class E13_TrainCarDrifter : MonoBehaviour
	{
		public float speed = 30f;

		public float wheelSpacing = 9.72f;

		public Vector3 bodyOffset = new Vector3(0f, 1f, 0f);

		public SplineController controllerWheelLeading;

		public SplineController controllerWheelTrailing;

		public Transform trainCar;

		[UsedImplicitly]
		private void Start()
		{
			controllerWheelLeading.Speed = speed;
		}

		[UsedImplicitly]
		private void Update()
		{
			if ((bool)controllerWheelLeading && (bool)controllerWheelTrailing && (bool)controllerWheelLeading.Spline && (bool)controllerWheelTrailing.Spline && controllerWheelLeading.Spline != controllerWheelTrailing.Spline && (bool)trainCar)
			{
				Vector3 localPosition = controllerWheelTrailing.Spline.transform.InverseTransformPoint(controllerWheelLeading.transform.position);
				Vector3 nearestPoint;
				float nearestPointTF = controllerWheelTrailing.Spline.GetNearestPointTF(localPosition, out nearestPoint);
				controllerWheelTrailing.RelativePosition = nearestPointTF;
				float num = Vector3.Distance(controllerWheelLeading.transform.position, nearestPoint);
				float num2 = Mathf.Clamp(Mathf.Sqrt(wheelSpacing * wheelSpacing - num * num), 0f, 20f);
				controllerWheelTrailing.AbsolutePosition -= num2;
				trainCar.position = (controllerWheelLeading.transform.position + controllerWheelTrailing.transform.position) / 2f + bodyOffset;
				Vector3 worldPosition = new Vector3(controllerWheelLeading.transform.position.x, trainCar.transform.position.y, controllerWheelLeading.transform.position.z);
				trainCar.LookAt(worldPosition, controllerWheelLeading.transform.up);
			}
		}
	}
}
