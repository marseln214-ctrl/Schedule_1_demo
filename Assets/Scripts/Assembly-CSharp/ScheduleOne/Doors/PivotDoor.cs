using UnityEngine;

namespace ScheduleOne.Doors
{
	public class PivotDoor : MonoBehaviour
	{
		[Header("Settings")]
		public Transform DoorTransform;

		public bool FlipSide;

		public float OpenInwardsAngle = -100f;

		public float OpenOutwardsAngle = 100f;

		public float SwingSpeed = 5f;

		private float targetDoorAngle;

		protected virtual void Awake()
		{
		}

		private void LateUpdate()
		{
			DoorTransform.localRotation = Quaternion.Lerp(DoorTransform.localRotation, Quaternion.Euler(0f, targetDoorAngle, 0f), Time.deltaTime * SwingSpeed);
		}

		public virtual void Opened(EDoorSide openSide)
		{
			switch (openSide)
			{
			case EDoorSide.Interior:
				targetDoorAngle = (FlipSide ? OpenInwardsAngle : OpenOutwardsAngle);
				break;
			case EDoorSide.Exterior:
				targetDoorAngle = (FlipSide ? OpenOutwardsAngle : OpenInwardsAngle);
				break;
			}
		}

		public virtual void Closed()
		{
			targetDoorAngle = 0f;
		}
	}
}
